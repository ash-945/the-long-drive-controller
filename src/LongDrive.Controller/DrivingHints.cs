using UnityEngine;

namespace LongDrive.Controller
{
    internal static class DrivingHints
    {
        private static string[] rows;
        private static int revision = -1;
        private static bool pointerSteer, pointerPedals;
        private static GUIStyle textStyle;
        public static void Draw(Plugin plugin)
        {
            if (!plugin.ControllerConnected || !plugin.Driving || !plugin.InGameplay || plugin.CustomPanel ||
                mainscript.s == null || !mainscript.s.showcrosshair2 || mainscript.s.sleeping || mainscript.s.player.died) return;
            PanelStyle.Init();
            if (textStyle == null) textStyle = new GUIStyle(PanelStyle.Text) { fontSize = 13 };
            var before = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            bool steer = inputscript.i != null && inputscript.i.mouseSteer;
            bool pedals = inputscript.i != null && inputscript.i.mousePedal;
            if (rows == null || revision != ActionCatalog.Bindings.Revision || steer != pointerSteer || pedals != pointerPedals)
            {
                rows = BindingHints.DrivingRows(ActionCatalog.Bindings, steer, pedals);
                revision = ActionCatalog.Bindings.Revision; pointerSteer = steer; pointerPedals = pedals;
            }
            float width = 480, height = 14;
            foreach (string row in rows) height += Mathf.Max(21, ControllerGlyphs.MeasureHeight(row, width - 20, textStyle));
            var area = new Rect((Screen.width / scale - width) / 2, Screen.height / scale - height - 12, width, height);
            var color = GUI.color;
            GUI.color = new Color(.035f, .045f, .055f, .78f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = color;
            float y = area.y + 7;
            foreach (string row in rows)
            {
                float rowHeight = Mathf.Max(21, ControllerGlyphs.MeasureHeight(row, width - 20, textStyle));
                ControllerGlyphs.Label(new Rect(area.x + 10, y, width - 20, rowHeight), row, textStyle);
                y += rowHeight;
            }
            GUI.matrix = before;
        }
    }
}
