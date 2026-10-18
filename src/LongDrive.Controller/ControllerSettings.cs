using UnityEngine;

namespace LongDrive.Controller
{
    internal sealed class ControllerSettings
    {
        public bool Open { get; private set; }
        internal string CaptureState => page == 0 ? "settings" : capture.Active ? "binding-capture" : pending != null ? "binding-conflict" : "bindings-" + (driving ? "driving" : "walking");
        private int selected, page, category;
        private bool driving, confirmReset;
        private string message = "";
        private readonly PanelNavigation navigation = new PanelNavigation();
        private readonly BindingCapture capture = new BindingCapture();
        private ControlChord pending;
        private GameAction? conflict;
        private float nextAdjustment;
        private GameAction Selected => ActionCatalog.Groups[category][selected];
        public void Show() { Open = true; }
        public void ShowBinding(GameAction action, bool drivingProfile)
        {
            Open = true; ChangePage(1); driving = drivingProfile;
            category = ActionCatalog.Page(action);
            selected = System.Array.IndexOf(ActionCatalog.Groups[category], action);
        }
        public void Close() { Open = false; capture.Cancel(); pending = null; confirmReset = false; }
        private void ChangePage(int value) { page = value; selected = 0; message = ""; capture.Cancel(); pending = null; confirmReset = false; }
        private void StartCapture() { pending = null; confirmReset = false; message = ""; capture.Begin(); }
        private void Apply(Plugin plugin, ControlChord chord)
        {
            ActionCatalog.Bindings.Assign(Selected, driving, chord);
            plugin.SaveBindings();
            message = ActionCatalog.Label(Selected) + ": " + chord.Label;
            pending = null;
        }
        private void Propose(Plugin plugin, ControlChord chord)
        {
            conflict = ActionCatalog.Bindings.Conflict(Selected, driving, chord);
            if (conflict.HasValue) pending = chord;
            else Apply(plugin, chord);
        }
        public void Tick(Plugin plugin, PadState pad, PadState previous)
        {
            if (!Open) return;
            bool Down(Buttons b) => pad.Held(b) && !previous.Held(b);
            if (capture.Active)
            {
                if (Down(Buttons.Start)) { capture.Cancel(); message = "Binding cancelled."; return; }
                if (capture.Tick(pad, out var chord)) Propose(plugin, chord);
                return;
            }
            if (pending != null || confirmReset)
            {
                if (Down(Buttons.B) || Down(Buttons.Start)) { pending = null; confirmReset = false; return; }
                if (Down(Buttons.A)) Confirm(plugin);
                return;
            }
            if (Down(Buttons.B) || Down(Buttons.Start)) { Close(); plugin.MenuActivated(); return; }
            if (Down(Buttons.LB) || Down(Buttons.RB)) { ChangePage(1 - page); return; }
            if (page == 1)
            {
                if (pad.LT > .6f && previous.LT <= .6f || pad.RT > .6f && previous.RT <= .6f)
                { category = (category + (pad.RT > .6f ? 1 : ActionCatalog.Groups.Length - 1)) % ActionCatalog.Groups.Length; selected = 0; message = ""; return; }
                if (Down(Buttons.X)) { driving = !driving; message = ""; return; }
                selected = (selected + navigation.Move(pad, Time.unscaledTime, 1) + ActionCatalog.Groups[category].Length) % ActionCatalog.Groups[category].Length;
                if (Down(Buttons.A)) StartCapture();
                else if (Down(Buttons.RS)) Apply(plugin, ControlChord.Empty);
                else if (Down(Buttons.Y)) confirmReset = true;
                return;
            }
            var vertical = pad;
            vertical.Buttons &= ~(Buttons.Left | Buttons.Right);
            vertical.LX = 0;
            selected = (selected + navigation.Move(vertical, Time.unscaledTime, 1) + plugin.SettingCount) % plugin.SettingCount;
            int delta = pad.Held(Buttons.Left) || pad.LX < -.5f ? -1 : pad.Held(Buttons.Right) || pad.LX > .5f ? 1 : 0;
            bool fresh = Down(Buttons.Left) || Down(Buttons.Right) || Mathf.Abs(pad.LX) > .5f && Mathf.Abs(previous.LX) <= .5f;
            if (delta != 0 && (fresh || Time.unscaledTime >= nextAdjustment))
            { plugin.AdjustSetting(selected, delta); nextAdjustment = Time.unscaledTime + .15f; }
            if (Down(Buttons.A)) plugin.AdjustSetting(selected, selected == 6 || selected == 9 ? 0 : 1);
            if (Down(Buttons.Y)) plugin.ResetControllerSettings();
        }
        private void Confirm(Plugin plugin)
        {
            if (confirmReset)
            {
                ActionCatalog.Bindings.Reset(driving); plugin.SaveBindings();
                confirmReset = false; message = "Defaults restored for this profile.";
            }
            else if (pending != null) Apply(plugin, pending);
        }
        public void Draw(Plugin plugin)
        {
            if (!Open) return;
            var area = PanelStyle.Begin(920, 610);
            PanelStyle.Heading(new Rect(area.x + 24, area.y + 8, 720, 40), "Controller settings");
            if (PanelStyle.Choice(new Rect(area.x + 764, area.y + 10, 128, 34), "Close [B]", false, true)) { Close(); plugin.MenuActivated(); }
            if (PanelStyle.Choice(new Rect(area.x + 24, area.y + 52, 430, 35), "General  [LB / RB]", page == 0, true)) ChangePage(0);
            if (PanelStyle.Choice(new Rect(area.x + 464, area.y + 52, 430, 35), "Bindings  [LB / RB]", page == 1, true)) ChangePage(1);
            if (page == 0)
            {
                ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 96, 870, 35), "[D-pad / LS] select and adjust    [A] increase / toggle    [Y] defaults", PanelStyle.Text);
                for (int i = 0; i < plugin.SettingCount; i++)
                {
                    float y = area.y + 140 + i * 32;
                    if (PanelStyle.Choice(new Rect(area.x + 24, y, 700, 28), plugin.SettingText(i), selected == i)) selected = i;
                    if (PanelStyle.Choice(new Rect(area.x + 736, y, 70, 28), "-", false)) { selected = i; plugin.AdjustSetting(i, -1); }
                    if (PanelStyle.Choice(new Rect(area.x + 818, y, 70, 28), "+", false)) { selected = i; plugin.AdjustSetting(i, 1); }
                }
                ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 535, 870, 60), "Settings save automatically. Sprint: " + ActionCatalog.Hint(GameAction.run, false) + ".\nCrouch: " + ActionCatalog.Hint(GameAction.duck, false) + ". [B / Start / View] closes.", PanelStyle.Text);
            }
            else DrawBindings(plugin, area);
            GUI.matrix = Matrix4x4.identity;
        }
        private void DrawBindings(Plugin plugin, Rect area)
        {
            bool busy = capture.Active || pending != null || confirmReset;
            GUI.enabled = !busy;
            if (PanelStyle.Choice(new Rect(area.x + 24, area.y + 96, 280, 36), (driving ? "Driving" : "On foot") + "  [X] change", false, true)) { driving = !driving; message = ""; }
            if (PanelStyle.Choice(new Rect(area.x + 316, area.y + 96, 55, 36), "LT", false, true)) { category = (category + ActionCatalog.Groups.Length - 1) % ActionCatalog.Groups.Length; selected = 0; }
            GUI.Label(new Rect(area.x + 385, area.y + 98, 420, 34), ActionCatalog.Pages[category], PanelStyle.Text);
            if (PanelStyle.Choice(new Rect(area.x + 830, area.y + 96, 58, 36), "RT", false, true)) { category = (category + 1) % ActionCatalog.Groups.Length; selected = 0; }
            var group = ActionCatalog.Groups[category];
            for (int i = 0; i < group.Length; i++)
            {
                float y = area.y + 146 + i * 42;
                // Action names are plain text; only the trusted binding label uses glyphs.
                if (PanelStyle.Choice(new Rect(area.x + 24, y, 380, 37), ActionCatalog.Label(group[i]), selected == i)) { selected = i; StartCapture(); }
                if (PanelStyle.Choice(new Rect(area.x + 414, y, 390, 37), ActionCatalog.Bindings.Get(group[i], driving).Label, selected == i, true)) { selected = i; StartCapture(); }
                if (PanelStyle.Choice(new Rect(area.x + 816, y, 72, 37), "Clear", false)) { selected = i; Apply(plugin, ControlChord.Empty); }
            }
            GUI.enabled = true;
            string help = capture.Active ? ActionCatalog.Label(Selected) + ": " + capture.Error :
                pending != null ? pending.Label + " is used by " + ActionCatalog.Label(conflict.Value) + ". [A] swap bindings; [B] cancel." :
                confirmReset ? "Restore all " + (driving ? "driving" : "on-foot") + " bindings? [A] restore; [B] cancel." :
                message.Length > 0 ? message : "[A] rebind   [RS Click] clear   [Y] profile defaults. Start / View stay reserved for menus.";
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 490, 870, 60), help, PanelStyle.Text);
            if (pending != null || confirmReset)
            {
                if (PanelStyle.Choice(new Rect(area.x + 24, area.y + 552, 420, 34), "Confirm  [A]", true, true)) Confirm(plugin);
                if (PanelStyle.Choice(new Rect(area.x + 460, area.y + 552, 428, 34), "Cancel  [B]", false, true)) { pending = null; confirmReset = false; }
            }
            else if (capture.Active)
            {
                if (PanelStyle.Choice(new Rect(area.x + 24, area.y + 552, 864, 34), "Cancel capture  [Start / View]", false, true)) capture.Cancel();
            }
            else ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 559, 870, 35), "Release controls to save a binding. [LT / RT] category. [B / Start / View] closes.", PanelStyle.Text);
        }
    }
}
