using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LongDrive.Controller
{
    // Native interaction prompts and Controls rows use Unity UI Text, which has no
    // sprite support. Keep the original component/text for the game's own updates,
    // and replace only its visual mesh while our sprite markup is present.
    internal sealed class LegacyGlyphText : BaseMeshEffect
    {
        private Text original;
        private TextMeshProUGUI replacement;
        private bool showing;

        internal void Refresh(string value)
        {
            original = GetComponent<Text>();
            showing = ControllerGlyphs.ContainsMarkup(value);
            if (showing && replacement == null)
            {
                var child = new GameObject("Controller prompt icons", typeof(RectTransform), typeof(TextMeshProUGUI));
                child.transform.SetParent(transform, false);
                replacement = child.GetComponent<TextMeshProUGUI>();
                var rect = replacement.rectTransform;
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                replacement.raycastTarget = false;
                replacement.font = TMP_Settings.defaultFontAsset;
                replacement.richText = true;
            }
            if (replacement != null)
            {
                replacement.enabled = showing;
                if (showing) { replacement.text = value; Sync(); }
            }
            original.SetVerticesDirty();
        }
        private void LateUpdate() { if (showing) Sync(); }
        private void Sync()
        {
            if (original == null || replacement == null) return;
            replacement.enabled = showing && original.enabled;
            replacement.color = original.color;
            replacement.fontSize = original.fontSize;
            replacement.enableAutoSizing = original.resizeTextForBestFit;
            replacement.fontSizeMin = original.resizeTextMinSize;
            replacement.fontSizeMax = original.resizeTextMaxSize;
            replacement.enableWordWrapping = original.horizontalOverflow == HorizontalWrapMode.Wrap;
            replacement.overflowMode = original.verticalOverflow == VerticalWrapMode.Overflow ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
            replacement.fontStyle = original.fontStyle == UnityEngine.FontStyle.Bold ? FontStyles.Bold :
                original.fontStyle == UnityEngine.FontStyle.Italic ? FontStyles.Italic :
                original.fontStyle == UnityEngine.FontStyle.BoldAndItalic ? FontStyles.Bold | FontStyles.Italic : FontStyles.Normal;
            switch (original.alignment)
            {
                case TextAnchor.UpperLeft: replacement.alignment = TextAlignmentOptions.TopLeft; break;
                case TextAnchor.UpperCenter: replacement.alignment = TextAlignmentOptions.Top; break;
                case TextAnchor.UpperRight: replacement.alignment = TextAlignmentOptions.TopRight; break;
                case TextAnchor.MiddleLeft: replacement.alignment = TextAlignmentOptions.Left; break;
                case TextAnchor.MiddleCenter: replacement.alignment = TextAlignmentOptions.Center; break;
                case TextAnchor.MiddleRight: replacement.alignment = TextAlignmentOptions.Right; break;
                case TextAnchor.LowerLeft: replacement.alignment = TextAlignmentOptions.BottomLeft; break;
                case TextAnchor.LowerCenter: replacement.alignment = TextAlignmentOptions.Bottom; break;
                case TextAnchor.LowerRight: replacement.alignment = TextAlignmentOptions.BottomRight; break;
            }
        }
        public override void ModifyMesh(VertexHelper vertices) { if (showing && IsActive()) vertices.Clear(); }
        protected override void OnDestroy()
        {
            if (replacement != null) Destroy(replacement.gameObject);
            base.OnDestroy();
        }
    }

    [HarmonyPatch(typeof(Text), "set_text")]
    internal static class LegacyGlyphPatch
    {
        private static void Postfix(Text __instance, string value)
        {
            var bridge = __instance.GetComponent<LegacyGlyphText>();
            if (bridge == null && ControllerGlyphs.ContainsMarkup(value)) bridge = __instance.gameObject.AddComponent<LegacyGlyphText>();
            if (bridge != null) bridge.Refresh(value);
        }
    }
}
