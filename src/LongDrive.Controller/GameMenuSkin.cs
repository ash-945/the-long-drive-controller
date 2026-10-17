using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LongDrive.Controller
{
    // Sample loaded UI artwork, never game files. Cropping via a render target also
    // works for non-readable/atlas textures under both Windows and Proton.
    internal static class GameMenuSkin
    {
        private static readonly List<Texture2D> owned = new List<Texture2D>();
        private static readonly Dictionary<int, Texture2D> crops = new Dictionary<int, Texture2D>();
        private static Texture2D paper, buttonTexture, panelTexture, frameTexture;
        private static GUIStyle paperStyle, panelStyle, frameStyle;
        private static RectOffset buttonBorder;
        private static bool initialized, sampled, reported;
        private static float retryAfter;
        internal static int Revision { get; private set; }
        internal static Font Font { get; private set; }
        internal static readonly Color FocusColor = new Color(.04f, .24f, .85f, 1f);
        internal static readonly Color Ink = new Color(.22f, .19f, .14f);
        internal static readonly Color SelectedInk = new Color(.17f, .38f, .06f);

        internal static void Init()
        {
            if (!initialized)
            {
                paper = MakePaper(128, 128, false);
                buttonTexture = MakePaper(144, 48, true);
                paperStyle = Background(paper, new RectOffset(8, 8, 8, 8));
                buttonBorder = new RectOffset(8, 8, 8, 8);
                initialized = true;
            }
            if (sampled || Time.unscaledTime < retryAfter) return;
            retryAfter = Time.unscaledTime + 2;
            var menu = menuhandler.s;
            if (menu == null) return;
            try
            {
                var root = menu.ButtonMainMenu != null ? menu.ButtonMainMenu : menu.ButtonExit;
                var button = root == null ? null : root.GetComponentInChildren<Button>(true);
                if (button == null) return;
                var image = button.targetGraphic as Image;
                if (image == null) image = button.GetComponent<Image>();
                if (image == null || image.sprite == null) return;
                var texture = Crop(image.sprite);
                if (texture == null) return;
                buttonTexture = texture;
                buttonBorder = Border(image.sprite, texture);
                var label = button.GetComponentInChildren<Text>(true);
                if (label != null) Font = label.font;
                if (Font == null)
                {
                    var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(true);
                    if (tmp != null && tmp.font != null) Font = tmp.font.sourceFontFile;
                }
                var background = menu.menuOverlayBG;
                var frame = menu.menuOverlayBGKeret;
                if (background != null && background.sprite != null)
                {
                    panelTexture = Crop(background.sprite);
                    if (panelTexture != null) panelStyle = Background(panelTexture, Border(background.sprite, panelTexture));
                }
                if (frame != null && frame.sprite != null)
                {
                    frameTexture = Crop(frame.sprite);
                    if (frameTexture != null) frameStyle = Background(frameTexture, Border(frame.sprite, frameTexture));
                }
                sampled = true; Revision++;
                if (!reported)
                {
                    Plugin.Instance?.LoggerForSkin("Menu skin: native button=" + image.sprite.name + "; panel=" + (background != null && background.sprite != null ? background.sprite.name : "paper fallback") + "; frame=" + (frame != null && frame.sprite != null ? frame.sprite.name : "none") + "; font=" + (Font != null ? Font.name : "GUI font"));
                    reported = true;
                }
            }
            catch (Exception ex)
            {
                // A fallback remains readable if a different renderer/atlas cannot be
                // sampled. Do not allocate or throw repeatedly every GUI frame.
                sampled = true;
                Plugin.Instance?.LoggerForSkin("Menu skin: using paper fallback. " + ex.Message);
            }
        }
        internal static void ApplyButton(GUIStyle style)
        {
            style.border = buttonBorder;
            foreach (var state in new[] { style.normal, style.hover, style.active, style.focused, style.onNormal, style.onHover, style.onActive, style.onFocused })
            { state.background = buttonTexture; state.textColor = Ink; }
        }
        internal static void DrawPanel(Rect area)
        {
            if (Event.current.type != EventType.Repaint) return;
            Color saved = GUI.color;
            GUI.color = new Color(0, 0, 0, .24f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width / GUI.matrix.m00, Screen.height / GUI.matrix.m11), Texture2D.whiteTexture);
            GUI.color = new Color(0, 0, 0, .30f);
            GUI.DrawTexture(new Rect(area.x + 7, area.y + 9, area.width, area.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            paperStyle.Draw(area, GUIContent.none, false, false, false, false);
            panelStyle?.Draw(area, GUIContent.none, false, false, false, false);
            frameStyle?.Draw(area, GUIContent.none, false, false, false, false);
            GUI.color = saved;
        }
        internal static void Focus(Rect area)
        {
            Color saved = GUI.color;
            GUI.color = FocusColor;
            GUI.DrawTexture(new Rect(area.x + 3, area.y + 3, area.width - 6, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(area.x + 3, area.yMax - 6, area.width - 6, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(area.x + 3, area.y + 3, 3, area.height - 6), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(area.xMax - 6, area.y + 3, 3, area.height - 6), Texture2D.whiteTexture);
            GUI.color = saved;
        }
        private static GUIStyle Background(Texture2D texture, RectOffset border)
        {
            var result = new GUIStyle { border = border };
            result.normal.background = texture;
            return result;
        }
        private static RectOffset Border(Sprite sprite, Texture2D texture)
        {
            var b = sprite.border;
            // Preserve corners even on native simple sprites that do not declare a
            // sliced border. All measurements remain in reference GUI pixels.
            if (b == Vector4.zero)
            {
                int inset = Mathf.Max(1, Mathf.Min(12, Mathf.Min(texture.width, texture.height) / 6));
                return new RectOffset(inset, inset, inset, inset);
            }
            return new RectOffset(Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.z), Mathf.RoundToInt(b.w), Mathf.RoundToInt(b.y));
        }
        private static Texture2D Crop(Sprite sprite)
        {
            if (crops.TryGetValue(sprite.GetInstanceID(), out var existing)) return existing;
            var rect = sprite.textureRect;
            var source = sprite.texture;
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width)), height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
            var target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            Texture2D texture = null;
            try
            {
                Graphics.Blit(source, target, new Vector2(rect.width / source.width, rect.height / source.height), new Vector2(rect.x / source.width, rect.y / source.height));
                RenderTexture.active = target;
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "Controller menu " + sprite.name, hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply(false, true);
                crops[sprite.GetInstanceID()] = texture;
                owned.Add(texture);
                return texture;
            }
            catch { if (texture != null) UnityEngine.Object.Destroy(texture); throw; }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); }
        }
        private static Texture2D MakePaper(int width, int height, bool raised)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "Controller menu paper fallback", hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[width * height];
            var random = new System.Random(20241126);
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                float grain = (float)random.NextDouble() * .045f - .0225f;
                float edge = raised ? (y >= height - 4 || x < 3 ? .07f : y < 4 || x >= width - 3 ? -.13f : 0) : 0;
                pixels[y * width + x] = new Color(.80f + grain + edge, .78f + grain + edge, .66f + grain + edge, 1);
            }
            texture.SetPixels(pixels); texture.Apply(false, true); owned.Add(texture);
            return texture;
        }
        internal static void Dispose()
        {
            foreach (var texture in owned) UnityEngine.Object.Destroy(texture);
            owned.Clear(); crops.Clear();
            panelStyle = frameStyle = paperStyle = null;
            paper = buttonTexture = panelTexture = frameTexture = null;
            initialized = sampled = reported = false; retryAfter = 0; Font = null; Revision++;
        }
    }
}
