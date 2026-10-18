using System;
using LongDrive.Controller;

internal static class SteeringChecks
{
    internal static void Run(Action<bool, string> check)
    {
        foreach (int fps in new[] { 30, 60, 144 })
        {
            var ramp = new SteeringResponse();
            float dt = 1f / fps;
            float first = ramp.Step(1, 1, dt, true);
            check(first > 0 && first < .04f, "Full stick starts gradually at " + fps + " FPS");
            for (int i = 1; i < fps / 2; i++) ramp.Step(1, 1, dt, true);
            check(Math.Abs(ramp.Value - .5f) < .00001f, "Half a second reaches half steering at " + fps + " FPS");
            for (int i = fps / 2; i < fps; i++) ramp.Step(1, 1, dt, true);
            check(Math.Abs(ramp.Value - 1) < .00001f, "Full stick still reaches full lock at " + fps + " FPS");
            for (int i = 0; i < fps; i++) ramp.Step(-1, 1, dt, true);
            check(Math.Abs(ramp.Value) < .00001f, "Reversing passes gradually through center at " + fps + " FPS");
            for (int i = 0; i < fps + 1; i++) ramp.Step(-1, 1, dt, true);
            check(ramp.Value == -1, "Opposite full lock remains reachable at " + fps + " FPS");
            for (int i = 0; i < fps + 1; i++) ramp.Step(0, 1, dt, true);
            check(ramp.Value == 0, "Released stick returns fully to center at " + fps + " FPS");
        }
        var slow = new SteeringResponse(); var fast = new SteeringResponse();
        for (int i = 0; i < 15; i++) { slow.Step(1, 1, 1f/60, true); fast.Step(1, .25f, 1f/60, true); }
        check(Math.Abs(slow.Value - .25f) < .00001f && Math.Abs(fast.Value - 1) < .00001f, "Time-to-lock setting changes real ramp rate");
        var partial = new SteeringResponse();
        for (int i = 0; i < 180; i++) partial.Step(.13f, .75f, 1f/60, true);
        check(partial.Value == .13f, "Partial stick settles on its own target without overshooting");
        float before = partial.Value;
        partial.Step(1, .75f, 0, true);
        check(partial.Value == before, "Paused time does not advance steering");
        partial.Step(1, .75f, 5, true);
        check(partial.Value - before < .067f, "Long frame cannot snap steering to full lock");
        partial.Step(1, .75f, 1f/60, false);
        check(partial.Value == 0, "Menu, disconnect, focus loss or disabled input clears steering ramp");
        check(partial.Step(1, .75f, 1f/60, true) < .03f, "Rearmed input starts a new gradual ramp");
        partial.Reset(); check(partial.Value == 0, "Immediate transition reset clears steering ramp");
        check(!SteeringResponse.AllowNativeAxis(true, true, "Joy1"), "Native wheel pad axis cannot bypass mod steering rate");
        check(SteeringResponse.AllowNativeAxis(true, false, "Joy1"), "Native joystick steering returns when mod controller disconnects");
        check(SteeringResponse.AllowNativeAxis(true, true, "Mouse X"), "Native mouse steering is not disabled by duplicate-pad guard");
        check(!SteeringResponse.AllowNativeAxis(false, true, "Joy1"), "Disabled native steering stays disabled");
    }
}
