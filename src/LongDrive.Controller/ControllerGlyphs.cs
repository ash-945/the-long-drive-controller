using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace LongDrive.Controller
{
    internal static class ControllerGlyphs
    {
        internal const string AssetName = "TLDController";
        internal static ControllerFamily Family { get; private set; }
        internal static int Revision { get; private set; }
        private static string Key(string name) => Family + "/" + name;
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, int> Indices = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> RichCache = new Dictionary<string, string>();
        private static TMP_SpriteAsset asset;
        private static Texture2D atlas;
        private static Material material;
        public static readonly string[] SpriteNames = { "A", "B", "X", "Y", "LB", "RB", "LT", "RT", "L3", "R3", "Select", "Start", "DPad_UP", "DPad_Down", "DPad_Left", "DPad_Right", "LS", "RS", "DPad", "LS_Up", "LS_Down", "LS_Left", "LS_Right", "RS_Up", "RS_Down", "RS_Left", "RS_Right" };

        public static void Load(ManualLogSource log)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (ControllerFamily family in Enum.GetValues(typeof(ControllerFamily)))
            foreach (string name in SpriteNames)
            {
                using (var stream = assembly.GetManifestResourceStream("LongDrive.Controller." + family + "." + name + ".png"))
                {
                    if (stream == null) { log.LogWarning("Missing controller sprite: " + family + "/" + name); continue; }
                    using (var data = new MemoryStream())
                    {
                        stream.CopyTo(data);
                        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true) { name = family + " " + name, filterMode = FilterMode.Trilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                        if (ImageConversion.LoadImage(texture, data.ToArray())) Textures[family + "/" + name] = texture;
                        else UnityEngine.Object.Destroy(texture);
                    }
                }
            }
            try { BuildSpriteAsset(); }
            catch (Exception ex) { asset = null; log.LogWarning("Inline controller sprites unavailable; using text fallback. " + ex.Message); }
            log.LogInfo("Controller glyphs: " + Textures.Count + "/81 embedded images; TMP atlas=" + (asset != null));
        }

        private static void BuildSpriteAsset()
        {
            if (Textures.Count == 0) return;
            var names = new List<string>();
            foreach (ControllerFamily family in Enum.GetValues(typeof(ControllerFamily)))
                foreach (string name in SpriteNames)
                    if (Textures.ContainsKey(family + "/" + name)) names.Add(family + "/" + name);
            var images = new Texture2D[names.Count];
            for (int i = 0; i < names.Count; i++) images[i] = Textures[names[i]];
            atlas = new Texture2D(4, 4, TextureFormat.RGBA32, true) { name = AssetName + " Atlas", filterMode = FilterMode.Trilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var rects = atlas.PackTextures(images, 4, 4096, false);
            var shader = Shader.Find("TextMeshPro/Sprite");
            if (shader == null) throw new InvalidOperationException("The game did not supply the TextMeshPro sprite shader.");
            asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.name = AssetName;
            asset.hideFlags = HideFlags.HideAndDontSave;
            asset.spriteSheet = atlas;
            // This installed TMP version exposes these serialization fields with internal setters.
            AccessTools.Field(typeof(TMP_SpriteAsset), "m_Version").SetValue(asset, "1.1.0");
            AccessTools.Field(typeof(TMP_SpriteAsset), "m_FaceInfo").SetValue(asset,
                new FaceInfo { pointSize = 100, scale = 1, ascentLine = 85, descentLine = -15, lineHeight = 115 });
            for (int i = 0; i < names.Count; i++)
            {
                var uv = rects[i];
                int x = Mathf.RoundToInt(uv.x * atlas.width), y = Mathf.RoundToInt(uv.y * atlas.height);
                int width = Mathf.RoundToInt(uv.width * atlas.width), height = Mathf.RoundToInt(uv.height * atlas.height);
                float aspect = images[i].width / (float)images[i].height;
                var glyph = new TMP_SpriteGlyph((uint)i, new GlyphMetrics(100 * aspect, 100, 0, 85, 100 * aspect + 8), new GlyphRect(x, y, width, height), 1, 0);
                Indices[names[i]] = i;
                asset.spriteGlyphTable.Add(glyph);
                asset.spriteCharacterTable.Add(new TMP_SpriteCharacter(0xE100u + (uint)i, asset, glyph) { name = names[i] });
            }
            material = new Material(shader) { name = AssetName + " Material", mainTexture = atlas, hideFlags = HideFlags.HideAndDontSave };
            asset.material = material;
            asset.hashCode = TMP_TextUtilities.GetSimpleHashCode(AssetName);
            asset.UpdateLookupTables();
            MaterialReferenceManager.AddSpriteAsset(asset.hashCode, asset);
            TMP_Text.OnSpriteAssetRequest += FindAsset;
        }
        private static TMP_SpriteAsset FindAsset(int hash, string name) => name == AssetName ? asset : null;

        internal static void SelectFamily(ControllerFamily family)
        {
            if (Family == family) return;
            Family = family; Revision++; RichCache.Clear();
            foreach (var text in UnityEngine.Object.FindObjectsOfType<TMP_Text>(true))
                if (ContainsMarkup(text.text)) text.text = Retarget(text.text);
            foreach (var text in UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Text>(true))
                if (ContainsMarkup(text.text)) text.text = Retarget(text.text);
        }
        internal static bool ContainsMarkup(string text) => text != null && text.Contains("<sprite=\"" + AssetName + "\"");
        internal static string Retarget(string text) => text == null ? null : System.Text.RegularExpressions.Regex.Replace(text,
            "<sprite=\"" + AssetName + "\" index=([0-9]+) tint=0>", match =>
            {
                int old = int.Parse(match.Groups[1].Value);
                if (old < 0 || old >= SpriteNames.Length * 3) return match.Value;
                string key = Key(SpriteNames[old % SpriteNames.Length]);
                return Indices.TryGetValue(key, out int index) ? "<sprite=\"" + AssetName + "\" index=" + index + " tint=0>" : match.Value;
            });

        public static string Tmp(string text)
        {

            if (RichCache.TryGetValue(text, out string existing)) return existing;
            var result = new StringBuilder();
            foreach (var run in GlyphTokens.Parse(text))
                if (asset != null && run.Sprite != null && Textures.ContainsKey(Key(run.Sprite))) result.Append("<sprite=\"" + AssetName + "\" index=" + Indices[Key(run.Sprite)] + " tint=0>");
                else result.Append(run.Sprite != null ? ControllerIdentity.ButtonName(run.Sprite, Family) : run.Text);
            return RichCache[text] = result.ToString();
        }

        public static bool Button(Rect area, string text, GUIStyle style, bool selected = false, bool glyphs = true, bool outline = false)
        {
            Color background = GUI.backgroundColor;
            GUI.backgroundColor = selected ? new Color(1, .98f, .82f) : Color.white;
            bool clicked = GUI.Button(area, GUIContent.none, style);
            GUI.backgroundColor = background;
            if (selected && GUI.enabled) GameMenuSkin.Focus(area);
            var textStyle = new GUIStyle(style);
            textStyle.normal.textColor = outline
                ? (selected ? new Color(.75f, .95f, .45f) : new Color(.98f, .96f, .86f))
                : (selected ? GameMenuSkin.SelectedInk : GameMenuSkin.Ink);
            // Compact tabs and binding rows need a complete glyph line even when
            // the skin's normal button padding would leave too little vertical room.
            int fontSize = style.fontSize > 0 ? style.fontSize : 17;
            float line = Mathf.Max(style.lineHeight, fontSize * 1.45f);
            float top = Mathf.Min(style.padding.top, Mathf.Max(0, (area.height - line) / 2));
            float bottom = Mathf.Min(style.padding.bottom, Mathf.Max(0, area.height - top - line));
            // Narrow Clear/+/-/trigger buttons still need room for their complete label.
            float left = Mathf.Min(style.padding.left, area.width < 150 ? 10 : style.padding.left);
            float right = Mathf.Min(style.padding.right, area.width < 150 ? 10 : style.padding.right);
            var content = new Rect(area.x + left, area.y + top, area.width - left - right, area.height - top - bottom);
            if (glyphs)
            {
                if (outline)
                {
                    // Keep two-line action cards inside the bright face of the sprite.
                    while (textStyle.fontSize > 13 && MeasureHeight(text, content.width, textStyle) > content.height)
                        textStyle.fontSize--;
                    float height = MeasureHeight(text, content.width, textStyle);
                    content.y += Mathf.Max(0, (content.height - height) / 2);
                }
                Label(content, text, textStyle, true, outline);
            }
            else
            {
                var plain = new GUIStyle(GUI.skin.label) { font = textStyle.font, fontSize = textStyle.fontSize, fontStyle = textStyle.fontStyle, padding = new RectOffset(), alignment = TextAnchor.MiddleLeft, wordWrap = textStyle.wordWrap };
                plain.normal.textColor = textStyle.normal.textColor;
                if (outline)
                    while (plain.fontSize > 13 && plain.CalcSize(new GUIContent(text)).x > content.width)
                        plain.fontSize--;
                DrawText(content, text, plain, outline);
            }
            return clicked;
        }

        public static float MeasureHeight(string text, float width, GUIStyle style) => Label(new Rect(0, 0, width, 100000), text, style, false);

        private struct LabelPart
        {
            internal float X, Width;
            internal int Row;
            internal string Text;
            internal Texture2D Image;
        }
        public static float Label(Rect area, string text, GUIStyle style, bool draw = true, bool outline = false)
        {
            // Lay out each complete line before drawing so text and button icons
            // share one center, including wrapped and explicit two-line captions.
            int fontSize = style.fontSize > 0 ? style.fontSize : 17;
            float line = Mathf.Max(style.lineHeight, fontSize * 1.45f);
            float iconHeight = fontSize * 1.35f;
            var labelStyle = new GUIStyle(GUI.skin.label) { font = style.font, fontSize = fontSize, fontStyle = style.fontStyle, padding = new RectOffset(), alignment = TextAnchor.MiddleLeft, wordWrap = false };
            labelStyle.normal.textColor = style.normal.textColor;
            var parts = new List<LabelPart>();
            var widths = new List<float> { 0 };
            float x = 0;
            int row = 0;
            void NewLine() { row++; widths.Add(0); x = 0; }
            void Add(float width, string word, Texture2D image)
            {
                if (x > 0 && x + width > area.width) NewLine();
                parts.Add(new LabelPart { X = x, Row = row, Width = width, Text = word, Image = image });
                widths[row] = x + width;
                x += width + (image != null ? 2 : 0);
            }
            foreach (var run in GlyphTokens.Parse(text))
            {
                if (run.Sprite != null && Textures.TryGetValue(Key(run.Sprite), out var image))
                {
                    Add(iconHeight * image.width / image.height, null, image);
                    continue;
                }
                foreach (string token in System.Text.RegularExpressions.Regex.Split(run.Sprite != null ? ControllerIdentity.ButtonName(run.Sprite, Family) : run.Text, @"(\n|[ \t]+)"))
                {
                    if (token.Length == 0) continue;
                    if (token == "\n") { NewLine(); continue; }
                    float width = labelStyle.CalcSize(new GUIContent(token)).x;
                    if (string.IsNullOrWhiteSpace(token)) { if (x > 0) x += width; }
                    else Add(width, token, null);
                }
            }
            bool centered = style.alignment == TextAnchor.UpperCenter || style.alignment == TextAnchor.MiddleCenter || style.alignment == TextAnchor.LowerCenter;
            if (draw)
                foreach (var part in parts)
                {
                    float y = area.y + part.Row * line;
                    if (y + line > area.yMax + 1) continue;
                    float offset = centered ? Mathf.Max(0, (area.width - widths[part.Row]) / 2) : 0;
                    float left = area.x + offset + part.X;
                    if (part.Image != null)
                        GUI.DrawTexture(new Rect(left, y + (line - iconHeight) / 2, part.Width, iconHeight), part.Image, ScaleMode.ScaleToFit, true);
                    else DrawText(new Rect(left, y, part.Width + 1, line), part.Text, labelStyle, outline);
                }
            return widths.Count * line;
        }

        private static void DrawText(Rect area, string text, GUIStyle style, bool outline)
        {
            if (outline)
            {
                Color ink = style.normal.textColor;
                style.normal.textColor = Color.black;
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (dx != 0 || dy != 0)
                            GUI.Label(new Rect(area.x + dx, area.y + dy, area.width, area.height), text, style);
                style.normal.textColor = ink;
            }
            GUI.Label(area, text, style);
        }

        public static void Verify(ManualLogSource log)
        {
            if (asset == null) { log.LogWarning("TMP glyph verification: atlas unavailable."); return; }
            GameObject canvasObject = null;
            try
            {
                // Render only into TMP's mesh data on a disabled canvas. No visible panel,
                // focus change, navigation, or input is generated by this check.
                canvasObject = new GameObject("Controller glyph verification", typeof(RectTransform), typeof(Canvas));
                canvasObject.hideFlags = HideFlags.HideAndDontSave;
                canvasObject.GetComponent<Canvas>().enabled = false;
                var textObject = new GameObject("Glyph probe", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(canvasObject.transform, false);
                var text = textObject.GetComponent<TextMeshProUGUI>();
                text.raycastTarget = false;
                text.font = TMP_Settings.defaultFontAsset;
                text.fontSize = 32;
                text.rectTransform.sizeDelta = new Vector2(1600, 200);
                var expectedNames = new List<string>(Textures.Keys);
                var content = new StringBuilder();
                foreach (string name in expectedNames) content.Append("<sprite=\"" + AssetName + "\" index=" + Indices[name] + " tint=0> ");
                text.text = content.ToString();
                text.ForceMeshUpdate(true, true);
                int parsed = 0;
                // This game's TMP never updates m_spriteCount during mesh generation;
                // inspect the actual parsed elements rather than its stale summary counter.
                for (int i = 0; i < text.textInfo.characterCount; i++)
                {
                    var character = text.textInfo.characterInfo[i];
                    if (character.elementType != TMP_TextElementType.Sprite) continue;
                    string expected = parsed < expectedNames.Count ? expectedNames[parsed] : "unexpected extra sprite";
                    int expectedIndex = Indices.TryGetValue(expected, out int index) ? index : -1;
                    if (character.spriteAsset != asset || character.spriteIndex != expectedIndex ||
                        character.textElement != asset.spriteCharacterTable[expectedIndex])
                        throw new InvalidOperationException("Sprite identity mismatch: expected " + expected +
                            " at " + expectedIndex + ", parsed " + character.spriteIndex);
                    parsed++;
                }
                if (parsed != expectedNames.Count) throw new InvalidOperationException("Missing parsed sprites: " + parsed + "/" + expectedNames.Count);
                log.LogInfo("TMP glyph verification: " + parsed + "/81 sprite identities verified; diagnostic overlay removed.");
            }
            catch (Exception ex) { log.LogWarning("TMP glyph verification failed: " + ex); }
            finally { if (canvasObject != null) UnityEngine.Object.Destroy(canvasObject); }
        }

        public static void Dispose()
        {
            TMP_Text.OnSpriteAssetRequest -= FindAsset;
            foreach (var texture in Textures.Values) UnityEngine.Object.Destroy(texture);
            Textures.Clear(); RichCache.Clear(); Indices.Clear();
            if (asset != null) UnityEngine.Object.Destroy(asset);
            if (material != null) UnityEngine.Object.Destroy(material);
            if (atlas != null) UnityEngine.Object.Destroy(atlas);
            asset = null; material = null; atlas = null;
        }
    }

    [HarmonyPatch(typeof(TMP_Text), "set_text")]
    internal static class GlyphRichTextPatch
    {
        private static void Prefix(TMP_Text __instance, string value)
        {
            if (value != null && value.IndexOf("<sprite=\"TLDController\"", StringComparison.Ordinal) >= 0) __instance.richText = true;
        }
    }
}
