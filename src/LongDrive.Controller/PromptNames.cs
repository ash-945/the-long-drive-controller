using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace LongDrive.Controller
{
    internal static class PromptNames
    {
        private static inputscript.oneInput[] source;
        private static readonly Dictionary<inputscript.oneInput, GameAction> lookup = new Dictionary<inputscript.oneInput, GameAction>();
        private static bool lastConnected, lastDriving;

        public static string Get(inputscript.oneInput input)
        {
            var plugin = Plugin.Instance;
            if (plugin == null || !plugin.ControllerConnected) return input.GetName();
            var current = settingsscript.s == null ? null : settingsscript.s.S.inputs;
            if (source != current || !lookup.ContainsKey(input))
            {
                source = current;
                lookup.Clear();
                if (source != null)
                    for (int i = 0; i < source.Length && i < ActionCatalog.Count; i++)
                        if (source[i] != null) lookup[source[i]] = (GameAction)i;
            }
            return lookup.TryGetValue(input, out var action) ? ControllerGlyphs.Tmp(ActionCatalog.Hint(action, plugin.Driving)) : input.GetName();
        }

        public static void Refresh(Plugin plugin)
        {
            if (lastConnected == plugin.ControllerConnected && lastDriving == plugin.Driving) return;
            if (settingsscript.s == null || mainscript.s == null || !menuhandler.isStarted()) return;
            lastConnected = plugin.ControllerConnected;
            lastDriving = plugin.Driving;
            mainscript.s.RefreshHelpTexts();
            foreach (var row in UnityEngine.Object.FindObjectsOfType<controlsKey>(true))
                if (row.buttontexts != null && row.buttontexts.Length == 3) row.UpdText();
        }
    }

    // Only change presentation call sites. GetName also participates in gameplay binding
    // comparisons; patching it globally would alter those comparisons.
    [HarmonyPatch]
    internal static class PromptTextPatch
    {
        internal static int Replacements;
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(mainscript), "Update");
            yield return AccessTools.Method(typeof(mainscript), "RefreshHelpTextLanguage");
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var original = AccessTools.Method(typeof(inputscript.oneInput), "GetName", System.Type.EmptyTypes);
            var replacement = AccessTools.Method(typeof(PromptNames), nameof(PromptNames.Get));
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(original)) { instruction.opcode = System.Reflection.Emit.OpCodes.Call; instruction.operand = replacement; Replacements++; }
                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(mainscript), "Mouse")]
    internal static class MousePromptPatch
    {
        private static void Postfix(ref string __result) { if (Plugin.Instance != null && Plugin.Instance.ControllerConnected) __result = "RS"; }
    }
    [HarmonyPatch(typeof(mainscript), "Szamok")]
    internal static class GearPromptPatch
    {
        private static void Postfix(ref string __result) { if (Plugin.Instance != null && Plugin.Instance.ControllerConnected) __result = ControllerGlyphs.Tmp("View > Gears"); }
    }
    [HarmonyPatch(typeof(controlsKey), "UpdText")]
    internal static class ControlsPromptPatch
    {
        private static void Postfix(controlsKey __instance)
        {
            if (Plugin.Instance == null || !Plugin.Instance.ControllerConnected || __instance.actionId < 0 || __instance.actionId >= ActionCatalog.Count) return;
            string[] labels = { ActionCatalog.Hint((GameAction)__instance.actionId, Plugin.Instance.Driving), "View: uncommon actions", "" };
            for (int i = 0; i < __instance.buttontexts.Length && i < labels.Length; i++)
                if (__instance.buttontexts[i] != null) __instance.buttontexts[i].text = ControllerGlyphs.Tmp(labels[i]);
        }
    }
    [HarmonyPatch(typeof(controlsKey), "Pressed")]
    internal static class ControlsActionPatch
    {
        private static bool Prefix(controlsKey __instance)
        {
            if (Plugin.Instance == null || !Plugin.Instance.ControllerConnected || __instance.actionId < 0 || __instance.actionId >= ActionCatalog.Count) return true;
            Plugin.Instance.ShowAction((GameAction)__instance.actionId);
            return false;
        }
    }
}
