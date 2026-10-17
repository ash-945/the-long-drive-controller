using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LongDrive.Controller
{
    internal sealed class PanelNavigation
    {
        private int last;
        private float repeat;
        public int Move(PadState pad, float now, int columns)
        {
            int direction = pad.Held(Buttons.Up) || pad.LY > .5f ? -columns : pad.Held(Buttons.Down) || pad.LY < -.5f ? columns :
                pad.Held(Buttons.Left) || pad.LX < -.5f ? -1 : pad.Held(Buttons.Right) || pad.LX > .5f ? 1 : 0;
            bool fresh = direction != last;
            last = direction;
            if (direction == 0 || !fresh && now < repeat) return 0;
            repeat = now + (fresh ? .3f : .12f);
            return direction;
        }
    }

    internal static class PanelStyle
    {
        public static GUIStyle Title, Text, Button;
        private static int skinRevision = -1;
        public static void Init()
        {
            GameMenuSkin.Init();
            if (Title != null && skinRevision == GameMenuSkin.Revision) return;
            skinRevision = GameMenuSkin.Revision;
            Title = new GUIStyle(GUI.skin.label) { font = GameMenuSkin.Font, fontSize = 28, fontStyle = FontStyle.Bold };
            Text = new GUIStyle(GUI.skin.label) { font = GameMenuSkin.Font, fontSize = 17, wordWrap = true };
            Button = new GUIStyle(GUI.skin.button) { font = GameMenuSkin.Font, fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(14, 10, 6, 6) };
            Title.normal.textColor = Text.normal.textColor = GameMenuSkin.Ink;
            GameMenuSkin.ApplyButton(Button);
        }
        public static Rect Begin(float width, float height)
        {
            Init();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            var area = new Rect((Screen.width / scale - width) / 2, (Screen.height / scale - height) / 2, width, height);
            GameMenuSkin.DrawPanel(area);
            return area;
        }
        public static void Heading(Rect area, string text)
        {
            var highlight = new GUIStyle(Title);
            highlight.normal.textColor = new Color(.98f, .96f, .85f, .75f);
            GUI.Label(new Rect(area.x + 1, area.y + 1, area.width, area.height), text, highlight);
            GUI.Label(area, text, Title);
        }
        public static bool Choice(Rect rect, string text, bool selected, bool glyphs = false)
        {
            Init();
            return ControllerGlyphs.Button(rect, text, Button, selected, glyphs);
        }
    }

    internal sealed class ActionMenu
    {
        public bool Open { get; private set; }
        private int page, selected;
        private bool holding;
        private readonly PanelNavigation navigation = new PanelNavigation();
        private string message = "";
        public void Toggle() { Open = !Open; message = ""; }
        public void Close() { Open = false; holding = false; }
        public void Show(GameAction action)
        {
            int targetPage = ActionCatalog.MenuPage(action);
            page = targetPage >= 0 ? targetPage : 0;
            selected = targetPage >= 0 ? Array.IndexOf(ActionCatalog.MenuGroups[page], action) : 0;
            message = ""; Open = true;
        }
        public GameAction? HeldAction(PadState pad)
        {
            if (ActionCatalog.IsDefecateEntry(page, selected)) return null;
            var action = ActionCatalog.MenuGroups[page][selected];
            return Open && pad.Held(Buttons.A) && ActionCatalog.CanHold(action) ? action : (GameAction?)null;
        }
        public void Tick(Plugin plugin, PadState pad, PadState previous)
        {
            if (!Open) return;
            holding = HeldAction(pad).HasValue && plugin.InGameplay;
            bool Down(Buttons b) => pad.Held(b) && !previous.Held(b);
            if (Down(Buttons.Start)) { plugin.ShowControllerSettings(); return; }
            if (Down(Buttons.B)) { Close(); plugin.MenuActivated(); return; }
            // Lock the selection throughout a hold, including its release frame.
            if (pad.Held(Buttons.A) || previous.Held(Buttons.A))
            {
                if (Down(Buttons.A) && (ActionCatalog.IsDefecateEntry(page, selected) || !ActionCatalog.CanHold(ActionCatalog.MenuGroups[page][selected]))) Activate(plugin, false);
                return;
            }
            if (Down(Buttons.LB)) { page = (page + ActionCatalog.MenuGroups.Length - 1) % ActionCatalog.MenuGroups.Length; selected = 0; }
            if (Down(Buttons.RB)) { page = (page + 1) % ActionCatalog.MenuGroups.Length; selected = 0; }
            int length = ActionCatalog.MenuEntryCount(page);
            selected = (selected + navigation.Move(pad, Time.unscaledTime, 2) + length) % length;
            if (Down(Buttons.Y)) { plugin.Actions.Clear(); message = "All held actions released."; }
            if (Down(Buttons.A)) Activate(plugin, false);
            else if (Down(Buttons.X)) Activate(plugin, true);
        }
        private void Activate(Plugin plugin, bool hold)
        {
            if (!plugin.InGameplay) { message = "Resume gameplay to perform actions."; return; }
            if (ActionCatalog.IsDefecateEntry(page, selected))
            {
                if (hold) { message = "Use [A] for this action."; return; }
                if (!plugin.Defecate(out message)) return;
                Close(); plugin.MenuActivated(); return;
            }
            var action = ActionCatalog.MenuGroups[page][selected];
            if (hold)
            {
                if (!ActionCatalog.CanHold(action)) { message = "Use [A] for this action."; return; }
                plugin.Actions.ToggleHold(action);
            }
            else plugin.Actions.Queue(action);
            Close();
            plugin.MenuActivated();
        }
        public void Draw(Plugin plugin)
        {
            if (!Open || holding) return;
            var area = PanelStyle.Begin(920, 565);
            PanelStyle.Heading(new Rect(area.x + 24, area.y + 14, 720, 40), "Uncommon actions / " + ActionCatalog.MenuPages[page]);
            if (PanelStyle.Choice(new Rect(area.x + 764, area.y + 16, 128, 34), "Back [B]", false, true)) { Close(); plugin.MenuActivated(); }
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 60, 875, 45), "[LB] / [RB] change page     [D-pad / LS] select     [A] perform / hold     [X] toggle hold", PanelStyle.Text);
            var group = ActionCatalog.MenuGroups[page];
            for (int i = 0; i < ActionCatalog.MenuEntryCount(page); i++)
            {
                bool defecate = ActionCatalog.IsDefecateEntry(page, i);
                var action = defecate ? default : group[i];
                string label = defecate ? "Defecate\n[A] perform (requires 30% bowel meter)" : (plugin.Actions.Held(action) ? "HOLDING  " : "") + ActionCatalog.Label(action) + "\n" + ActionCatalog.Hint(action, plugin.Driving);
                if (PanelStyle.Choice(new Rect(area.x + 24 + i % 2 * 440, area.y + 115 + i / 2 * 78, 430, 70), label, selected == i, true))
                { selected = i; Activate(plugin, false); }
            }
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 438, 875, 65), message.Length > 0 ? message : "Hold [A] for continuous actions; release to stop. [X] toggles a hold after closing. [Y] releases toggled holds.", PanelStyle.Text);
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 515, 875, 35), "[B / View] close   [Y] release holds   [Start] settings                 " + (page + 1) + " / " + ActionCatalog.MenuGroups.Length, PanelStyle.Text);
            GUI.matrix = Matrix4x4.identity;
        }
    }

    internal sealed class ControllerKeyboard
    {
        private TMP_InputField tmp;
        private InputField legacy;
        private computerscript computer;
        private string original, buffer = "", pending = "";
        private int selected;
        private bool upper, symbols, ready;
        private readonly PanelNavigation navigation = new PanelNavigation();
        public bool Open => tmp != null || legacy != null || computer != null;
        internal void Begin(TMP_InputField field) { tmp = field; original = buffer = field.text; selected = 0; ready = false; }
        private string Keys => symbols ? "1234567890!@#$%^&*()-_=+[]{};:'\",.<>/?\\|`~" : "1234567890qwertyuiopasdfghjkl;zxcvbnm,./ ";
        public void Detect(Plugin plugin)
        {
            if (Open || !plugin.ControllerConnected) return;
            var selectedObject = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            if (selectedObject != null)
            {
                var t = selectedObject.GetComponent<TMP_InputField>();
                var l = selectedObject.GetComponent<InputField>();
                if (t != null && t.isFocused && !t.readOnly) Begin(t);
                else if (l != null && l.isFocused && !l.readOnly) { legacy = l; original = buffer = l.text; ready = false; }
            }
            if (!Open && inputscript.i != null && inputscript.i.usingComputer && mainscript.s != null && mainscript.s.player != null)
            {
                computer = mainscript.s.player.lookedAtComputer;
                buffer = "";
                ready = false;
            }
            if (Open) { selected = 0; plugin.MenuActivated(); }
        }
        public void Tick(Plugin plugin, PadState pad, PadState previous)
        {
            if (!Open) return;
            if (!pad.Connected) return;
            if (!ready) { if (pad.Buttons == 0 && pad.LT == 0 && pad.RT == 0) ready = true; return; }
            bool Down(Buttons b) => pad.Held(b) && !previous.Held(b);
            int move = navigation.Move(pad, Time.unscaledTime, 10);
            int count = Keys.Length + 4;
            selected = (selected + move + count) % count;
            if (Down(Buttons.B)) { Close(false); plugin.MenuActivated(); return; }
            if (Down(Buttons.Start)) { Close(true); plugin.MenuActivated(); return; }
            if (Down(Buttons.Y)) upper = !upper;
            if (Down(Buttons.LB)) { symbols = !symbols; selected = 0; }
            if (Down(Buttons.RB)) Enter(" ");
            if (Down(Buttons.X)) Delete();
            if (Down(Buttons.A)) Choose(plugin);
        }
        private void Choose(Plugin plugin)
        {
            if (selected < Keys.Length) Enter(upper ? char.ToUpperInvariant(Keys[selected]).ToString() : Keys[selected].ToString());
            else switch (selected - Keys.Length)
            {
                case 0: Enter(" "); break;
                case 1: Delete(); break;
                case 2: if (computer != null) Enter("\r"); else { buffer = ""; Sync(); } break;
                case 3: Close(true); plugin.MenuActivated(); break;
            }
        }
        private void Enter(string text)
        {
            if (computer != null) { pending += text; buffer = text == "\r" ? "" : buffer + text; }
            else { buffer += text; Sync(); }
        }
        private void Delete()
        {
            if (computer != null) pending += "\b";
            if (buffer.Length > 0) buffer = buffer.Substring(0, buffer.Length - 1);
            Sync();
        }
        private void Sync()
        {
            if (tmp != null) { tmp.text = buffer; tmp.caretPosition = tmp.text.Length; buffer = tmp.text; }
            if (legacy != null) { legacy.text = buffer; legacy.caretPosition = legacy.text.Length; buffer = legacy.text; }
        }
        public string TakeComputerText() { string result = pending; pending = ""; return result; }
        public void Close(bool commit)
        {
            if (!commit && computer == null) { buffer = original ?? ""; Sync(); }
            if (tmp != null) { tmp.DeactivateInputField(); EventSystem.current?.SetSelectedGameObject(null); }
            if (legacy != null) { legacy.DeactivateInputField(); EventSystem.current?.SetSelectedGameObject(null); }
            if (computer != null)
            {
                computer.StopAllNote();
                if (inputscript.i != null) inputscript.i.usingComputer = false;
            }
            tmp = null; legacy = null; computer = null;
            pending = "";
        }
        public void Draw(Plugin plugin)
        {
            if (!Open) return;
            var area = PanelStyle.Begin(1000, 530);
            PanelStyle.Heading(new Rect(area.x + 24, area.y + 14, 800, 40), computer != null ? "Computer keyboard" : "Enter text");
            if (PanelStyle.Choice(new Rect(area.x + 844, area.y + 16, 128, 34), "Back [B]", false, true)) { Close(false); plugin.MenuActivated(); }
            GUI.Label(new Rect(area.x + 24, area.y + 60, 950, 55), buffer.Length > 120 ? buffer.Substring(buffer.Length - 120) : buffer + "|", PanelStyle.Text);
            int count = Keys.Length + 4;
            for (int i = 0; i < count; i++)
            {
                string label = i < Keys.Length ? (upper ? char.ToUpperInvariant(Keys[i]) : Keys[i]).ToString() : new[] { "Space", "Delete", computer != null ? "Enter" : "Clear", "Done" }[i - Keys.Length];
                if (label == " ") label = "Space";
                if (PanelStyle.Choice(new Rect(area.x + 24 + i % 10 * 95, area.y + 125 + i / 10 * 60, 88, 52), label, selected == i))
                { selected = i; Choose(plugin); }
            }
            ControllerGlyphs.Label(new Rect(area.x + 24, area.y + 448, 950, 65), "[A] type   [X] delete   [Y] case   [LB] symbols   [RB] space\n[Start] done   [B] cancel   [D-pad / LS] select", PanelStyle.Text);
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
