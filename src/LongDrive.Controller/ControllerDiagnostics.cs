using System;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LongDrive.Controller
{
    // Opt-in only. Virtual input is read directly here, never through the gameplay
    // hub, and every virtual device is detached before normal polling resumes.
    internal static class ControllerDiagnostics
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VirtualDescription
        {
            public ushort Version, Type, Axes, Buttons, Hats, Vendor, Product, Padding;
            public uint ButtonMask, AxisMask;
            public IntPtr Name, User, Update, PlayerIndex, Rumble, TriggerRumble, Led, Effect;
        }
        [DllImport(SdlNative.Dll, CallingConvention = CallingConvention.Cdecl)] private static extern int SDL_JoystickAttachVirtualEx(ref VirtualDescription description);
        [DllImport(SdlNative.Dll, CallingConvention = CallingConvention.Cdecl)] private static extern int SDL_JoystickDetachVirtual(int index);
        [DllImport(SdlNative.Dll, CallingConvention = CallingConvention.Cdecl)] private static extern int SDL_JoystickSetVirtualButton(IntPtr joystick, int button, byte value);
        [DllImport(SdlNative.Dll, CallingConvention = CallingConvention.Cdecl)] private static extern int SDL_JoystickSetVirtualAxis(IntPtr joystick, int axis, short value);
        [DllImport(SdlNative.Dll, CallingConvention = CallingConvention.Cdecl)] private static extern int SDL_JoystickSetVirtualHat(IntPtr joystick, int hat, byte value);
        private static int checks;
        private static void Require(bool result, string description)
        {
            if (!result) throw new InvalidOperationException(description + "; SDL: " + SdlNative.Error);
            checks++;
        }
        internal static void Run(ManualLogSource log)
        {
            checks = 0;
            try
            {
                Require(SdlNative.Initialize(), "SDL runtime initialization");
                Require(Marshal.SizeOf(typeof(VirtualDescription)) == 88, "Windows x64 virtual-device ABI");
                Probe(0x045e, 0x028e, "Xbox virtual probe", ControllerFamily.Xbox, false);
                Probe(0x054c, 0x09cc, "DualShock virtual probe", ControllerFamily.PlayStation, false);
                Probe(0x057e, 0x2009, "Switch virtual probe", ControllerFamily.Nintendo, false);
                Probe(0x1234, 0x9876, "Unidentified virtual probe", ControllerFamily.PlayStation, false);
                Probe(0x1234, 0x9875, "Unmapped virtual probe", ControllerFamily.PlayStation, true);
                Probe(0x1234, 0x9874, "Four-axis unmapped virtual probe", ControllerFamily.PlayStation, true, 4);
                VerifySwitching();
                VerifyLabels();
                VerifySavedPreferences();
                VerifyGameSteering();
                Require(PointerDrivePatch.SteeringGuards == 1, "Native steering duplicate guard installed in actual DriveStuff method");
                log.LogInfo("Controller diagnostics PASS: " + checks + " native virtual-input and UI-family checks; no gameplay input generated.");
            }
            catch (Exception ex) { log.LogError("Controller diagnostics FAILED after " + checks + " checks: " + ex); }
        }
        private static inputscript steeringFixture;
        private static float steeringFixtureValue;
        [HarmonyPriority(Priority.Last)]
        private static void SupplyFixtureAxis(inputscript __instance, inputscript.IN _input, ref float __result)
        {
            if (__instance != steeringFixture) return;
            __result = _input == inputscript.IN.steerright ? Mathf.Max(0, steeringFixtureValue) :
                _input == inputscript.IN.steerleft ? Mathf.Max(0, -steeringFixtureValue) : 0;
        }
        private static void VerifyGameSteering()
        {
            // Exercise the real DriveStuff implementation on an inactive component.
            // Only this fixture's getter is supplied synthetic input. A private copy
            // of native settings is restored before returning; no live input changes.
            Require(settingsscript.s != null && settingsscript.s.S != null && mainscript.s != null, "Native steering fixture prerequisites");
            var originalSettings = settingsscript.s.S;
            var originalInput = inputscript.i;
            var hook = new Harmony("ashley.thelongdrive.controller.steering-fixture");
            GameObject root = null;
            UnityEngine.UI.Text[] labels = { mainscript.s.menu.TSteer, mainscript.s.menu.TThrottle,
                mainscript.s.menu.TBrake, mainscript.s.menu.TClutch, mainscript.s.menu.THandBrake };
            var captions = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++) captions[i] = labels[i] != null ? labels[i].text : null;
            try
            {
                settingsscript.s.S = JsonUtility.FromJson<settingsscript.setting>(JsonUtility.ToJson(originalSettings));
                settingsscript.s.S.CBenableSteer = false;
                root = new GameObject("Inactive steering method fixture"); root.SetActive(false);
                steeringFixture = root.AddComponent<inputscript>();
                Require(inputscript.i == originalInput, "Inactive fixture does not replace live input singleton");
                hook.Patch(AccessTools.Method(typeof(inputscript), "GetAxis"), postfix:
                    new HarmonyMethod(AccessTools.Method(typeof(ControllerDiagnostics), nameof(SupplyFixtureAxis))) { priority = Priority.Last });
                var ramp = new SteeringResponse();
                float Apply(float target)
                {
                    steeringFixtureValue = ramp.Step(target, .75f, 1f / 60, true);
                    steeringFixture.DriveStuff();
                    return steeringFixture.steer;
                }
                float first = Apply(1);
                Require(first > 0 && first < .03f, "Real DriveStuff receives gradual first full-stick frame");
                for (int i = 1; i < 15; i++) Apply(1);
                Require(Mathf.Abs(steeringFixture.steer - 1f / 3) < .0001f, "Real DriveStuff reaches one-third steering after quarter second");
                for (int i = 15; i < 60; i++) Apply(1);
                Require(Mathf.Abs(steeringFixture.steer - 1) < .0001f, "Real DriveStuff reaches full steering when held");
                float reversal = Apply(-1);
                Require(reversal > .95f, "Real DriveStuff cannot instantly reverse full steering");
                for (int i = 1; i < 120; i++) Apply(-1);
                Require(Mathf.Abs(steeringFixture.steer + 1) < .0001f, "Real DriveStuff reaches opposite lock gradually");
                for (int i = 0; i < 120; i++) Apply(0);
                Require(Mathf.Abs(steeringFixture.steer) < .0001f, "Real DriveStuff fully centers after release");
            }
            finally
            {
                hook.UnpatchSelf(); settingsscript.s.S = originalSettings; inputscript.i = originalInput;
                steeringFixture = null; steeringFixtureValue = 0;
                for (int i = 0; i < labels.Length; i++) if (labels[i] != null) labels[i].text = captions[i];
                if (root != null) UnityEngine.Object.Destroy(root);
            }
        }

        private static void VerifySavedPreferences()
        {
            string path = Path.Combine(Path.GetTempPath(), "tld-controller-preferences-" + Guid.NewGuid().ToString("N") + ".cfg");
            try
            {
                foreach (ControllerFamilySelection choice in Enum.GetValues(typeof(ControllerFamilySelection)))
                {
                    var config = new ConfigFile(path, false);
                    var family = config.Bind("UI", "ControllerFamily", ControllerFamilySelection.Auto);
                    var hints = config.Bind("UI", "ShowDrivingHints", true);
                    var steeringTime = config.Bind("Input", "SteeringSecondsToFullLock", SteeringResponse.DefaultSeconds);
                    Require(config.SaveOnConfigSet, "Preference changes save automatically");
                    family.Value = choice; hints.Value = false; steeringTime.Value = 1.5f;
                    var restarted = new ConfigFile(path, false);
                    var savedFamily = restarted.Bind("UI", "ControllerFamily", ControllerFamilySelection.Auto);
                    var savedHints = restarted.Bind("UI", "ShowDrivingHints", true);
                    var savedSteeringTime = restarted.Bind("Input", "SteeringSecondsToFullLock", SteeringResponse.DefaultSeconds);
                    Require(savedSteeringTime.Value == 1.5f, "Steering time-to-lock survives restart");
                    Require(savedFamily.Value == choice && !savedHints.Value, "Family and hidden driving hints survive config restart: " + choice);
                    foreach (ControllerFamily detected in Enum.GetValues(typeof(ControllerFamily)))
                        Require(ControllerIdentity.Resolve(savedFamily.Value, detected) == ControllerIdentity.Resolve(choice, detected), "Saved override resolves against new detected " + detected);
                }
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        private static void Probe(ushort vendor, ushort product, string name, ControllerFamily family, bool generic, ushort axisCount = 6)
        {
            IntPtr nativeName = Marshal.StringToHGlobalAnsi(name), stick = IntPtr.Zero;
            SdlPad pad = null;
            int index = -1;
            try
            {
                var description = new VirtualDescription { Version = 1, Type = (ushort)(generic ? 0 : 1), Axes = axisCount, Buttons = (ushort)(generic ? 12 : 15), Hats = 1,
                    Vendor = vendor, Product = product, Name = nativeName, ButtonMask = 0x7fff, AxisMask = 0x3f };
                index = SDL_JoystickAttachVirtualEx(ref description);
                Require(index >= 0, "Attach " + name);
                stick = SdlNative.SDL_JoystickOpen(index);
                Require(stick != IntPtr.Zero, "Open virtual joystick");
                pad = SdlPad.Open(index);
                Require(pad != null && pad.Attached && pad.Family == family, "Device identity " + name);
                Require(pad.Generic == generic, "Mapped versus raw joystick route " + name);
                int lt = generic ? 2 : 4, rt = 5;
                if (!generic || axisCount >= 6)
                {
                    SDL_JoystickSetVirtualAxis(stick, lt, short.MinValue);
                    SDL_JoystickSetVirtualAxis(stick, rt, short.MinValue);
                }
                SdlNative.SDL_JoystickUpdate();
                Require(pad.Read(.18f, .08f).Neutral, "Neutral sticks/triggers " + name);
                int[] indices = generic ? new[] { 1, 2, 0, 3, 4, 5, 8, 9, 10, 11 } : new[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
                Buttons[] expected = generic ? new[] { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LB, Buttons.RB, Buttons.Back, Buttons.Start, Buttons.LS, Buttons.RS } :
                    new[] { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.Back, Buttons.Start, Buttons.LS, Buttons.RS, Buttons.LB, Buttons.RB, Buttons.Up, Buttons.Down, Buttons.Left, Buttons.Right };
                for (int i = 0; i < indices.Length; i++)
                {
                    SDL_JoystickSetVirtualButton(stick, indices[i], 1); SdlNative.SDL_JoystickUpdate();
                    Require(pad.Read(.18f, .08f).Buttons == expected[i], "Isolated button " + expected[i] + " " + name);
                    SDL_JoystickSetVirtualButton(stick, indices[i], 0); SdlNative.SDL_JoystickUpdate();
                }
                if (generic)
                {
                    SDL_JoystickSetVirtualHat(stick, 0, 3); SdlNative.SDL_JoystickUpdate();
                    Require(pad.Read(.18f, .08f).Buttons == (Buttons.Up | Buttons.Right), "Diagonal DirectInput POV hat");
                    SDL_JoystickSetVirtualHat(stick, 0, 0);
                }
                SDL_JoystickSetVirtualAxis(stick, 0, short.MaxValue);
                SDL_JoystickSetVirtualAxis(stick, 1, short.MinValue);
                SDL_JoystickSetVirtualAxis(stick, generic && axisCount >= 5 ? 3 : 2, short.MinValue);
                SDL_JoystickSetVirtualAxis(stick, generic && axisCount >= 5 ? 4 : 3, short.MaxValue);
                if (!generic || axisCount >= 6)
                {
                    SDL_JoystickSetVirtualAxis(stick, lt, short.MaxValue);
                    SDL_JoystickSetVirtualAxis(stick, rt, short.MaxValue);
                }
                else { SDL_JoystickSetVirtualButton(stick, 6, 1); SDL_JoystickSetVirtualButton(stick, 7, 1); }
                SdlNative.SDL_JoystickUpdate();
                var value = pad.Read(.18f, .08f);
                Require(value.LX > .7f && value.LY > .7f && value.RX < -.7f && value.RY < -.7f && value.LT == 1 && value.RT == 1, "Independent axes, Y sign, both triggers " + name);
                SDL_JoystickDetachVirtual(index); index = -1; SdlNative.SDL_JoystickUpdate();
                Require(!pad.Read(.18f, .08f).Connected, "Disconnect clears input " + name);
            }
            finally
            {
                if (index >= 0) SDL_JoystickDetachVirtual(index);
                pad?.Dispose();
                if (stick != IntPtr.Zero) SdlNative.SDL_JoystickClose(stick);
                Marshal.FreeHGlobal(nativeName);
            }
        }
        private static void VerifySwitching()
        {
            IntPtr firstName = Marshal.StringToHGlobalAnsi("Hub probe Sony"), secondName = Marshal.StringToHGlobalAnsi("Hub probe Nintendo");
            IntPtr first = IntPtr.Zero, second = IntPtr.Zero;
            int firstIndex = -1, secondIndex = -1, firstId = -1, secondId = -1;
            var hub = new ControllerHub(pad => pad.Name.StartsWith("Hub probe ", StringComparison.Ordinal));
            try
            {
                var description = new VirtualDescription { Version = 1, Type = 1, Axes = 6, Buttons = 15, Vendor = 0x054c,
                    Product = 0x09cc, Name = firstName, ButtonMask = 0x7fff, AxisMask = 0x3f };
                firstIndex = SDL_JoystickAttachVirtualEx(ref description);
                Require(firstIndex >= 0, "Attach first hub controller");
                firstId = SdlNative.SDL_JoystickGetDeviceInstanceID(firstIndex);
                description.Vendor = 0x057e; description.Product = 0x2009; description.Name = secondName;
                secondIndex = SDL_JoystickAttachVirtualEx(ref description);
                Require(secondIndex >= 0, "Attach second hub controller");
                secondId = SdlNative.SDL_JoystickGetDeviceInstanceID(secondIndex);
                first = SdlNative.SDL_JoystickOpen(firstIndex); second = SdlNative.SDL_JoystickOpen(secondIndex);
                foreach (var stick in new[] { first, second })
                { SDL_JoystickSetVirtualAxis(stick, 4, short.MinValue); SDL_JoystickSetVirtualAxis(stick, 5, short.MinValue); }
                SdlNative.SDL_JoystickUpdate();
                Require(!hub.Poll(.18f, .08f, _ => { }).Connected, "Hub startup inserts input boundary");
                Require(hub.Poll(.18f, .08f, _ => { }).Connected && hub.Family == ControllerFamily.PlayStation, "Hub initially chooses first controller");
                int revision = hub.Revision;
                for (int i = 0; i < 3; i++) hub.Poll(.18f, .08f, _ => { });
                Require(hub.Revision == revision, "Idle second controller cannot steal selection");
                SDL_JoystickSetVirtualButton(second, 0, 1); SdlNative.SDL_JoystickUpdate();
                Require(!hub.Poll(.18f, .08f, _ => { }).Connected && hub.Family == ControllerFamily.Nintendo, "Activity switches family with a clean input boundary");
                Require(hub.Poll(.18f, .08f, _ => { }).Held(Buttons.A), "Selected Nintendo controller preserves SDL normalized A");
                SDL_JoystickSetVirtualButton(second, 0, 0); SDL_JoystickSetVirtualButton(first, 1, 1); SdlNative.SDL_JoystickUpdate();
                Require(!hub.Poll(.18f, .08f, _ => { }).Connected && hub.Family == ControllerFamily.PlayStation, "Fresh first-controller activity switches back safely");
                Require(hub.Poll(.18f, .08f, _ => { }).Held(Buttons.B), "Selected PlayStation controller supplies canonical cancel input");
                SDL_JoystickDetachVirtual(firstIndex); firstIndex = -1;
                Require(!hub.Poll(.18f, .08f, _ => { }).Connected && hub.Family == ControllerFamily.Nintendo, "Active unplug clears its held button before fallback");
                Require(hub.Poll(.18f, .08f, _ => { }).Connected && hub.Poll(.18f, .08f, _ => { }).Neutral, "Remaining controller resumes without stale held input");
            }
            finally
            {
                DetachInstance(firstId); DetachInstance(secondId);
                hub.Dispose();
                if (first != IntPtr.Zero) SdlNative.SDL_JoystickClose(first);
                if (second != IntPtr.Zero) SdlNative.SDL_JoystickClose(second);
                Marshal.FreeHGlobal(firstName); Marshal.FreeHGlobal(secondName);
            }
        }
        private static void DetachInstance(int id)
        {
            if (id < 0) return;
            for (int i = 0; i < SdlNative.SDL_NumJoysticks(); i++)
                if (SdlNative.SDL_JoystickGetDeviceInstanceID(i) == id) { SDL_JoystickDetachVirtual(i); return; }
        }
        private static void VerifyLabels()
        {
            var original = ControllerGlyphs.Family;
            GameObject root = null;
            try
            {
                root = new GameObject("Hidden family UI verification", typeof(RectTransform), typeof(Canvas));
                root.GetComponent<Canvas>().enabled = false;
                var legacyObject = new GameObject("Legacy native-label probe", typeof(RectTransform), typeof(Text));
                legacyObject.transform.SetParent(root.transform, false);
                var legacy = legacyObject.GetComponent<Text>();
                var textObject = new GameObject("TMP native-label probe", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(root.transform, false);
                var text = textObject.GetComponent<TextMeshProUGUI>(); text.font = TMP_Settings.defaultFontAsset;
                ControllerGlyphs.SelectFamily(ControllerFamily.Xbox);
                legacy.text = text.text = ControllerGlyphs.Tmp("[A / B / LB / LT / View / Start / LS / RS / D-pad]");
                string xbox = text.text;
                ControllerGlyphs.SelectFamily(ControllerFamily.PlayStation);
                Require(legacy.text == text.text && text.text != xbox, "Existing legacy/TMP labels retarget to PlayStation");
                Require(ControllerGlyphs.Retarget(xbox) == text.text, "Cached markup resolves to current PlayStation family");
                string ps = text.text;
                var save = new SaveMenuNavigation();
                var fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(SaveMenuNavigation).GetField("legacyHint", fields).SetValue(save, legacy);
                typeof(SaveMenuNavigation).GetField("original", fields).SetValue(save, "Save fixture");
                typeof(SaveMenuNavigation).GetField("marked", fields).SetValue(save, ps);
                ControllerGlyphs.SelectFamily(ControllerFamily.Nintendo);
                Require(legacy.text == text.text && text.text != ps, "Existing legacy/TMP labels retarget to Nintendo");
                Require(ControllerGlyphs.Retarget(ps) == text.text, "Cached markup resolves to current Nintendo family");
                save.Release();
                Require(legacy.text == "Save fixture", "Save caption restores after family retarget before next update");
                legacy.text = text.text;
                ControllerGlyphs.SelectFamily(ControllerFamily.Xbox);
                Require(legacy.text == xbox && text.text == xbox, "Round-trip glyph cache remains family-correct");
            }
            finally { ControllerGlyphs.SelectFamily(original); if (root != null) UnityEngine.Object.Destroy(root); }
        }
    }
}
