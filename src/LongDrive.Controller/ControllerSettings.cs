using UnityEngine;

namespace LongDrive.Controller
{
    internal sealed class ControllerSettings
    {
        public bool Open { get; private set; }
        private int selected;
        private readonly PanelNavigation navigation = new PanelNavigation();
        private float nextAdjustment;
        public void Show() { Open = true; }
        public void Close() { Open = false; }
        public void Tick(Plugin plugin, PadState pad, PadState previous)
        {
            if (!Open) return;
            bool Down(Buttons b) => pad.Held(b) && !previous.Held(b);
            if (Down(Buttons.B) || Down(Buttons.Start)) { Close(); plugin.MenuActivated(); return; }
            // Up/down selects a setting; left/right adjusts without opening another screen.
            var vertical = pad;
            vertical.Buttons &= ~(Buttons.Left | Buttons.Right);
            vertical.LX = 0;
            selected = (selected + navigation.Move(vertical, Time.unscaledTime, 1) + plugin.SettingCount) % plugin.SettingCount;
            int delta = pad.Held(Buttons.Left) || pad.LX < -.5f ? -1 : pad.Held(Buttons.Right) || pad.LX > .5f ? 1 : 0;
            bool fresh = Down(Buttons.Left) || Down(Buttons.Right) || Mathf.Abs(pad.LX) > .5f && Mathf.Abs(previous.LX) <= .5f;
            if (delta != 0 && (fresh || Time.unscaledTime >= nextAdjustment))
            { plugin.AdjustSetting(selected, delta); nextAdjustment = Time.unscaledTime + .15f; }
            if (Down(Buttons.A)) plugin.AdjustSetting(selected, selected == 6 ? 0 : 1);
            if (Down(Buttons.Y)) plugin.ResetControllerSettings();
        }
        public void Draw(Plugin plugin)
        {
            if (!Open) return;
            var area = PanelStyle.Begin(920, 610);
            GUI.Label(new Rect(area.x + 24, area.y + 14, 870, 42), "Controller settings", PanelStyle.Title);
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 60, 870, 40), "[D-pad / LS] select and adjust     [A] increase / toggle     [Y] defaults", PanelStyle.Text);
            for (int i = 0; i < plugin.SettingCount; i++)
            {
                float y = area.y + 110 + i * 45;
                if (PanelStyle.Choice(new Rect(area.x + 24, y, 700, 39), plugin.SettingText(i), selected == i)) selected = i;
                if (PanelStyle.Choice(new Rect(area.x + 736, y, 70, 39), "-", false)) { selected = i; plugin.AdjustSetting(i, -1); }
                if (PanelStyle.Choice(new Rect(area.x + 818, y, 70, 39), "+", false)) { selected = i; plugin.AdjustSetting(i, 1); }
            }
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 535, 870, 60), "Settings save automatically. Sprint: LS click, stops when movement stops.\nCrouch: B toggles. [B / Start / View] closes this screen.", PanelStyle.Text);
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
