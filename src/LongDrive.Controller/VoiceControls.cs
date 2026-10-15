using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace LongDrive.Controller
{
    // Humming adjusts pitch/volume through inputscript fields before GetLook is called.
    // Keep those reads separate from the camera adapter so controller look is not doubled.
    [HarmonyPatch(typeof(fpscontroller), "Update")]
    internal static class VoiceControls
    {
        internal static float VolumeAxis(inputscript input) => input.lookX + ControllerAxis(true) * Time.unscaledDeltaTime * 2;
        internal static float PitchAxis(inputscript input) => input.lookY + ControllerAxis(false);
        private static float ControllerAxis(bool horizontal)
        {
            var plugin = Plugin.Instance;
            if (plugin == null || !plugin.InGameplay || plugin.CustomPanel) return 0;
            return horizontal ? plugin.Actions.Value(GameAction.lookright) - plugin.Actions.Value(GameAction.lookleft) : plugin.Actions.Value(GameAction.lookup) - plugin.Actions.Value(GameAction.lookdown);
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var x = AccessTools.Field(typeof(inputscript), "lookX");
            var y = AccessTools.Field(typeof(inputscript), "lookY");
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, x))
                { instruction.opcode = OpCodes.Call; instruction.operand = AccessTools.Method(typeof(VoiceControls), nameof(VolumeAxis)); }
                else if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, y))
                { instruction.opcode = OpCodes.Call; instruction.operand = AccessTools.Method(typeof(VoiceControls), nameof(PitchAxis)); }
                yield return instruction;
            }
        }
    }
}
