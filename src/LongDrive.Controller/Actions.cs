using System;

namespace LongDrive.Controller
{
    // Ordinals are verified against the installed game's metadata by the core checks.
    internal enum GameAction
    {
        forward, back, left, right, lookup, lookdown, lookleft, lookright,
        lmb, rmb, mmb, run, jump, duck, use, pickup, reload, mountdismount,
        push, equiprecenter, looklean, piss, burpfart, target, map, glue,
        sleep, pointrubwave, hum, smoke, removeSunglasses, inventory1,
        inventory2, inventory3, inventory4, scrollup, scrolldown, camera,
        zoom, hidehud, smalldropcam, screenshot3D, screenshot360, camera360,
        vrrecenter, multiplayerchat, voicechat, keylook, togglemousedrive,
        gas, brake, steerleft, steerright, clutch, handbrake, shiftup,
        shiftdown, gearr, gearn, gear1, gear2, gear3, gear4, gear5, gear6,
        ignition, horn, lights
    }

    internal static class ActionCatalog
    {
        public const int Count = 68;
        public static readonly string[] Pages = { "Movement", "Hands", "Tools", "Inventory", "Survival", "Driving", "Gears", "Camera & vehicle", "System" };
        public static readonly GameAction[][] Groups =
        {
            new[] { GameAction.forward, GameAction.back, GameAction.left, GameAction.right, GameAction.lookup, GameAction.lookdown, GameAction.lookleft, GameAction.lookright },
            new[] { GameAction.lmb, GameAction.rmb, GameAction.mmb, GameAction.use, GameAction.pickup, GameAction.mountdismount, GameAction.push, GameAction.equiprecenter },
            new[] { GameAction.reload, GameAction.looklean, GameAction.target, GameAction.glue, GameAction.scrollup, GameAction.scrolldown, GameAction.zoom, GameAction.keylook },
            new[] { GameAction.run, GameAction.jump, GameAction.duck, GameAction.inventory1, GameAction.inventory2, GameAction.inventory3, GameAction.inventory4, GameAction.map },
            new[] { GameAction.piss, GameAction.burpfart, GameAction.sleep, GameAction.pointrubwave, GameAction.hum, GameAction.smoke, GameAction.removeSunglasses, GameAction.camera },
            new[] { GameAction.gas, GameAction.brake, GameAction.steerleft, GameAction.steerright, GameAction.clutch, GameAction.handbrake, GameAction.shiftup, GameAction.shiftdown },
            new[] { GameAction.gearr, GameAction.gearn, GameAction.gear1, GameAction.gear2, GameAction.gear3, GameAction.gear4, GameAction.gear5, GameAction.gear6 },
            new[] { GameAction.ignition, GameAction.horn, GameAction.lights, GameAction.hidehud, GameAction.smalldropcam, GameAction.screenshot3D, GameAction.screenshot360, GameAction.camera360 },
            new[] { GameAction.vrrecenter, GameAction.multiplayerchat, GameAction.voicechat, GameAction.togglemousedrive }
        };
        public static readonly string[] MenuPages = { "Survival", "Tools", "Gears", "Camera", "System" };
        public static readonly GameAction[][] MenuGroups =
        {
            new[] { GameAction.piss, GameAction.burpfart, GameAction.sleep, GameAction.pointrubwave, GameAction.hum, GameAction.smoke, GameAction.removeSunglasses },
            new[] { GameAction.map, GameAction.zoom, GameAction.keylook },
            new[] { GameAction.gearr, GameAction.gearn, GameAction.gear1, GameAction.gear2, GameAction.gear3, GameAction.gear4, GameAction.gear5, GameAction.gear6 },
            new[] { GameAction.camera, GameAction.hidehud, GameAction.smalldropcam, GameAction.screenshot3D, GameAction.screenshot360, GameAction.camera360 },
            new[] { GameAction.vrrecenter, GameAction.multiplayerchat, GameAction.voicechat, GameAction.togglemousedrive }
        };
        // Defecation is a native player operation, not a separate bindable input ordinal.
        public static bool IsDefecateEntry(int page, int selected) => page == 0 && selected == MenuGroups[page].Length;
        public static int MenuEntryCount(int page) => MenuGroups[page].Length + (page == 0 ? 1 : 0);
        public static int MenuPage(GameAction action)
        {
            for (int i = 0; i < MenuGroups.Length; i++) if (Array.IndexOf(MenuGroups[i], action) >= 0) return i;
            return -1;
        }
        private static readonly string[] Labels =
        {
            "Move forward", "Move backward", "Move left", "Move right", "Look up", "Look down", "Look left", "Look right",
            "Primary action", "Secondary action / move object", "Rotate object", "Sprint", "Jump", "Crouch", "Use / enter / exit", "Pick up / drop",
            "Reload / adjust", "Mount / detach", "Push", "Equip / recenter", "Lean", "Urinate", "Burp / fart", "Aim / target", "Map", "Attach with glue",
            "Sleep", "Point / rub / wave", "Hum", "Smoke", "Remove sunglasses", "Inventory 1", "Inventory 2", "Inventory 3", "Inventory 4",
            "Scroll up / increase distance", "Scroll down / decrease distance", "Change camera", "Zoom", "Hide HUD", "Drop camera", "3D screenshot", "360 screenshot", "360 camera",
            "VR recenter", "Multiplayer chat", "Voice chat", "Keyboard look", "Mouse driving", "Throttle", "Brake", "Steer left", "Steer right", "Clutch", "Handbrake", "Shift up", "Shift down",
            "Reverse gear", "Neutral gear", "Gear 1", "Gear 2", "Gear 3", "Gear 4", "Gear 5", "Gear 6", "Ignition", "Horn", "Lights"
        };
        public static string Label(GameAction action) => Labels[(int)action];
        public static int Page(GameAction action)
        {
            for (int i = 0; i < Groups.Length; i++) if (Array.IndexOf(Groups[i], action) >= 0) return i;
            throw new ArgumentOutOfRangeException(nameof(action));
        }
        public static string Hint(GameAction action, bool driving)
        {
            if (driving && (int)action < 4) return "View > Movement";
            switch (action)
            {
                case GameAction.forward: return "LS Up";
                case GameAction.back: return "LS Down";
                case GameAction.left: return "LS Left";
                case GameAction.right: return "LS Right";
                case GameAction.lookup: return "RS Up";
                case GameAction.lookdown: return "RS Down";
                case GameAction.lookleft: return "RS Left";
                case GameAction.lookright: return "RS Right";
                case GameAction.use: return "X";
                case GameAction.pickup: return "Y";
                case GameAction.equiprecenter: return "RS Click";
            }
            if (!driving)
            {
                switch (action)
                {
                    case GameAction.run: return "LS Click (toggle)";
                    case GameAction.jump: return "A";
                    case GameAction.duck: return "B (toggle)";
                    case GameAction.lmb: return "RT";
                    case GameAction.rmb: return "LT";
                    case GameAction.mmb: return "LB + RB";
                    case GameAction.mountdismount: return "RB";
                    case GameAction.reload: return "LB + X";
                    case GameAction.push: return "LB + A";
                    case GameAction.looklean: return "LB + B";
                    case GameAction.glue: return "LB + Y";
                    case GameAction.target: return "LB + RS Click";
                    case GameAction.scrollup: return "LB + D-pad Up";
                    case GameAction.scrolldown: return "LB + D-pad Down";
                    case GameAction.inventory1: return "D-pad Up";
                    case GameAction.inventory2: return "D-pad Right";
                    case GameAction.inventory3: return "D-pad Down";
                    case GameAction.inventory4: return "D-pad Left";
                }
            }
            else
            {
                switch (action)
                {
                    case GameAction.gas: return "RT";
                    case GameAction.brake: return "LT";
                    case GameAction.steerleft: return "LS Left";
                    case GameAction.steerright: return "LS Right";
                    case GameAction.clutch: return "LS Click";
                    case GameAction.handbrake: return "A";
                    case GameAction.ignition: return "B";
                    case GameAction.shiftup: return "RB";
                    case GameAction.shiftdown: return "LB";
                    case GameAction.lights: return "D-pad Up";
                    case GameAction.horn: return "D-pad Down";
                    case GameAction.scrollup: return "D-pad Right";
                    case GameAction.scrolldown: return "D-pad Left";
                }
            }
            int menuPage = MenuPage(action);
            return menuPage >= 0 ? "View > " + MenuPages[menuPage] : "On-foot / driving controls";
        }
        public static bool CanHold(GameAction a)
        {
            switch (a)
            {
                case GameAction.forward: case GameAction.back: case GameAction.left: case GameAction.right:
                case GameAction.lookup: case GameAction.lookdown: case GameAction.lookleft: case GameAction.lookright:
                case GameAction.lmb: case GameAction.rmb: case GameAction.mmb: case GameAction.run: case GameAction.duck:
                case GameAction.use: case GameAction.pickup: case GameAction.reload: case GameAction.push: case GameAction.looklean:
                case GameAction.piss: case GameAction.burpfart: case GameAction.target: case GameAction.glue:
                case GameAction.sleep: case GameAction.pointrubwave:
                case GameAction.hum: case GameAction.smoke: case GameAction.zoom: case GameAction.voicechat: case GameAction.keylook:
                case GameAction.gas: case GameAction.brake: case GameAction.steerleft: case GameAction.steerright:
                case GameAction.clutch: case GameAction.ignition: case GameAction.horn:
                    return true;
                default: return false;
            }
        }
    }

