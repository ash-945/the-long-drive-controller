using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace LongDrive.Controller
{
    [BepInPlugin(Id, "The Long Drive Controller", "0.6.6")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Id = "ashley.thelongdrive.controller";
        internal static Plugin Instance;
        private readonly ControllerHub device = new ControllerHub();
        private readonly InputGate gate = new InputGate();
        private PadState raw, previousRaw, current, previous;
        private int sampled = -1, blockUntil;
        private string context = "starting", lastContext;
        private bool wasConnected, supported;
        private Harmony patches;
        private MenuControls menus;
        internal readonly ActionState Actions = new ActionState();
        private readonly ActionMenu actionMenu = new ActionMenu();
        private readonly ControllerSettings controllerSettings = new ControllerSettings();
        private ConfigEntry<float> deadzone, triggerThreshold, lookSpeed, pointerSpeed, manipulationSpeed, rotationSpeed, steeringSensitivity;
        private ConfigEntry<bool> invertY, showDrivingHints;
        private ConfigEntry<ControllerFamilySelection> familySelection;
        private ConfigEntry<string> walkingBindings, drivingBindings;
        private bool reportedRuntime;
        private bool captureRequested;
        private bool firstSample;
        private bool capturedActions;
        private readonly System.Collections.Generic.HashSet<string> capturedSettings = new System.Collections.Generic.HashSet<string>();
        private int capturedBindingRevision = -1;
        private float bindingCaptureAfter;
        internal static int AxisCalls, ButtonCalls, LookCalls;

        private void Awake()
        {
            Instance = this;
            captureRequested = Array.IndexOf(Environment.GetCommandLineArgs(), "--controller-capture") >= 0;
            if (captureRequested) Application.runInBackground = true;
            supported = Application.version == "2024.11.26b_test" || Application.version == "2024.11.26b";
            deadzone = Config.Bind("Input", "StickDeadzone", .18f, new ConfigDescription("Radial stick deadzone.", new AcceptableValueRange<float>(.02f, .6f)));
            triggerThreshold = Config.Bind("Input", "TriggerDeadzone", .08f, new ConfigDescription("Independent trigger deadzone.", new AcceptableValueRange<float>(0, .5f)));
            lookSpeed = Config.Bind("Input", "LookSpeed", 140f, new ConfigDescription("Approximate camera degrees per second, before the game's zoom modifiers.", new AcceptableValueRange<float>(20, 360)));
            manipulationSpeed = Config.Bind("Input", "ManipulationSpeed", 3f, new ConfigDescription("Object movement and leaning rate. Rotation has its own independent setting.", new AcceptableValueRange<float>(.2f, 12)));
            rotationSpeed = Config.Bind("Input", "ObjectRotationDegreesPerSecond", 120f, new ConfigDescription("Held-object rotation speed before game weight/zoom modifiers. Separate from mouse and camera sensitivity.", new AcceptableValueRange<float>(15, 360)));
            pointerSpeed = Config.Bind("UI", "PointerSpeed", 850f, new ConfigDescription("Virtual cursor pixels per second.", new AcceptableValueRange<float>(100, 2000)));
            steeringSensitivity = Config.Bind("Input", "SteeringStickSensitivity", 1f, new ConfigDescription("Steering stick response: lower gives finer small corrections; higher responds sooner. Full stick travel always reaches full steering. Does not affect walking or camera sensitivity.", new AcceptableValueRange<float>(.25f, 2f)));
            invertY = Config.Bind("Input", "InvertLookY", false, "Invert controller look independently of the mouse.");
            familySelection = Config.Bind("UI", "ControllerFamily", ControllerFamilySelection.Auto,
                "Button icon family: Auto, Xbox, PlayStation, or Nintendo. A manual choice overrides detection until Auto is selected.");
            showDrivingHints = Config.Bind("UI", "ShowDrivingHints", true, "Show the controller hint box while driving.");
            walkingBindings = Config.Bind("Bindings", "OnFoot", "", "Saved controller bindings for on-foot gameplay. Edit using the Bindings page in controller settings.");
            drivingBindings = Config.Bind("Bindings", "Driving", "", "Saved controller bindings for driving. Edit using the Bindings page in controller settings.");
            if (!ActionCatalog.Bindings.TryLoad(walkingBindings.Value, false)) Logger.LogWarning("Invalid on-foot bindings; using defaults.");
            if (!ActionCatalog.Bindings.TryLoad(drivingBindings.Value, true)) Logger.LogWarning("Invalid driving bindings; using defaults.");
            menus = new MenuControls();
            ControllerGlyphs.Load(Logger);
            RefreshControllerFamily();
            Logger.LogInfo("Controller 0.6.6; game=" + Application.version + "; Unity=" + Application.unityVersion + "; OS=" + SystemInfo.operatingSystem);
            if (!supported) { Logger.LogError("Unsupported game version. Controller hooks disabled."); return; }
            patches = new Harmony(Id);
            patches.PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo("Controller routes: " + ActionCatalog.Count + "; prompt presentation call sites rewritten: " + PromptTextPatch.Replacements);
            Logger.LogInfo("Installed input GetAxis/GetKey/GetKeyDown/GetKeyUp hooks.");
        }

        internal bool Driving => mainscript.s != null && mainscript.s.player != null && mainscript.s.player.Car != null;
        internal bool InGameplay => context == "walking" || context == "driving";
        internal bool ControllerConnected => supported && raw.Connected;
        internal bool ShowDrivingHints => showDrivingHints.Value;
        private void RefreshControllerFamily() => ControllerGlyphs.SelectFamily(ControllerIdentity.Resolve(familySelection.Value, device.Family));
        internal bool CustomPanel => actionMenu.Open || controllerSettings.Open;
        private bool MenuOpen => !menuhandler.isStarted() || DataFromMenuScript.s == null || DataFromMenuScript.s.mainmenu || mainscript.s == null || mainscript.s.SomeMenuOpen();

        internal void Sample()
        {
            if (sampled == Time.frameCount) return;
            sampled = Time.frameCount;
            if (!firstSample) { firstSample = true; Logger.LogInfo("First controller poll at frame " + Time.frameCount); }
            previousRaw = raw;
            raw = device.Poll(deadzone.Value, triggerThreshold.Value, message => Logger.LogInfo(message));
            if (raw.Connected != wasConnected)
            {
                Logger.LogInfo(raw.Connected ? "Connected " + device.Description : "Controller disconnected; clearing controller input.");
                wasConnected = raw.Connected;
            }
            RefreshControllerFamily();
            previous = current;
            context = !supported ? "unsupported build" : !Application.isFocused ? "unfocused" :
                inputscript.i != null && inputscript.i.usingComputer ? "computer" : MenuOpen ? "menu" : mainscript.s.SomeInputSelected() ? "text" : Driving ? "driving" : "walking";
            current = gate.Filter(raw, context, Application.isFocused && supported);
            if (context != lastContext)
            {
                if (inputscript.i != null)
                {
                    // The game's keyboard smoothing keeps values across frames. Do not carry
                    // controller pedal/steer values into a menu, a different context, or reconnect.
                    inputscript.i.BGas = inputscript.i.BBrake = inputscript.i.BClutch = inputscript.i.BSteer = 0;
                }
                previous = default;
                Logger.LogInfo("Context: " + context);
                lastContext = context;
                actionMenu.Close();
                controllerSettings.Close();
                previousRaw = raw;
            }
            if (Time.frameCount <= blockUntil) current = default;
            if (!raw.Connected && inputscript.i != null)
            {
                if (previous.Connected) inputscript.i.BGas = inputscript.i.BBrake = inputscript.i.BClutch = inputscript.i.BSteer = 0;
            }
            if (!raw.Connected || !Application.isFocused)
            {
                Actions.Clear(); actionMenu.Close(); controllerSettings.Close();
            }
            // View is a recovery control and must not depend on the gameplay neutral gate.
            if (supported && Application.isFocused && UiDown(Buttons.Back))
            {
                if (CustomPanel) { actionMenu.Close(); controllerSettings.Close(); }
                else if (mainscript.s != null && mainscript.s.settingsOpen) controllerSettings.Show();
                else actionMenu.Toggle();
                previousRaw = raw;
                menus.Release(); SuppressTransition();
            }
            var actionPad = current;
            Actions.Tick(actionPad, context, InGameplay && current.Connected && !CustomPanel, Time.unscaledTime,
                InGameplay && raw.Connected && Application.isFocused ? actionMenu.HeldAction(raw) : null, steeringSensitivity.Value);
        }

        private void Update()
        {
            Sample();
            if (!supported) return;
            // The removed keyboard used to provide this exit control. Preserve it
            // without an overlay, and release any computer music notes.
            if (context == "computer" && !CustomPanel && (UiDown(Buttons.B) || UiDown(Buttons.Start)))
            {
                var player = mainscript.s == null ? null : mainscript.s.player;
                if (player != null && player.lookedAtComputer != null) player.lookedAtComputer.StopAllNote();
                if (inputscript.i != null) inputscript.i.usingComputer = false;
                SuppressTransition();
                return;
            }
            if (controllerSettings.Open) controllerSettings.Tick(this, raw, previousRaw);
            else if (actionMenu.Open) actionMenu.Tick(this, raw, previousRaw);
            else
            {
                if (UiDown(Buttons.Start) || (context == "menu" && UiDown(Buttons.B)))
                {
                    if (mainscript.s != null && menuhandler.isStarted())
                    {
                        if (context == "menu" && UiDown(Buttons.B) && menus.Cancel()) { }
                        else if (mainscript.s.settingsOpen) settingsscript.s.PBack(false);
                        else mainscript.s.PressedEscape(false, false);
                        menus.Release();
                        SuppressTransition();
                    }
                }
            }
            PromptNames.Refresh(this);
            if (captureRequested)
            {
                if (actionMenu.Open && !capturedActions) { capturedActions = true; Capture("controller-actions.png"); }
                if (controllerSettings.Open && capturedSettings.Add(controllerSettings.CaptureState)) Capture("controller-" + controllerSettings.CaptureState + ".png");
                if (capturedBindingRevision != ActionCatalog.Bindings.Revision)
                { capturedBindingRevision = ActionCatalog.Bindings.Revision; bindingCaptureAfter = Time.unscaledTime + .5f; }
                if (bindingCaptureAfter > 0 && Time.unscaledTime >= bindingCaptureAfter && InGameplay && !CustomPanel)
                { bindingCaptureAfter = 0; Capture("controller-bindings-gameplay.png"); }
            }
            if (context != "menu" && !CustomPanel) menus.Release();
            // Opt-in input/UI diagnostics need no loaded save or gameplay scene.
            bool selfTest = Array.IndexOf(Environment.GetCommandLineArgs(), "--controller-self-test") >= 0;
            if (!reportedRuntime && Time.realtimeSinceStartup > (selfTest ? 1 : 10) && (menuhandler.isStarted() || selfTest))
            {
                reportedRuntime = true;
                Logger.LogInfo("Runtime check: input=" + (inputscript.i != null) + "; EventSystem=" + (UnityEngine.EventSystems.EventSystem.current != null) + "; active Selectables=" + UnityEngine.UI.Selectable.allSelectablesArray.Length + "; hook calls axis/buttons/look=" + AxisCalls + "/" + ButtonCalls + "/" + LookCalls);
                if (selfTest) ControllerDiagnostics.Run(Logger);
                if (captureRequested)
                {
                    ControllerGlyphs.Verify(Logger);
                    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Paths.GameRootPath, "controller-prototype.png"));
                    Logger.LogInfo("Requested diagnostic screenshot: controller-prototype.png");
                }
            }
        }

        private void Capture(string name)
        {
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Paths.GameRootPath, name));
            Logger.LogInfo("Captured " + name);
        }

        private void LateUpdate()
        {
            if (supported && context == "menu" && !CustomPanel) menus.Tick(this, raw, previousRaw, pointerSpeed.Value);
        }

        internal void PrepareMenuInput()
        {
            Sample();
            if (CustomPanel)
            {
                menus.Prepare(raw);
                menus.SetModal(true);
            }
            else if (context == "menu") { menus.SetModal(false); menus.Prepare(raw); }
            else menus.Release();
        }

        internal void SaveBindings()
        {
            walkingBindings.Value = ActionCatalog.Bindings.Serialize(false);
            drivingBindings.Value = ActionCatalog.Bindings.Serialize(true);
            Actions.Clear(); SuppressTransition();
            Logger.LogInfo("Controller bindings saved; revision=" + ActionCatalog.Bindings.Revision);
            PromptNames.Refresh(this, true);
        }

        internal bool Defecate(out string message)
        {
            var player = mainscript.s == null ? null : mainscript.s.player;
            if (!InGameplay || player == null || player.survival == null)
            { message = "Resume gameplay to perform actions."; return false; }
            if (player.survival.shit < player.survival.maxShit * .3f)
            { message = "Bowel meter must be at least 30% full."; return false; }
            if (player.SFart.isPlaying)
            { message = "Wait for the current action to finish."; return false; }
            player.Shit();
            message = "";
            return true;
        }

        internal void ShowAction(GameAction action) { actionMenu.Show(action); menus.Release(); SuppressTransition(); }
        internal void ShowControllerSettings() { actionMenu.Close(); controllerSettings.Show(); previousRaw = raw; menus.Release(); SuppressTransition(); }
        internal void ShowControllerBinding(GameAction action)
        {
            actionMenu.Close(); controllerSettings.ShowBinding(action, Driving);
            previousRaw = raw; menus.Release(); SuppressTransition();
        }
        internal bool UiDown(Buttons b) => Application.isFocused && raw.Held(b) && !previousRaw.Held(b);
        internal int SettingCount => 10;
        internal string SettingText(int index)
        {
            switch (index)
            {
                case 0: return $"Camera speed:  {lookSpeed.Value:0} deg/s";
                case 1: return $"Object rotation speed:  {rotationSpeed.Value:0} deg/s";
                case 2: return $"Object movement / lean speed:  {manipulationSpeed.Value:0.00}";
                case 3: return $"Menu pointer speed:  {pointerSpeed.Value:0} px/s";
                case 4: return $"Stick deadzone:  {deadzone.Value:0.00}";
                case 5: return $"Trigger deadzone:  {triggerThreshold.Value:0.00}";
                case 6: return "Invert camera Y:  " + (invertY.Value ? "On" : "Off");
                case 7: return $"Steering stick sensitivity:  {steeringSensitivity.Value:0.00}  (lower = gentler)";
                case 8: return "Controller family:  " + (familySelection.Value == ControllerFamilySelection.Auto
                    ? "Auto (" + device.Family + ")" : familySelection.Value.ToString());
                case 9: return "Driving hint box:  " + (showDrivingHints.Value ? "On" : "Off");
                default: return "";
            }
        }
        internal void AdjustSetting(int index, int delta)
        {
            switch (index)
            {
                case 0: lookSpeed.Value = Mathf.Clamp(lookSpeed.Value + delta * 10, 20, 360); break;
                case 1: rotationSpeed.Value = Mathf.Clamp(rotationSpeed.Value + delta * 15, 15, 360); break;
                case 2: manipulationSpeed.Value = Mathf.Clamp(manipulationSpeed.Value + delta * .25f, .2f, 12); break;
                case 3: pointerSpeed.Value = Mathf.Clamp(pointerSpeed.Value + delta * 50, 100, 2000); break;
                case 4: deadzone.Value = Mathf.Clamp(deadzone.Value + delta * .02f, .02f, .6f); break;
                case 5: triggerThreshold.Value = Mathf.Clamp(triggerThreshold.Value + delta * .02f, 0, .5f); break;
                case 6: invertY.Value = delta == 0 ? !invertY.Value : delta > 0; break;
                case 7: steeringSensitivity.Value = Mathf.Clamp(steeringSensitivity.Value + delta * .1f, .25f, 2f); break;
                case 8: familySelection.Value = ControllerIdentity.Cycle(familySelection.Value, delta); RefreshControllerFamily(); break;
                case 9: showDrivingHints.Value = delta == 0 ? !showDrivingHints.Value : delta > 0; break;
            }
        }
        internal void ResetControllerSettings()
        {
            lookSpeed.Value = 140; rotationSpeed.Value = 120; manipulationSpeed.Value = 3;
            pointerSpeed.Value = 850; deadzone.Value = .18f; triggerThreshold.Value = .08f;
            invertY.Value = false; steeringSensitivity.Value = 1;
            familySelection.Value = ControllerFamilySelection.Auto; showDrivingHints.Value = true;
            RefreshControllerFamily();
        }
        internal PadState ComputerCursorPad() { Sample(); return context == "computer" ? current : default; }

        private void SuppressTransition()
        {
            blockUntil = Time.frameCount + 2;
            current = previous = default;
            // Rearm only after all controls are released, even if a delayed menu transition follows.
            gate.Filter(raw, "transition", false);
        }

        internal void MenuActivated() => SuppressTransition();
        internal bool Down(Buttons b) => current.Held(b) && !previous.Held(b);
        internal float Axis(inputscript.IN action)
        {
            Sample();
            if (!supported || (int)action < 0 || (int)action >= ActionCatalog.Count) return 0;
            // Right-stick camera/object input is added after the game's mouse sensitivity.
            if ((int)action >= 4 && (int)action <= 7) return 0;
            float value = Actions.Value((GameAction)action);
            return action == inputscript.IN.scrollup || action == inputscript.IN.scrolldown ? value * Time.unscaledDeltaTime : value;
        }

        internal bool Button(inputscript.IN action, int edge)
        {
            Sample();
            if (!supported || (int)action < 0 || (int)action >= ActionCatalog.Count) return false;
            var a = (GameAction)action;
            return edge == 1 ? Actions.Pressed(a) : edge == -1 ? Actions.Released(a) : Actions.Value(a) > .2f;
        }

        internal void AddLook(fpscontroller player)
        {
            Sample();
            if (!InGameplay || CustomPanel) return;
            if (inputscript.i != null && inputscript.i.mouseSteer) return;
            float x = Actions.Value(GameAction.lookright) - Actions.Value(GameAction.lookleft);
            float y = Actions.Value(GameAction.lookup) - Actions.Value(GameAction.lookdown);
            // Rotation has its own native rate path. LB is required for rotation, so treating
            // LB as a precision modifier would slow every rotation unintentionally.
            bool heldObject = player.pickedUp != null;
            bool rotateObject = heldObject && Actions.Value(GameAction.mmb) > .2f;
            bool manipulate = heldObject && Actions.Value(GameAction.rmb) > .2f || player.Bleaning || player.rotating || player.Etuning || player.reloading;
            float scale = rotateObject ? PadMath.RotationInput(rotationSpeed.Value, player.objRotSens) :
                manipulate ? manipulationSpeed.Value : lookSpeed.Value * Mathf.Min(Time.unscaledDeltaTime, .05f);
            player.LOOKX += x * scale;
            player.LOOKY += y * scale * (invertY.Value ? -1 : 1);
        }

        internal void AddPointerDriving(inputscript input)
        {
            Sample();
            if (!InGameplay || !Driving || !current.Connected || CustomPanel || !input.mouseSteer) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, .05f);
            float before = input.currentMouseSteer;
            input.currentMouseSteer = Mathf.Clamp(before + PadMath.SteeringInput(Actions.Value(GameAction.lookright) - Actions.Value(GameAction.lookleft), steeringSensitivity.Value) * dt * 1.2f, -1, 1);
            input.steer = Mathf.Clamp(input.steer + input.currentMouseSteer - before, -1, 1);
            if (input.mousePedal)
            {
                before = input.currentMouseGas;
                input.currentMouseGas = Mathf.Clamp(before + (Actions.Value(GameAction.lookup) - Actions.Value(GameAction.lookdown)) * dt, -1, 1);
                input.throttle = Mathf.Clamp01(input.throttle + Mathf.Max(0, input.currentMouseGas) - Mathf.Max(0, before));
                input.brake = Mathf.Clamp01(input.brake + Mathf.Max(0, -input.currentMouseGas) - Mathf.Max(0, -before));
            }
        }

        private void OnGUI()
        {
            if (supported && context == "menu" && !CustomPanel) menus.Draw();
            DrivingHints.Draw(this);
            actionMenu.Draw(this);
            controllerSettings.Draw(this);
            if (ControllerConnected && !CustomPanel && mainscript.s != null && mainscript.s.settingsOpen)
                if (PanelStyle.Choice(new Rect(Screen.width - 410, 16, 390, 42), "Controller settings  [View]", false, true)) ShowControllerSettings();
        }

        internal void LoggerForSkin(string message) => Logger.LogInfo(message);

        private void OnDestroy()
        {
            Logger.LogInfo("Controller prototype destroyed; removing hooks.");
            Actions.Clear();
            menus?.Release();
            patches?.UnpatchSelf();
            device.Dispose();
            SdlNative.Dispose();
            ControllerGlyphs.Dispose();
            GameMenuSkin.Dispose();
            if (Instance == this) Instance = null;
        }
        private void Start() { Logger.LogInfo("Controller prototype Start; enabled=" + enabled + "; active=" + gameObject.activeInHierarchy); }
        private void OnDisable() { Logger.LogInfo("Controller prototype disabled."); }
    }

    [HarmonyPatch(typeof(inputscript), "GetAxis")]
    internal static class AxisPatch
    {
        private static void Postfix(inputscript.IN _input, ref float __result)
        {
            Plugin.AxisCalls++;
            if (Plugin.Instance != null) __result = Mathf.Max(__result, Plugin.Instance.Axis(_input));
        }
    }
    [HarmonyPatch(typeof(inputscript), "GetKey")]
    internal static class HeldPatch
    {
        private static void Postfix(inputscript.IN _input, ref bool __result) { Plugin.ButtonCalls++; if (Plugin.Instance != null) __result |= Plugin.Instance.Button(_input, 0); }
    }
    [HarmonyPatch(typeof(inputscript), "GetKeyDown")]
    internal static class DownPatch
    {
        private static void Postfix(inputscript.IN _input, ref bool __result) { Plugin.ButtonCalls++; if (Plugin.Instance != null) __result |= Plugin.Instance.Button(_input, 1); }
    }
    [HarmonyPatch(typeof(inputscript), "GetKeyUp")]
    internal static class UpPatch
    {
        private static void Postfix(inputscript.IN _input, ref bool __result) { Plugin.ButtonCalls++; if (Plugin.Instance != null) __result |= Plugin.Instance.Button(_input, -1); }
    }
    [HarmonyPatch(typeof(fpscontroller), "GetLook")]
    internal static class LookPatch
    {
        private static void Postfix(fpscontroller __instance) { Plugin.LookCalls++; Plugin.Instance?.AddLook(__instance); }
    }
    [HarmonyPatch(typeof(inputscript), "DriveStuff")]
    internal static class PointerDrivePatch
    {
        private static void Postfix(inputscript __instance) { Plugin.Instance?.AddPointerDriving(__instance); }
    }
    [HarmonyPatch(typeof(UnityEngine.EventSystems.EventSystem), "Update")]
    internal static class NavigationPatch
    {
        private static void Prefix() { Plugin.Instance?.PrepareMenuInput(); }
    }
}
