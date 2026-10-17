using System;

namespace LongDrive.Controller
{
    internal static class SdlInputMath
    {
        internal static readonly Buttons[] ButtonMap = { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.Back, 0, Buttons.Start, Buttons.LS, Buttons.RS, Buttons.LB, Buttons.RB, Buttons.Up, Buttons.Down, Buttons.Left, Buttons.Right };
        internal static Buttons FamilyButtons(Buttons physical, ControllerFamily family)
        {
            if (family != ControllerFamily.Nintendo) return physical;
            // SDL is held in positional mode. Translate Nintendo's printed face
            // labels once, before gameplay, UI navigation, and binding capture.
            var result = physical & ~(Buttons.A | Buttons.B | Buttons.X | Buttons.Y);
            if ((physical & Buttons.A) != 0) result |= Buttons.B;
            if ((physical & Buttons.B) != 0) result |= Buttons.A;
            if ((physical & Buttons.X) != 0) result |= Buttons.Y;
            if ((physical & Buttons.Y) != 0) result |= Buttons.X;
            return result;
        }
        internal static void Stick(short rawX, short rawY, float deadzone, out float x, out float y)
        {
            x = PadMath.Axis(rawX); y = -PadMath.Axis(rawY);
            float length = (float)Math.Sqrt(x * x + y * y);
            if (length <= deadzone) { x = y = 0; return; }
            float scale = (Math.Min(length, 1f) - deadzone) / (1 - deadzone) / length;
            x *= scale; y *= scale;
        }
        internal static float Trigger(short raw, float threshold) => Math.Max(0, (Math.Max(0, (int)raw) / 32767f - threshold) / (1 - threshold));
        internal static float RawTrigger(short raw, float threshold) => Math.Max(0, ((raw + 32768f) / 65535f - threshold) / (1 - threshold));
        internal static bool NewActivity(PadState current, PadState previous) =>
            (current.Buttons & ~previous.Buttons) != 0 ||
            (Math.Abs(current.LX) > .3f && Math.Abs(current.LX - previous.LX) > .3f) || (Math.Abs(current.LY) > .3f && Math.Abs(current.LY - previous.LY) > .3f) ||
            (Math.Abs(current.RX) > .3f && Math.Abs(current.RX - previous.RX) > .3f) || (Math.Abs(current.RY) > .3f && Math.Abs(current.RY - previous.RY) > .3f) ||
            current.LT - previous.LT > .3f || current.RT - previous.RT > .3f;
    }
}