    internal sealed class ActionState
    {
        private readonly float[] values = new float[ActionCatalog.Count], before = new float[ActionCatalog.Count];
        private readonly bool[] held = new bool[ActionCatalog.Count];
        private GameAction? pulse;
        private bool pulseStarted;
        private float pulseEnd;
        private bool pendingSprint;
        private bool pendingCrouch;
        private Buttons previousButtons;
        private string lastContext;
        public bool Sprint { get; private set; }
        public bool Crouched { get; private set; }
        public float Value(GameAction action) => values[(int)action];
        public bool Pressed(GameAction action) => values[(int)action] > .2f && before[(int)action] <= .2f;
        public bool Released(GameAction action) => values[(int)action] <= .2f && before[(int)action] > .2f;
        public bool Held(GameAction action) => action == GameAction.run ? Sprint || pendingSprint : action == GameAction.duck ? Crouched || pendingCrouch : held[(int)action];
        public void Queue(GameAction action)
        {
            if (action == GameAction.run) { pendingSprint = !pendingSprint; return; }
            if (action == GameAction.duck) { pendingCrouch = !pendingCrouch; return; }
            pulse = action; pulseStarted = false;
        }
        public void ToggleHold(GameAction action)
        {
            if (action == GameAction.run)
            {
                if (Sprint || pendingSprint) { Sprint = false; pendingSprint = false; }
                else pendingSprint = true;
            }
            else if (action == GameAction.duck) pendingCrouch = !pendingCrouch;
            else if (ActionCatalog.CanHold(action)) held[(int)action] = !held[(int)action];
        }
        public void Clear()
        {
            Array.Clear(held, 0, held.Length);
            pulse = null;
            Sprint = false;
            pendingSprint = false;
            Crouched = pendingCrouch = false;
        }
        private void Set(GameAction a, float value) { values[(int)a] = Math.Max(values[(int)a], value); }
        private void Set(GameAction a, bool value) { if (value) Set(a, 1); }

