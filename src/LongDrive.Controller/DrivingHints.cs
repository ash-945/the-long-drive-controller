using UnityEngine;

namespace LongDrive.Controller
{
    internal static class DrivingHints
    {
        private static readonly string[] Rows =
        {
            "LS: Steer     RT: Throttle     LT: Brake     RS: Look",
            "B: Ignition (hold)     A: Handbrake     LS Click: Clutch",
            "LB / RB: Shift down / up     View > Gears: Select gear",
            "D-pad Up: Lights     D-pad Down: Horn     X: Use / exit"
        };
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
            float width = 480, height = 98;
            var area = new Rect((Screen.width / scale - width) / 2, Screen.height / scale - height - 12, width, height);
            var color = GUI.color;
            GUI.color = new Color(.035f, .045f, .055f, .78f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = color;
            for (int i = 0; i < Rows.Length; i++)
                ControllerGlyphs.Label(new Rect(area.x + 10, area.y + 7 + i * 21, width - 20, 21), Rows[i], textStyle);
            GUI.matrix = before;
        }
    }
}
