using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using LongDrive.Controller;

internal static class Program
{
    private static int count;
    private static void Check(bool result, string name)
    {
        if (!result) throw new Exception("FAILED: " + name);
        count++;
        Console.WriteLine("PASS " + name);
    }
    private static void Main(string[] args)
    {
        Check(Marshal.SizeOf<RawPad>() == 12 && Marshal.SizeOf<RawState>() == 16, "XInput native struct layout");
        Check(PadMath.Axis(short.MinValue) == -1 && PadMath.Axis(short.MaxValue) == 1, "asymmetric signed stick endpoints");
        PadMath.Stick(2000, 2000, .18f, out var x, out var y);
        Check(x == 0 && y == 0, "radial deadzone suppresses small diagonal drift");
        PadMath.Stick(short.MaxValue, short.MaxValue, .18f, out x, out y);
        Check(Math.Abs(x * x + y * y - 1) < .0001f, "diagonal input stays within unit circle");
        PadMath.Stick(short.MinValue, 0, .18f, out x, out y);
        Check(x == -1 && y == 0, "full negative stick travel");
        Check(PadMath.Trigger(0, .08f) == 0 && PadMath.Trigger(255, .08f) == 1, "trigger endpoints");
        var pad = new PadState { Connected = true, LT = PadMath.Trigger(255, .08f), RT = PadMath.Trigger(255, .08f) };
        Check(pad.LT == 1 && pad.RT == 1, "both triggers remain independently full");
        Check(PadMath.SteeringInput(.5f, 1) == .5f, "default steering preserves linear stick response");
        Check(PadMath.SteeringInput(.5f, .5f) == .25f && PadMath.SteeringInput(-.5f, .5f) == -.25f, "lower steering sensitivity gives symmetric fine corrections");
        Check(PadMath.SteeringInput(.5f, 2) > .5f, "higher steering sensitivity responds sooner");
        foreach (float sensitivity in new[] { .25f, .5f, 1f, 2f })
        {
            Check(PadMath.SteeringInput(0, sensitivity) == 0 && PadMath.SteeringInput(1, sensitivity) == 1 && PadMath.SteeringInput(-1, sensitivity) == -1, "steering keeps neutral and full lock at sensitivity " + sensitivity);
            float last = -1;
            bool monotonic = true;
            for (int step = -100; step <= 100; step++)
            {
                float result = PadMath.SteeringInput(step / 100f, sensitivity);
                monotonic &= result >= last && result >= -1 && result <= 1;
                last = result;
            }
            Check(monotonic, "steering curve is bounded and monotonic at sensitivity " + sensitivity);
        }
        var gate = new InputGate();
        var neutral = new PadState { Connected = true };
        var held = new PadState { Connected = true, Buttons = Buttons.A, RT = 1 };
        Check(!gate.Filter(held, "menu", true).Connected, "held input at startup cannot activate menu");
        Check(gate.Filter(neutral, "menu", true).Connected, "neutral arms controller");
        Check(gate.Filter(held, "menu", true).Held(Buttons.A), "armed input passes");
        Check(!gate.Filter(held, "driving", true).Connected, "menu activation cannot leak into driving");
        gate.Filter(neutral, "driving", true);
        Check(gate.Filter(held, "driving", true).RT == 1, "driving rearms after neutral");
        Check(!gate.Filter(held, "driving", false).Connected, "focus loss clears input");
        Check(!gate.Filter(held, "driving", true).Connected, "focus regain with held trigger stays blocked");
        gate.Filter(neutral, "driving", true);
        Check(!gate.Filter(default, "driving", true).Connected, "disconnect clears input");
        Check(!gate.Filter(held, "driving", true).Connected, "reconnect requires neutral");
        var state = new ActionState();
        var moving = new PadState { Connected = true, LY = 1, Buttons = Buttons.LS };
        state.Tick(moving, "walking", true, 0);
        Check(state.Sprint && state.Pressed(GameAction.run), "sprint starts with one stick click while moving");
        moving.Buttons = 0;
        state.Tick(moving, "walking", true, .1f);
        Check(state.Sprint && state.Value(GameAction.run) == 1 && !state.Pressed(GameAction.run), "sprint stays on after releasing stick click");
        for (int i = 0; i < 5; i++) state.Value(GameAction.run);
        Check(state.Sprint, "multiple input consumers do not retoggle sprint");
        moving.Buttons = Buttons.LS;
        state.Tick(moving, "walking", true, .2f);
        Check(!state.Sprint && state.Released(GameAction.run), "second stick click turns sprint off");
        moving.Buttons = 0; state.Tick(moving, "walking", true, .3f);
        moving.Buttons = Buttons.LS; state.Tick(moving, "walking", true, .4f);
        state.Tick(neutral, "walking", true, .5f);
        Check(!state.Sprint && state.Released(GameAction.run), "stopping movement cancels sprint");
        moving.Buttons = 0; state.Tick(moving, "walking", true, .6f);
        Check(state.Value(GameAction.run) == 0, "moving again does not restart sprint");
        moving.Buttons = Buttons.LS; state.Tick(moving, "walking", true, .7f);
        moving.Buttons = Buttons.B; state.Tick(moving, "walking", true, .8f);
        Check(!state.Sprint, "crouching cancels sprint");
        moving.Buttons = Buttons.LS; state.Tick(moving, "walking", true, .9f);
        state.Tick(neutral, "menu", false, 1);
        Check(!state.Sprint && state.Value(GameAction.run) == 0, "opening native menus cancels sprint");
        var stance = new ActionState();
        stance.Tick(new PadState { Connected = true, Buttons = Buttons.B }, "walking", true, 0);
        Check(stance.Crouched && stance.Pressed(GameAction.duck), "B toggles crouch on while stationary");
        stance.Tick(neutral, "walking", true, .1f);
        Check(stance.Crouched && stance.Value(GameAction.duck) == 1 && !stance.Released(GameAction.duck), "crouch stays on after B release and movement stop");
        stance.Tick(new PadState { Connected = true, Buttons = Buttons.B }, "walking", true, .2f);
        Check(!stance.Crouched && stance.Released(GameAction.duck), "second B toggles crouch off");
        stance.Tick(new PadState { Connected = true, Buttons = Buttons.LB | Buttons.B }, "walking", true, .3f);
        Check(!stance.Crouched && stance.Value(GameAction.looklean) == 1, "LB+B leans without toggling crouch");
        foreach (float hz in new[] { 30f, 60f, 144f }) foreach (float sensitivity in new[] { .5f, 1f, 5f })
        {
            float oneSecond = PadMath.RotationInput(120, sensitivity) * (1 / hz) * 10 * sensitivity * hz;
            Check(Math.Abs(oneSecond - 120) < .001f, "120 deg/s object rotation at " + hz + " Hz / mouse sensitivity " + sensitivity);
        }
        moving.Buttons = Buttons.LB | Buttons.X | Buttons.Up | Buttons.RB;
        state.Tick(moving, "walking", true, 2);
        Check(state.Value(GameAction.reload) == 1 && state.Value(GameAction.use) == 0, "reload modifier suppresses use");
        Check(state.Value(GameAction.mmb) == 1 && state.Value(GameAction.mountdismount) == 0, "rotation modifier suppresses part detach");
        Check(state.Value(GameAction.scrollup) == 1 && state.Value(GameAction.inventory1) == 0, "scroll modifier suppresses inventory selection");
        state.Tick(new PadState { Connected = true, LT = 1, RT = 1, LX = -.5f }, "driving", true, 3);
        Check(state.Value(GameAction.gas) == 1 && state.Value(GameAction.brake) == 1 && state.Value(GameAction.steerleft) == .5f, "simultaneous independent driving inputs");
        var covered = new bool[ActionCatalog.Count];
        foreach (var group in ActionCatalog.Groups) foreach (var action in group)
        {
            Check(!covered[(int)action], "unique action-menu entry " + action);
            covered[(int)action] = true;
        }
        foreach (GameAction action in Enum.GetValues<GameAction>())
        {
            Check(covered[(int)action] && !string.IsNullOrWhiteSpace(ActionCatalog.Label(action)) && !string.IsNullOrWhiteSpace(ActionCatalog.Hint(action, false)) && !string.IsNullOrWhiteSpace(ActionCatalog.Hint(action, true)), "reachable action and controller hint " + action);
            if (action == GameAction.run)
            {
                var sprintMenu = new ActionState();
                sprintMenu.Tick(neutral, "walking", true, 0);
                sprintMenu.ToggleHold(GameAction.run);
                sprintMenu.Tick(neutral, "walking", false, 1);
                sprintMenu.Tick(new PadState { Connected = true, LY = 1 }, "walking", true, 2);
                Check(sprintMenu.Sprint && sprintMenu.Pressed(GameAction.run), "action-menu sprint uses the same toggle");
                sprintMenu.Tick(neutral, "walking", true, 3);
                Check(!sprintMenu.Sprint && sprintMenu.Released(GameAction.run) && !sprintMenu.Held(GameAction.run), "action-menu sprint also cancels on movement stop");
                continue;
            }
            if (action == GameAction.duck)
            {
                var menuCrouch = new ActionState();
                menuCrouch.Tick(neutral, "walking", true, 0);
                menuCrouch.Queue(action);
                menuCrouch.Tick(neutral, "walking", true, 1);
                menuCrouch.Tick(neutral, "walking", true, 3);
                Check(menuCrouch.Crouched && menuCrouch.Value(action) == 1, "action-menu crouch uses persistent toggle");
                menuCrouch.Queue(action);
                menuCrouch.Tick(neutral, "walking", true, 4);
                Check(!menuCrouch.Crouched && menuCrouch.Released(action), "action-menu crouch toggles off");
                continue;
            }
            var queued = new ActionState();
            queued.Tick(neutral, "walking", true, 0);
            queued.Queue(action);
            queued.Tick(neutral, "walking", false, 1);
            Check(queued.Value(action) == 0, "action does not fire while overlay/neutral gate blocks " + action);
            queued.Tick(neutral, "walking", true, 2);
            Check(queued.Value(action) == 1 && queued.Pressed(action), "queued action press " + action);
            queued.Tick(neutral, "walking", true, 2.1f);
            Check(!queued.Pressed(action) && queued.Value(action) == 1, "queued action held without repeated down " + action);
            queued.Tick(neutral, "walking", true, 3);
            Check(queued.Released(action) && queued.Value(action) == 0, "queued action release " + action);
        }
        Check(ActionCatalog.MenuPages[0] == "Survival" && ActionCatalog.MenuPages[1] == "Tools", "survival and tools pages swapped");
        Check(ActionCatalog.MenuEntryCount(0) == 8 && ActionCatalog.IsDefecateEntry(0, 7), "defecate occupies the eighth survival slot");
        Check(!ActionCatalog.IsDefecateEntry(1, 3) && ActionCatalog.MenuEntryCount(1) == 3, "tools contains only its normal entries");
        var menuEntries = new bool[ActionCatalog.Count];
        foreach (var group in ActionCatalog.MenuGroups) foreach (var action in group)
        {
            Check(!menuEntries[(int)action], "unique uncommon menu action " + action);
            menuEntries[(int)action] = true;
            Check(ActionCatalog.Hint(action, false).StartsWith("View > ") && ActionCatalog.Hint(action, true).StartsWith("View > "), "menu excludes directly bound action " + action);
            if (!ActionCatalog.CanHold(action)) continue;
            var continuous = new ActionState();
            continuous.Tick(neutral, "walking", false, 0);
            continuous.Tick(held, "walking", false, 1, action);
            Check(continuous.Pressed(action) && continuous.Value(GameAction.jump) == 0 && continuous.Value(GameAction.lmb) == 0, "menu hold starts without confirm/trigger leakage " + action);
            continuous.Tick(held, "walking", false, 3, action);
            Check(continuous.Value(action) == 1 && !continuous.Pressed(action), "menu hold continues past long-press threshold " + action);
            continuous.Tick(neutral, "walking", false, 4);
            Check(continuous.Released(action), "menu hold ends immediately on release " + action);
            continuous.Tick(neutral, "walking", true, 5);
            Check(continuous.Value(action) == 0 && !continuous.Pressed(action), "menu release never queues a delayed tap " + action);
            continuous.Tick(held, "menu", false, 6, action);
            Check(continuous.Value(action) == 0, "native menu suppresses selected hold " + action);
        }
        foreach (GameAction action in Enum.GetValues<GameAction>())
            Check(menuEntries[(int)action] || !ActionCatalog.Hint(action, false).StartsWith("View > ") || !ActionCatalog.Hint(action, true).StartsWith("View > "), "uncommon action remains reachable " + action);
        var latch = new ActionState();
        latch.Tick(neutral, "walking", true, 0);
        latch.ToggleHold(GameAction.looklean);
        latch.Tick(neutral, "walking", true, 1);
        Check(latch.Value(GameAction.looklean) == 1, "held modifier starts");
        latch.Tick(neutral, "walking", false, 2);
        Check(latch.Value(GameAction.looklean) == 0, "held modifier is suppressed while action menu is open");
        latch.Tick(neutral, "walking", true, 3);
        Check(latch.Value(GameAction.looklean) == 1, "held modifier resumes after action menu closes");
        latch.Tick(neutral, "driving", true, 4);
        Check(!latch.Held(GameAction.looklean) && latch.Value(GameAction.looklean) == 0, "context transition clears held modifiers");
        if (args.Length < 1) throw new Exception("Pass the installed game's Assembly-CSharp.dll to verify the actual action inventory.");
        using var stream = File.OpenRead(args[0]);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        int matched = 0;
        foreach (var typeHandle in metadata.TypeDefinitions)
        {
            var type = metadata.GetTypeDefinition(typeHandle);
            if (metadata.GetString(type.Name) != "IN" || type.GetDeclaringType().IsNil || metadata.GetString(metadata.GetTypeDefinition(type.GetDeclaringType()).Name) != "inputscript") continue;
            foreach (var fieldHandle in type.GetFields())
            {
                var field = metadata.GetFieldDefinition(fieldHandle);
                if (field.GetDefaultValue().IsNil) continue;
                string name = metadata.GetString(field.Name);
                var constant = metadata.GetConstant(field.GetDefaultValue());
                int ordinal = metadata.GetBlobReader(constant.Value).ReadInt32();
                Check(Enum.TryParse<GameAction>(name, out var value) && (int)value == ordinal, "actual game action matches " + name);
                matched++;
            }
        }
        Check(matched == ActionCatalog.Count, "all 68 actions match installed game metadata");
        Console.WriteLine(count + " core checks passed. In-game behavior still needs controller testing.");
        if (args.Length > 1)
        {
            var report = new StringBuilder("# Controller action inventory\n\nGenerated from the implemented catalog after verifying identifiers against the installed game. View contains uncommon actions; normal bindings cover common actions. Hold A for continuous menu actions, release A to stop; X toggles supported holds. Native game rules still apply.\n\n");
            for (int page = 0; page < ActionCatalog.Pages.Length; page++)
            {
                report.Append("## " + ActionCatalog.Pages[page] + "\n\n| Game action | Label | On foot | Driving |\n| --- | --- | --- | --- |\n");
                foreach (var action in ActionCatalog.Groups[page])
                    report.Append("| " + action + " | " + ActionCatalog.Label(action) + " | " + ActionCatalog.Hint(action, false) + " | " + ActionCatalog.Hint(action, true) + " |\n");
                report.Append("\n");
            }
            File.WriteAllText(args[1], report.ToString());
        }
    }
}