        public void Tick(PadState pad, string context, bool enabled, float now, GameAction? menuHold = null)
        {
            Array.Copy(values, before, values.Length);
            Array.Clear(values, 0, values.Length);
            if (lastContext != context) Clear();
            lastContext = context;
            if (!enabled)
            {
                // Only the selected action may bypass overlay suppression. Navigation,
                // triggers, and the confirm button must never become gameplay input.
                if ((context == "walking" || context == "driving") && menuHold.HasValue && ActionCatalog.CanHold(menuHold.Value))
                    Set(menuHold.Value, true);
                Sprint = false;
                if (context == "walking" && Crouched) Set(GameAction.duck, true);
                previousButtons = pad.Buttons;
                return;
            }
            bool driving = context == "driving";
            bool modifier = !driving && pad.Held(Buttons.LB);
            Buttons beforeButtons = previousButtons;
            bool click = pad.Held(Buttons.LS) && (previousButtons & Buttons.LS) == 0;
            previousButtons = pad.Buttons;
            if (!driving && !modifier && click) { Sprint = !Sprint; pendingSprint = false; if (Sprint) Crouched = false; }
            if (pendingSprint && !driving && (pad.LX != 0 || pad.LY != 0)) { Sprint = !Sprint; pendingSprint = false; }
            if (pad.LX == 0 && pad.LY == 0) Sprint = false;
            // Crouch uses an edge, not the held face button. Modifier+B remains lean.
            bool crouchClick = !driving && !modifier && pad.Held(Buttons.B) && (beforeButtons & Buttons.B) == 0;
            if (crouchClick || pendingCrouch && !driving)
            { Crouched = !Crouched; pendingCrouch = false; Sprint = false; }
            if (driving || Crouched) Sprint = false;
            Set(GameAction.lookup, Math.Max(0, pad.RY)); Set(GameAction.lookdown, Math.Max(0, -pad.RY));
            Set(GameAction.lookleft, Math.Max(0, -pad.RX)); Set(GameAction.lookright, Math.Max(0, pad.RX));
            Set(GameAction.use, !modifier && pad.Held(Buttons.X));
            Set(GameAction.pickup, !modifier && pad.Held(Buttons.Y));
            Set(GameAction.equiprecenter, !modifier && pad.Held(Buttons.RS));
            if (driving)
            {
                Set(GameAction.steerleft, Math.Max(0, -pad.LX)); Set(GameAction.steerright, Math.Max(0, pad.LX));
                Set(GameAction.gas, pad.RT); Set(GameAction.brake, pad.LT);
                Set(GameAction.clutch, pad.Held(Buttons.LS)); Set(GameAction.handbrake, pad.Held(Buttons.A));
                Set(GameAction.ignition, pad.Held(Buttons.B)); Set(GameAction.shiftup, pad.Held(Buttons.RB));
                Set(GameAction.shiftdown, pad.Held(Buttons.LB)); Set(GameAction.lights, pad.Held(Buttons.Up));
                Set(GameAction.horn, pad.Held(Buttons.Down)); Set(GameAction.scrollup, pad.Held(Buttons.Right));
                Set(GameAction.scrolldown, pad.Held(Buttons.Left));
            }
            else
            {
                Set(GameAction.forward, Math.Max(0, pad.LY)); Set(GameAction.back, Math.Max(0, -pad.LY));
                Set(GameAction.left, Math.Max(0, -pad.LX)); Set(GameAction.right, Math.Max(0, pad.LX));
                Set(GameAction.lmb, pad.RT); Set(GameAction.rmb, pad.LT);
                Set(GameAction.run, Sprint || modifier && pad.Held(Buttons.LS));
                Set(GameAction.jump, !modifier && pad.Held(Buttons.A)); Set(GameAction.duck, Crouched);
                Set(GameAction.mountdismount, !modifier && pad.Held(Buttons.RB));
                Set(GameAction.mmb, modifier && pad.Held(Buttons.RB)); Set(GameAction.reload, modifier && pad.Held(Buttons.X));
                Set(GameAction.push, modifier && pad.Held(Buttons.A)); Set(GameAction.looklean, modifier && pad.Held(Buttons.B));
                Set(GameAction.glue, modifier && pad.Held(Buttons.Y)); Set(GameAction.target, modifier && pad.Held(Buttons.RS));
                Set(GameAction.scrollup, modifier && pad.Held(Buttons.Up)); Set(GameAction.scrolldown, modifier && pad.Held(Buttons.Down));
                Set(GameAction.inventory1, !modifier && pad.Held(Buttons.Up)); Set(GameAction.inventory2, !modifier && pad.Held(Buttons.Right));
                Set(GameAction.inventory3, !modifier && pad.Held(Buttons.Down)); Set(GameAction.inventory4, !modifier && pad.Held(Buttons.Left));
            }
            for (int i = 0; i < held.Length; i++) if (held[i]) Set((GameAction)i, 1);
            if (pulse.HasValue)
            {
                // Stay below this game's 0.25 s long-press threshold. X explicitly holds.
                if (!pulseStarted) { pulseStarted = true; pulseEnd = now + .12f; }
                if (now < pulseEnd) Set(pulse.Value, 1);
                else pulse = null;
            }
        }
    }
}
