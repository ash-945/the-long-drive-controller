using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace LongDrive.Controller
{
    // Local to the game's computer: the virtual keyboard augments its input reads without
    // changing desktop input, other game input consumers, or the player's saved bindings.
    internal static class ComputerInput
    {
        private static int sampled = -1;
        private static string text = "", note = "";
        private static float noteUntil;
        public static string Text()
        {
            if (sampled != Time.frameCount)
            {
                sampled = Time.frameCount;
                string synthetic = Plugin.Instance == null ? "" : Plugin.Instance.Keyboard.TakeComputerText();
                text = Input.inputString + synthetic;
                if (synthetic.Length > 0) { note = synthetic; noteUntil = Time.unscaledTime + .18f; }
            }
            return text;
        }
        public static bool AnyDown() => Input.anyKeyDown || Text().Length > 0;
        public static bool Any() => Input.anyKey || Text().Length > 0;
        public static bool Held(string key) { Text(); return Input.GetKey(key) || Time.unscaledTime < noteUntil && note.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0; }
        public static bool Down(string key) => Input.GetKeyDown(key) || Text().IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0;
    }
    [HarmonyPatch]
    internal static class ComputerInputPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(computerscript), "InputRead");
            yield return AccessTools.Method(typeof(computerscript), "Update");
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.operand is MethodInfo method && method.DeclaringType == typeof(Input))
                {
                    string replacement = null;
                    switch (method.Name)
                    {
                        case "get_inputString": replacement = nameof(ComputerInput.Text); break;
                        case "get_anyKeyDown": replacement = nameof(ComputerInput.AnyDown); break;
                        case "get_anyKey": replacement = nameof(ComputerInput.Any); break;
                        case "GetKey": if (method.GetParameters()[0].ParameterType == typeof(string)) replacement = nameof(ComputerInput.Held); break;
                        case "GetKeyDown": if (method.GetParameters()[0].ParameterType == typeof(string)) replacement = nameof(ComputerInput.Down); break;
                    }
                    if (replacement != null) { instruction.opcode = System.Reflection.Emit.OpCodes.Call; instruction.operand = AccessTools.Method(typeof(ComputerInput), replacement); }
                }
                yield return instruction;
            }
        }
    }
    [HarmonyPatch(typeof(inputscript), "Update")]
    internal static class ComputerCursorPatch
    {
        private static float repeat;
        private static void Postfix(inputscript __instance)
        {
            if (!__instance.usingComputer || Plugin.Instance == null || !Plugin.Instance.ControllerConnected) return;
            var pad = Plugin.Instance.ComputerCursorPad();
            bool step = Time.unscaledTime >= repeat;
            __instance.keyup = step && pad.RY > .5f;
            __instance.keydown = step && pad.RY < -.5f;
            __instance.keyleft = step && pad.RX < -.5f;
            __instance.keyright = step && pad.RX > .5f;
            if (__instance.keyup || __instance.keydown || __instance.keyleft || __instance.keyright) repeat = Time.unscaledTime + .12f;
        }
    }
}
