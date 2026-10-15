using System;
using System.Collections.Generic;
using System.Text;

namespace LongDrive.Controller
{
    internal enum PadControl
    {
        None, LB, RB, LSClick, RSClick, LT, RT, A, B, X, Y,
        DPadUp, DPadRight, DPadDown, DPadLeft,
        LSUp, LSDown, LSLeft, LSRight, RSUp, RSDown, RSLeft, RSRight
    }

    internal sealed class ControlChord
    {
        public static readonly ControlChord Empty = new ControlChord(Array.Empty<PadControl>());
        public readonly PadControl[] Controls;
        public ControlChord(params PadControl[] controls)
        {
            Controls = (PadControl[])controls.Clone();
            Array.Sort(Controls);
            if (Controls.Length > 3) throw new ArgumentException("Use at most three controls.");
            int axes = 0;
            for (int i = 0; i < Controls.Length; i++)
            {
                if (Controls[i] == PadControl.None || !Enum.IsDefined(typeof(PadControl), Controls[i]) || i > 0 && Controls[i] == Controls[i - 1])
                    throw new ArgumentException("Invalid or repeated control.");
                if (Controls[i] >= PadControl.LSUp) axes++;
            }
            if (axes > 1) throw new ArgumentException("Use one stick direction per binding.");
        }
        public bool IsEmpty => Controls.Length == 0;
        public string Code => string.Join("+", Array.ConvertAll(Controls, c => c.ToString()));
        public string Label => IsEmpty ? "Unbound" : string.Join(" + ", Array.ConvertAll(Controls, LabelOf));
        public bool Contains(PadControl control) => Array.IndexOf(Controls, control) >= 0;
        public bool Includes(ControlChord other)
        {
            foreach (var control in other.Controls) if (!Contains(control)) return false;
            return true;
        }
        public float Value(PadState pad)
        {
            if (IsEmpty) return 0;
            float value = 1;
            foreach (var control in Controls) value = Math.Min(value, Read(control, pad));
            return value;
        }
        public static bool TryParse(string code, out ControlChord chord)
        {
            chord = Empty;
            if (code == "") return true;
            if (code == null) return false;
            var parts = code.Split('+');
            var controls = new PadControl[parts.Length];
            for (int i = 0; i < parts.Length; i++) if (!Enum.TryParse(parts[i], false, out controls[i])) return false;
            try { chord = new ControlChord(controls); return true; }
            catch (ArgumentException) { return false; }
        }
        public static string LabelOf(PadControl control)
        {
            switch (control)
            {
                case PadControl.LSClick: return "LS Click";
                case PadControl.RSClick: return "RS Click";
                case PadControl.DPadUp: return "D-pad Up";
                case PadControl.DPadDown: return "D-pad Down";
                case PadControl.DPadLeft: return "D-pad Left";
                case PadControl.DPadRight: return "D-pad Right";
                case PadControl.LSUp: return "LS Up";
                case PadControl.LSDown: return "LS Down";
                case PadControl.LSLeft: return "LS Left";
                case PadControl.LSRight: return "LS Right";
                case PadControl.RSUp: return "RS Up";
                case PadControl.RSDown: return "RS Down";
                case PadControl.RSLeft: return "RS Left";
                case PadControl.RSRight: return "RS Right";
                default: return control.ToString();
            }
        }
        public static float Read(PadControl control, PadState pad)
        {
            switch (control)
            {
                case PadControl.LB: return pad.Held(Buttons.LB) ? 1 : 0;
                case PadControl.RB: return pad.Held(Buttons.RB) ? 1 : 0;
                case PadControl.LSClick: return pad.Held(Buttons.LS) ? 1 : 0;
                case PadControl.RSClick: return pad.Held(Buttons.RS) ? 1 : 0;
                case PadControl.A: return pad.Held(Buttons.A) ? 1 : 0;
                case PadControl.B: return pad.Held(Buttons.B) ? 1 : 0;
                case PadControl.X: return pad.Held(Buttons.X) ? 1 : 0;
                case PadControl.Y: return pad.Held(Buttons.Y) ? 1 : 0;
                case PadControl.DPadUp: return pad.Held(Buttons.Up) ? 1 : 0;
                case PadControl.DPadDown: return pad.Held(Buttons.Down) ? 1 : 0;
                case PadControl.DPadLeft: return pad.Held(Buttons.Left) ? 1 : 0;
                case PadControl.DPadRight: return pad.Held(Buttons.Right) ? 1 : 0;
                case PadControl.LT: return pad.LT;
                case PadControl.RT: return pad.RT;
                case PadControl.LSUp: return Math.Max(0, pad.LY);
                case PadControl.LSDown: return Math.Max(0, -pad.LY);
                case PadControl.LSLeft: return Math.Max(0, -pad.LX);
                case PadControl.LSRight: return Math.Max(0, pad.LX);
                case PadControl.RSUp: return Math.Max(0, pad.RY);
                case PadControl.RSDown: return Math.Max(0, -pad.RY);
                case PadControl.RSLeft: return Math.Max(0, -pad.RX);
                case PadControl.RSRight: return Math.Max(0, pad.RX);
                default: return 0;
            }
        }
    }

