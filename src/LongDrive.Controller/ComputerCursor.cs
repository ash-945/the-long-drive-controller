using HarmonyLib;
using UnityEngine;

namespace LongDrive.Controller
{
    [HarmonyPatch(typeof(inputscript), "Update")]
    internal static class ComputerCursorPatch
    {
        private static float repeat;
        private static void Postfix(inputscript __instance)
        {
            if (!__instance.usingComputer || Plugin.Instance == null || !Plugin.Instance.ControllerConnected) return;
            var pad = Plugin.Instance.ComputerCursorPad();
            bool step = Time.unscaledTime >= repeat;
            bool up = step && pad.RY > .5f, down = step && pad.RY < -.5f;
            bool left = step && pad.RX < -.5f, right = step && pad.RX > .5f;
            // Keep native physical-keyboard arrows after removing synthetic text.
            __instance.keyup |= up; __instance.keydown |= down;
            __instance.keyleft |= left; __instance.keyright |= right;
            if (up || down || left || right) repeat = Time.unscaledTime + .12f;
        }
    }
}
