namespace LongDrive.Controller
{
    internal static class BindingHints
    {
        public static string Pair(ControllerBindings bindings, GameAction negative, GameAction positive, bool driving)
        {
            var a = bindings.Get(negative, driving);
            var b = bindings.Get(positive, driving);
            if (a.Controls.Length == 1 && b.Controls.Length == 1)
            {
                var first = a.Controls[0]; var second = b.Controls[0];
                if (first == PadControl.LSLeft && second == PadControl.LSRight || first == PadControl.LSDown && second == PadControl.LSUp) return "LS";
                if (first == PadControl.RSLeft && second == PadControl.RSRight || first == PadControl.RSDown && second == PadControl.RSUp) return "RS";
            }
            return a.Label + " / " + b.Label;
        }
        public static string Look(ControllerBindings bindings, bool driving)
        {
            string horizontal = Pair(bindings, GameAction.lookleft, GameAction.lookright, driving);
            string vertical = Pair(bindings, GameAction.lookdown, GameAction.lookup, driving);
            return horizontal == vertical ? horizontal : horizontal + " / " + vertical;
        }
        public static string[] DrivingRows(ControllerBindings bindings, bool pointerSteer = false, bool pointerPedals = false)
        {
            string Hint(GameAction action) => bindings.Hint(action, true);
            string steering = pointerSteer ? Pair(bindings, GameAction.lookleft, GameAction.lookright, true) : Pair(bindings, GameAction.steerleft, GameAction.steerright, true);
            string pedals = pointerSteer && pointerPedals ? Pair(bindings, GameAction.lookdown, GameAction.lookup, true) + ": Pedals" : Hint(GameAction.gas) + ": Throttle     " + Hint(GameAction.brake) + ": Brake";
            return new[]
            {
                steering + ": Steer     " + pedals + (pointerSteer ? "" : "     " + Look(bindings, true) + ": Look"),
                Hint(GameAction.ignition) + ": Ignition (hold)     " + Hint(GameAction.handbrake) + ": Handbrake     " + Hint(GameAction.clutch) + ": Clutch",
                Hint(GameAction.shiftdown) + " / " + Hint(GameAction.shiftup) + ": Shift down / up     View > Gears: Select gear",
                Hint(GameAction.lights) + ": Lights     " + Hint(GameAction.horn) + ": Horn     " + Hint(GameAction.use) + ": Use / exit"
            };
        }
    }
}