    internal sealed class ControllerBindings
    {
        private readonly ControlChord[][] profiles = { Defaults(false), Defaults(true) };
        public int Revision { get; private set; }
        public ControlChord Get(GameAction action, bool driving) => profiles[driving ? 1 : 0][(int)action];
        public GameAction? Conflict(GameAction action, bool driving, ControlChord chord)
        {
            if (chord.IsEmpty) return null;
            for (int i = 0; i < ActionCatalog.Count; i++)
                if (i != (int)action && profiles[driving ? 1 : 0][i].Code == chord.Code) return (GameAction)i;
            return null;
        }
        public void Assign(GameAction action, bool driving, ControlChord chord)
        {
            var conflict = Conflict(action, driving, chord);
            var profile = profiles[driving ? 1 : 0];
            if (conflict.HasValue) profile[(int)conflict.Value] = profile[(int)action];
            profile[(int)action] = chord;
            Revision++;
        }
        public void Reset(bool driving) { profiles[driving ? 1 : 0] = Defaults(driving); Revision++; }
        public string Hint(GameAction action, bool driving)
        {
            var chord = Get(action, driving);
            if (!chord.IsEmpty) return chord.Label + (!driving && (action == GameAction.run || action == GameAction.duck) ? " (toggle)" : "");
            int page = ActionCatalog.MenuPage(action);
            return page >= 0 ? "View > " + ActionCatalog.MenuPages[page] : "Unbound";
        }
        public void Evaluate(PadState pad, bool driving, float[] result)
        {
            var profile = profiles[driving ? 1 : 0];
            for (int i = 0; i < result.Length; i++) result[i] = profile[i].Value(pad);
            // More specific active chords win over their constituent bindings.
            // Keep the raw values intact while deciding suppression so ordering cannot
            // change the result for nested chords.
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] <= 0) continue;
                for (int j = 0; j < profile.Length; j++)
                    if (profile[j].Controls.Length > profile[i].Controls.Length && profile[j].Value(pad) > .2f && profile[j].Includes(profile[i]))
                    { result[i] = 0; break; }
            }
        }
        public bool RunModifierHeld(PadState pad)
        {
            foreach (var chord in profiles[0])
                for (int i = 0; i < chord.Controls.Length - 1; i++)
                    if (!Get(GameAction.run, false).Contains(chord.Controls[i]) && ControlChord.Read(chord.Controls[i], pad) > .2f) return true;
            return false;
        }
        public string Serialize(bool driving)
        {
            var result = new StringBuilder("v1");
            for (int i = 0; i < ActionCatalog.Count; i++) result.Append(';').Append((GameAction)i).Append('=').Append(Get((GameAction)i, driving).Code);
            return result.ToString();
        }
        public bool TryLoad(string text, bool driving)
        {
            if (string.IsNullOrEmpty(text)) return true;
            string[] parts = text.Split(';');
            if (parts[0] != "v1") return false;
            var next = Defaults(driving);
            var seen = new HashSet<GameAction>();
            for (int i = 1; i < parts.Length; i++)
            {
                int split = parts[i].IndexOf('=');
                if (split < 1 || !Enum.TryParse(parts[i].Substring(0, split), false, out GameAction action) || (int)action < 0 || (int)action >= ActionCatalog.Count || !seen.Add(action) ||
                    !ControlChord.TryParse(parts[i].Substring(split + 1), out var chord)) return false;
                next[(int)action] = chord;
            }
            var occupied = new HashSet<string>();
            foreach (var chord in next) if (!chord.IsEmpty && !occupied.Add(chord.Code)) return false;
            profiles[driving ? 1 : 0] = next;
            Revision++;
            return true;
        }
        private static ControlChord[] Defaults(bool driving)
        {
            var map = new ControlChord[68];
            for (int i = 0; i < map.Length; i++) map[i] = ControlChord.Empty;
            void Bind(GameAction action, params PadControl[] controls) => map[(int)action] = new ControlChord(controls);
            Bind(GameAction.lookup, PadControl.RSUp); Bind(GameAction.lookdown, PadControl.RSDown);
            Bind(GameAction.lookleft, PadControl.RSLeft); Bind(GameAction.lookright, PadControl.RSRight);
            Bind(GameAction.use, PadControl.X); Bind(GameAction.pickup, PadControl.Y); Bind(GameAction.equiprecenter, PadControl.RSClick);
            if (driving)
            {
                Bind(GameAction.steerleft, PadControl.LSLeft); Bind(GameAction.steerright, PadControl.LSRight);
                Bind(GameAction.gas, PadControl.RT); Bind(GameAction.brake, PadControl.LT);
                Bind(GameAction.clutch, PadControl.LSClick); Bind(GameAction.handbrake, PadControl.A);
                Bind(GameAction.ignition, PadControl.B); Bind(GameAction.shiftup, PadControl.RB); Bind(GameAction.shiftdown, PadControl.LB);
                Bind(GameAction.lights, PadControl.DPadUp); Bind(GameAction.horn, PadControl.DPadDown);
                Bind(GameAction.scrollup, PadControl.DPadRight); Bind(GameAction.scrolldown, PadControl.DPadLeft);
            }
            else
            {
                Bind(GameAction.forward, PadControl.LSUp); Bind(GameAction.back, PadControl.LSDown);
                Bind(GameAction.left, PadControl.LSLeft); Bind(GameAction.right, PadControl.LSRight);
                Bind(GameAction.run, PadControl.LSClick); Bind(GameAction.duck, PadControl.B); Bind(GameAction.jump, PadControl.A);
                Bind(GameAction.lmb, PadControl.RT); Bind(GameAction.rmb, PadControl.LT);
                Bind(GameAction.mountdismount, PadControl.RB); Bind(GameAction.mmb, PadControl.LB, PadControl.RB);
                Bind(GameAction.reload, PadControl.LB, PadControl.X); Bind(GameAction.push, PadControl.LB, PadControl.A);
                Bind(GameAction.looklean, PadControl.LB, PadControl.B); Bind(GameAction.glue, PadControl.LB, PadControl.Y);
                Bind(GameAction.target, PadControl.LB, PadControl.RSClick);
                Bind(GameAction.scrollup, PadControl.LB, PadControl.DPadUp); Bind(GameAction.scrolldown, PadControl.LB, PadControl.DPadDown);
                Bind(GameAction.inventory1, PadControl.DPadUp); Bind(GameAction.inventory2, PadControl.DPadRight);
                Bind(GameAction.inventory3, PadControl.DPadDown); Bind(GameAction.inventory4, PadControl.DPadLeft);
            }
            return map;
        }
    }

    internal sealed class BindingCapture
    {
        private readonly List<PadControl> controls = new List<PadControl>();
        private bool armed;
        public bool Active { get; private set; }
        public string Error { get; private set; }
        public void Begin() { controls.Clear(); armed = false; Active = true; Error = "Release all controls, then press a button or combination."; }
        public void Cancel() { Active = false; controls.Clear(); }
        public bool Tick(PadState pad, out ControlChord result)
        {
            result = null;
            if (!Active || !pad.Connected) return false;
            if (!armed) { if (pad.Neutral) { armed = true; Error = "Press controls, then release to save. Start / View cancels."; } return false; }
            foreach (PadControl control in Enum.GetValues(typeof(PadControl)))
                if (control != PadControl.None && ControlChord.Read(control, pad) > .6f && !controls.Contains(control)) controls.Add(control);
            if (controls.Count == 0 || !pad.Neutral) return false;
            try { result = new ControlChord(controls.ToArray()); Active = false; return true; }
            catch (ArgumentException ex) { controls.Clear(); Error = ex.Message + " Try again, or Start / View to cancel."; return false; }
        }
    }
}
