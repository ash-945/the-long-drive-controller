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
        private const string AssetName = "TLDXbox";
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, int> Indices = new Dictionary<string, int>();
        private static readonly Dictionary<string, string> RichCache = new Dictionary<string, string>();
        private static TMP_SpriteAsset asset;
        private static Texture2D atlas;
        private static Material material;
        public static readonly string[] SpriteNames = { "A", "B", "X", "Y", "LB", "RB", "LT", "RT", "L3", "R3", "Select", "Start", "DPad_UP", "DPad_Down", "DPad_Left", "DPad_Right" };

        public static void Load(ManualLogSource log)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (string name in SpriteNames)
            {
                using (var stream = assembly.GetManifestResourceStream("LongDrive.Controller.Xbox." + name + ".png"))
                {
                    if (stream == null) { log.LogWarning("Missing controller sprite: " + name); continue; }
                    using (var data = new MemoryStream())
                    {
                        stream.CopyTo(data);
                        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true) { name = "Xbox " + name, filterMode = FilterMode.Trilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                        if (ImageConversion.LoadImage(texture, data.ToArray())) Textures[name] = texture;
                        else UnityEngine.Object.Destroy(texture);
                    }
                }
            }
            try { BuildSpriteAsset(); }
            catch (Exception ex) { asset = null; log.LogWarning("Inline controller sprites unavailable; using text fallback. " + ex.Message); }
            log.LogInfo("Xbox glyphs: " + Textures.Count + "/16 embedded images; TMP atlas=" + (asset != null));
        }

        private static void BuildSpriteAsset()
        {
            if (Textures.Count == 0) return;
            var names = new List<string>(Textures.Keys);
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

        public static string Tmp(string text)
        {
            if (asset == null) return text;
            if (RichCache.TryGetValue(text, out string existing)) return existing;
            var result = new StringBuilder();
            foreach (var run in GlyphTokens.Parse(text))
                if (run.Sprite != null && Textures.ContainsKey(run.Sprite)) result.Append("<sprite=\"" + AssetName + "\" index=" + Indices[run.Sprite] + " tint=0>");
                else result.Append(run.Text);
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
            textStyle.normal.textColor = selected ? GameMenuSkin.SelectedInk : GameMenuSkin.Ink;
            // Compact tabs and binding rows need a complete glyph line even when
            // the skin's normal button padding would leave too little vertical room.
            int fontSize = style.fontSize > 0 ? style.fontSize : 17;
            float line = Mathf.Max(style.lineHeight, fontSize * 1.45f);
            float top = Mathf.Min(style.padding.top, Mathf.Max(0, (area.height - line) / 2));
            float bottom = Mathf.Min(style.padding.bottom, Mathf.Max(0, area.height - top - line));
            var content = new Rect(area.x + style.padding.left, area.y + top, area.width - style.padding.horizontal, area.height - top - bottom);
            if (glyphs)
            {
                if (outline)
                {
                    // Keep two-line action cards inside the bright face of the sprite.
                    textStyle.normal.textColor = selected ? new Color(.75f, .95f, .45f) : new Color(.98f, .96f, .86f);
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
                GUI.Label(content, text, plain);
            }
            return clicked;
        }

        public static float MeasureHeight(string text, float width, GUIStyle style) => Label(new Rect(0, 0, width, 100000), text, style, false);

        public static float Label(Rect area, string text, GUIStyle style, bool draw = true, bool outline = false)
        {
            // Use fixed inline boxes with preserved aspect ratios, then wrap normal text at
            // spaces. Font labels and game settings are never parsed as controller commands.
            int fontSize = style.fontSize > 0 ? style.fontSize : 17;
            float line = Mathf.Max(style.lineHeight, fontSize * 1.45f);
            float iconHeight = fontSize * 1.35f;
            float x = area.x, y = area.y;
            var labelStyle = new GUIStyle(GUI.skin.label) { font = style.font, fontSize = fontSize, fontStyle = style.fontStyle, padding = new RectOffset(), alignment = TextAnchor.MiddleLeft, wordWrap = false };
            labelStyle.normal.textColor = style.normal.textColor;
            foreach (var run in GlyphTokens.Parse(text))
            {
                if (run.Sprite != null && Textures.TryGetValue(run.Sprite, out var image))
                {
                    float width = iconHeight * image.width / image.height;
                    if (x > area.x && x + width > area.xMax) { x = area.x; y += line; }
                    if (draw && y + line <= area.yMax + 1) GUI.DrawTexture(new Rect(x, y + (line - iconHeight) / 2, width, iconHeight), image, ScaleMode.ScaleToFit, true);
                    x += width + 2;
                    continue;
                }
                foreach (string token in System.Text.RegularExpressions.Regex.Split(run.Text, @"(\n|[ \t]+)"))
                {
                    if (token.Length == 0) continue;
                    if (token == "\n") { x = area.x; y += line; continue; }
                    float width = labelStyle.CalcSize(new GUIContent(token)).x;
                    bool space = string.IsNullOrWhiteSpace(token);
                    if (!space && x > area.x && x + width > area.xMax) { x = area.x; y += line; }
                    if (draw && !space && y + line <= area.yMax + 1)
                    {
                        var word = new Rect(x, y, width + 1, line);
                        if (outline)
                        {
                            Color ink = labelStyle.normal.textColor;
                            labelStyle.normal.textColor = Color.black;
                            for (int dx = -1; dx <= 1; dx++)
                                for (int dy = -1; dy <= 1; dy++)
                                    if (dx != 0 || dy != 0)
                                        GUI.Label(new Rect(word.x + dx, word.y + dy, word.width, word.height), token, labelStyle);
                            labelStyle.normal.textColor = ink;
                        }
                        GUI.Label(word, token, labelStyle);
                    }
                    if (x > area.x || !space) x += width;
                }
            }
            return y + line - area.y;
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
                var content = new StringBuilder();
                foreach (string name in SpriteNames) content.Append("<sprite=\"" + AssetName + "\" index=" + Indices[name] + " tint=0> ");
                text.text = content.ToString();
                text.ForceMeshUpdate(true, true);
                int parsed = 0;
                // This game's TMP never updates m_spriteCount during mesh generation;
                // inspect the actual parsed elements rather than its stale summary counter.
                for (int i = 0; i < text.textInfo.characterCount; i++)
                    if (text.textInfo.characterInfo[i].elementType == TMP_TextElementType.Sprite) parsed++;
                log.LogInfo("TMP glyph verification: " + parsed + "/16 sprites parsed; diagnostic overlay removed.");
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
            if (value != null && value.IndexOf("<sprite=\"TLDXbox\"", StringComparison.Ordinal) >= 0) __instance.richText = true;
        }
    }
}
