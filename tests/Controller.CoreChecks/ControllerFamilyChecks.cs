using System;
using System.Linq;
using LongDrive.Controller;

internal static class ControllerFamilyChecks
{
    internal static void Run(Action<bool, string> check)
    {
        foreach (ControllerTransport transport in Enum.GetValues(typeof(ControllerTransport)))
        {
            check(ControllerIdentity.Detect(0x054c, "Wireless Controller", 0, transport) == ControllerFamily.PlayStation, "Sony USB identity over " + transport);
            check(ControllerIdentity.Detect(0x057e, "Pro Controller", 1, transport) == ControllerFamily.Nintendo, "Nintendo vendor overrides Xbox-like layout over " + transport);
            check(ControllerIdentity.Detect(0x045e, "Controller", 0, transport) == ControllerFamily.Xbox, "Microsoft vendor over " + transport);
            check(ControllerIdentity.Detect(0, null, 0, transport) == (transport == ControllerTransport.XInput ? ControllerFamily.Xbox : ControllerFamily.PlayStation), "unidentified transport fallback " + transport);
        }
        foreach (string name in new[] { "DualSense Wireless Controller", "DUALSHOCK 4", "PlayStation Controller", "PS5 Controller" })
            check(ControllerIdentity.Detect(0, name, 0, ControllerTransport.DirectInput) == ControllerFamily.PlayStation, "PlayStation name " + name);
        foreach (string name in new[] { "Nintendo Pro Controller", "8BitDo Switch Controller", "Joy-Con (L)", "JoyCon Pair" })
            check(ControllerIdentity.Detect(0, name, 0, ControllerTransport.XInput) == ControllerFamily.Nintendo, "Nintendo identity overrides transport " + name);
        check(ControllerIdentity.Detect(0, "Xbox Wireless Controller", 0, ControllerTransport.DirectInput) == ControllerFamily.Xbox, "Xbox name identified outside XInput");
        check(ControllerIdentity.Detect(0x2dc8, "SDL 8BitDo Ultimate 2 Wireless Controller", 0, ControllerTransport.DirectInput, 0x310b) == ControllerFamily.Xbox, "Proton-exposed Ultimate 2 Wireless retains Xbox legends");
        check(ControllerIdentity.Detect(0x2dc8, "SDL 8BitDo Ultimate 2 Wireless Controller", 0, ControllerTransport.DirectInput, 0x6012) == ControllerFamily.PlayStation, "unidentified DirectInput mode with shared 8BitDo name uses PlayStation fallback");
        check(ControllerIdentity.Detect(0x2dc8, "SDL 8BitDo Ultimate 2 Wireless Controller", 0, ControllerTransport.XInput, 0x6012) == ControllerFamily.Xbox, "unidentified XInput mode with shared 8BitDo name uses Xbox fallback");
        check(ControllerIdentity.Detect(0x2dc8, "Unknown 8BitDo Controller", 0, ControllerTransport.DirectInput) == ControllerFamily.PlayStation, "third-party vendor alone never implies Xbox or Nintendo");
        foreach (int type in new[] { 3, 4, 7 }) check(ControllerIdentity.Detect(0, "Wireless Controller", type, ControllerTransport.DirectInput) == ControllerFamily.PlayStation, "SDL PlayStation type " + type);
        foreach (int type in new[] { 5, 11, 12, 13 }) check(ControllerIdentity.Detect(0, "Pro Controller", type, ControllerTransport.XInput) == ControllerFamily.Nintendo, "SDL Nintendo type " + type);
        check(ControllerIdentity.ButtonName("A", ControllerFamily.Nintendo) == "A" && ControllerIdentity.ButtonName("B", ControllerFamily.Nintendo) == "B", "Nintendo A/B text follows printed labels");
        check(ControllerIdentity.ButtonName("X", ControllerFamily.Nintendo) == "X" && ControllerIdentity.ButtonName("Y", ControllerFamily.Nintendo) == "Y", "Nintendo X/Y text follows printed labels");
        check(ControllerIdentity.ButtonName("A", ControllerFamily.PlayStation) == "Cross" && ControllerIdentity.ButtonName("LB", ControllerFamily.PlayStation) == "L1", "PlayStation text fallback uses matching buttons");
        check(ControllerIdentity.ButtonName("Start", ControllerFamily.Nintendo) == "Plus" && ControllerIdentity.ButtonName("Select", ControllerFamily.Nintendo) == "Minus", "Nintendo menu aliases");
        SdlInputMath.Stick(0, short.MinValue, .18f, out float x, out float y);
        check(x == 0 && y == 1, "SDL full up avoids signed-short overflow");
        SdlInputMath.Stick(0, short.MaxValue, .18f, out x, out y);
        check(x == 0 && y == -1, "SDL full down sign matches XInput convention");
        SdlInputMath.Stick(1000, -1000, .18f, out x, out y);
        check(x == 0 && y == 0, "SDL radial deadzone suppresses diagonal drift");
        SdlInputMath.Stick(short.MaxValue, short.MinValue, .18f, out x, out y);
        check(Math.Abs(x*x+y*y-1) < .0001f && y > 0, "SDL diagonal sticks retain unit magnitude");
        check(SdlInputMath.Trigger(short.MaxValue, .08f) == 1 && SdlInputMath.Trigger(0, .08f) == 0 && SdlInputMath.Trigger(-1, .08f) == 0, "SDL independent trigger endpoints");
        check(SdlInputMath.RawTrigger(short.MinValue, .08f) == 0 && SdlInputMath.RawTrigger(short.MaxValue, .08f) == 1, "raw joystick trigger endpoints");
        check(SdlInputMath.ButtonMap[0] == Buttons.A && SdlInputMath.ButtonMap[1] == Buttons.B && SdlInputMath.ButtonMap[5] == 0, "canonical SDL face and guide normalization");
        check(!SdlInputMath.NewActivity(new PadState(), new PadState()), "idle second controller does not steal ownership");
        check(SdlInputMath.NewActivity(new PadState { Buttons=Buttons.A }, new PadState()), "new controller button can select active device");
        check(!SdlInputMath.NewActivity(new PadState(), new PadState { LX = 1 }), "releasing another controller stick cannot steal ownership");
        foreach (string label in new[] { "A", "B", "X", "Y", "LB", "RB", "LT", "RT", "L3", "R3", "View", "Start", "LS", "RS", "D-pad", "D-pad Up", "D-pad Down", "D-pad Left", "D-pad Right", "LS Up", "RS Right", "LS Click", "RS click" })
            check(GlyphTokens.Parse(label).Length == 1 && GlyphTokens.Parse(label)[0].Sprite != null, "single family-neutral token " + label);
        check(GlyphTokens.Parse("Brake / Unbound / Camera").All(run => run.Sprite == null), "ordinary label substrings never become buttons");
        check(GlyphTokens.Parse("[LB + A] / [RS Click]").Count(run => run.Sprite != null) == 3, "compound bindings preserve all canonical glyphs");
    }
}
