using System;
using LongDrive.Controller;

internal static class BindingChecks
{
    internal static void Run(Action<bool, string> check)
    {
        var neutral = new PadState { Connected = true };
        var profile = new ControllerBindings();
        var a = new ControlChord(PadControl.A);
        var x = new ControlChord(PadControl.X);
        profile.Assign(GameAction.jump, false, x);
        check(profile.Get(GameAction.use, false).Code == "A" && profile.Hint(GameAction.jump, false) == "X", "binding swap changes both on-foot hints");
        check(profile.Hint(GameAction.use, true) == "X", "on-foot swap leaves driving profile unchanged");
        var actions = new ActionState(profile);
        actions.Tick(neutral, "walking", true, 0);
        actions.Tick(new PadState { Connected = true, Buttons = Buttons.X }, "walking", true, 1);
        check(actions.Pressed(GameAction.jump) && actions.Value(GameAction.use) == 0, "remapped X jumps without also using");
        actions.Tick(new PadState { Connected = true, Buttons = Buttons.A }, "walking", true, 2);
        check(actions.Value(GameAction.use) == 1 && actions.Value(GameAction.jump) == 0, "swapped A uses without jumping");
        profile.Assign(GameAction.duck, false, new ControlChord(PadControl.RT));
        actions.Clear(); actions.Tick(neutral, "walking", true, 3);
        actions.Tick(new PadState { Connected = true, RT = 1 }, "walking", true, 4);
        actions.Tick(neutral, "walking", true, 5);
        check(actions.Crouched && actions.Value(GameAction.duck) == 1, "remapped trigger crouch retains toggle after release");
        actions.Tick(new PadState { Connected = true, RT = 1 }, "walking", true, 6);
        check(!actions.Crouched, "remapped trigger crouch toggles off");
        profile.Assign(GameAction.run, false, a);
        actions.Clear(); actions.Tick(neutral, "walking", true, 7);
        actions.Tick(new PadState { Connected = true, LY = 1, Buttons = Buttons.A }, "walking", true, 8);
        actions.Tick(new PadState { Connected = true, LY = 1 }, "walking", true, 9);
        check(actions.Sprint, "remapped sprint persists after new button release");
        actions.Tick(neutral, "walking", true, 10);
        check(!actions.Sprint, "remapped sprint still cancels at movement stop");

        foreach (bool driving in new[] { false, true })
        foreach (GameAction action in Enum.GetValues<GameAction>())
        {
            var bindings = new ControllerBindings();
            var chord = new ControlChord(PadControl.LT, PadControl.RT);
            bindings.Assign(action, driving, chord);
            check(bindings.Get(action, driving).Code == "LT+RT", "all actions support binding edits " + driving + "/" + action);
            var values = new float[ActionCatalog.Count];
            bindings.Evaluate(new PadState { Connected = true, LT = .35f, RT = .8f }, driving, values);
            check(Math.Abs(values[(int)action] - .35f) < .00001f, "remapped combined trigger preserves analog value " + driving + "/" + action);
            check(bindings.Hint(action, driving).StartsWith("LT + RT"), "hint uses actual assigned chord " + driving + "/" + action);
            var reloaded = new ControllerBindings();
            check(reloaded.TryLoad(bindings.Serialize(driving), driving) && reloaded.Serialize(driving) == bindings.Serialize(driving), "all action edits survive profile save/load " + driving + "/" + action);
            check(reloaded.Get(action, driving).Code == chord.Code && reloaded.Hint(action, driving) == bindings.Hint(action, driving), "saved binding and hint agree " + driving + "/" + action);
        }
        var three = new ControllerBindings();
        three.Assign(GameAction.reload, false, new ControlChord(PadControl.LB, PadControl.RB, PadControl.X));
        var triple = new ActionState(three);
        triple.Tick(neutral, "walking", true, 0);
        triple.Tick(new PadState { Connected = true, Buttons = Buttons.LB | Buttons.RB | Buttons.X }, "walking", true, 1);
        check(triple.Value(GameAction.reload) == 1 && triple.Value(GameAction.use) == 0 && triple.Value(GameAction.mountdismount) == 0 && triple.Value(GameAction.mmb) == 0, "specific three-button chord suppresses nested rotation and base inputs");
        triple.Tick(new PadState { Connected = true, Buttons = Buttons.LB | Buttons.RB }, "walking", true, 2);
        check(triple.Value(GameAction.mmb) == 1 && triple.Value(GameAction.reload) == 0, "partial three-button chord permits the matching two-button action");
        var steer = new ControllerBindings();
        steer.Assign(GameAction.steerleft, true, new ControlChord(PadControl.RSLeft));
        steer.Assign(GameAction.steerright, true, new ControlChord(PadControl.RSRight));
        var drivingActions = new ActionState(steer);
        drivingActions.Tick(neutral, "driving", true, 0);
        drivingActions.Tick(new PadState { Connected = true, RX = -.5f, LX = -.5f }, "driving", true, 1, null, .5f);
        check(drivingActions.Value(GameAction.steerleft) == .25f && drivingActions.Value(GameAction.lookleft) == .5f, "steering response follows remapped axis without changing swapped look axis");
        steer.Assign(GameAction.gas, true, new ControlChord(PadControl.Y));
        steer.Assign(GameAction.ignition, true, new ControlChord(PadControl.LB, PadControl.A));
        var rows = BindingHints.DrivingRows(steer);
        check(rows[0].StartsWith("RS: Steer") && rows[0].Contains("Y: Throttle") && !rows[0].Contains("RT: Throttle"), "driving hints update steering and pedal assignments");
        check(rows[1].Contains("LB + A: Ignition (hold)"), "driving ignition hint updates to a combination");
        check(BindingHints.Look(steer, true) != "RS", "native mouse-look hint follows remapped look controls");
        steer.Assign(GameAction.gas, true, ControlChord.Empty);
        check(BindingHints.DrivingRows(steer)[0].Contains("Unbound: Throttle"), "cleared controls never retain stale driving hints");
        check(BindingHints.DrivingRows(steer, true, true)[0].Contains(": Pedals") && !BindingHints.DrivingRows(steer, true, true)[0].Contains("Throttle"), "pointer-driving hints describe its actual look-axis controls");
        string good = steer.Serialize(true);
        var loaded = new ControllerBindings();
        check(loaded.TryLoad(good, true), "valid saved profile accepted");
        foreach (string bad in new[] { "bad", "v2;gas=A", "v1;gas=Start", "v1;gas=View", "v1;gas=Unknown", "v1;gas=None", "v1;gas=A+A", "v1;gas=LSLeft+LSRight", "v1;gas=LB+RB+A+B", "v1;gas=A;gas=B", "v1;gas=A;handbrake=A", "v1;999=A" })
        {
            check(!loaded.TryLoad(bad, true) && loaded.Serialize(true) == good, "invalid profile fails atomically: " + bad);
        }
        loaded.Reset(true);
        check(loaded.Get(GameAction.gas, true).Code == "RT" && loaded.Get(GameAction.run, false).Code == "LSClick", "reset restores only the chosen context");
        check(ControlChord.TryParse("X+LB", out var canonical) && canonical.Code == "LB+X", "chord order is canonical for conflict detection");
        var capture = new BindingCapture();
        capture.Begin();
        check(!capture.Tick(new PadState { Connected = true, Buttons = Buttons.A }, out _), "capture ignores the confirm button until neutral");
        capture.Tick(neutral, out _);
        capture.Tick(new PadState { Connected = true, Buttons = Buttons.B }, out _);
        check(capture.Tick(neutral, out var captured) && captured.Code == "B", "B can be rebound instead of cancelling capture");
        capture.Begin(); capture.Tick(neutral, out _);
        capture.Tick(new PadState { Connected = true, Buttons = Buttons.LB }, out _);
        capture.Tick(new PadState { Connected = true, Buttons = Buttons.LB | Buttons.X }, out _);
        check(capture.Tick(neutral, out captured) && captured.Code == "LB+X", "capture collects a held modifier plus pressed button");
        capture.Begin(); capture.Tick(neutral, out _);
        capture.Tick(new PadState { Connected = true, RX = -.8f }, out _);
        check(capture.Tick(neutral, out captured) && captured.Code == "RSLeft", "capture supports directional stick controls");
        capture.Begin(); capture.Tick(neutral, out _);
        capture.Tick(new PadState { Connected = true, LT = .8f }, out _);
        check(!capture.Tick(default, out _) && capture.Active, "device loss cannot complete a binding");
        capture.Cancel();
        check(!capture.Active && !capture.Tick(neutral, out _), "capture cancellation discards pending inputs");
        capture.Begin(); capture.Tick(neutral, out _);
        capture.Tick(new PadState { Connected = true, Buttons = Buttons.A | Buttons.B | Buttons.X | Buttons.Y }, out _);
        check(!capture.Tick(neutral, out _) && capture.Active, "oversized chord reports an error and allows retry");
        capture.Tick(new PadState { Connected = true, RT = 1 }, out _);
        check(capture.Tick(neutral, out captured) && captured.Code == "RT", "invalid capture recovers without closing settings");
    }
}
