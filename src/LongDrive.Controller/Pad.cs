using System;
using System.Runtime.InteropServices;

namespace LongDrive.Controller
{
    [Flags]
    internal enum Buttons : ushort
    {
        Up = 1, Down = 2, Left = 4, Right = 8, Start = 16, Back = 32,
        LS = 64, RS = 128, LB = 256, RB = 512,
        A = 4096, B = 8192, X = 16384, Y = 32768
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RawPad
    {
        public Buttons Buttons;
        public byte LT, RT;
        public short LX, LY, RX, RY;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RawState
    {
        public uint Packet;
        public RawPad Pad;
    }

    internal struct PadState
    {
        public bool Connected;
        public Buttons Buttons;
        public float LX, LY, RX, RY, LT, RT;
        public bool Held(Buttons b) => (Buttons & b) != 0;
        public bool Neutral => Buttons == 0 && LX == 0 && LY == 0 && RX == 0 && RY == 0 && LT == 0 && RT == 0;
    }

    internal static class PadMath
    {
        public static float Axis(short value) => value < 0 ? value / 32768f : value / 32767f;
        public static void Stick(short rawX, short rawY, float deadzone, out float x, out float y)
        {
            x = Axis(rawX); y = Axis(rawY);
            float length = (float)Math.Sqrt(x * x + y * y);
            if (length <= deadzone) { x = y = 0; return; }
            float scale = (Math.Min(length, 1f) - deadzone) / (1f - deadzone) / length;
            x *= scale; y *= scale;
        }
        public static float Trigger(byte value, float threshold) => Math.Max(0, (value / 255f - threshold) / (1 - threshold));
        // A response curve keeps full steering lock at either endpoint while allowing
        // gentler small corrections. Deadzone is applied before this curve.
        public static float SteeringInput(float value, float sensitivity)
        {
            float magnitude = (float)Math.Pow(Math.Min(1, Math.Abs(value)), 1f / Math.Max(.25f, Math.Min(2f, sensitivity)));
            return value < 0 ? -magnitude : magnitude;
        }
        public static float RotationInput(float degreesPerSecond, float nativeSensitivity) => degreesPerSecond / (10f * Math.Max(.1f, Math.Abs(nativeSensitivity)));
    }

    internal sealed class XInputPad
    {
        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState", CallingConvention = CallingConvention.StdCall)]
        private static extern uint Read14(uint index, out RawState state);
        [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState", CallingConvention = CallingConvention.StdCall)]
        private static extern uint Read13(uint index, out RawState state);
        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState", CallingConvention = CallingConvention.StdCall)]
        private static extern uint Read910(uint index, out RawState state);
        private delegate uint ReadFn(uint index, out RawState state);
        private ReadFn read;
        private bool initialized;
        public int Index { get; private set; } = -1;
        public string Backend { get; private set; } = "not initialized";

        public PadState Poll(float deadzone, float triggerThreshold)
        {
            if (!initialized)
            {
                initialized = true;
                ReadFn[] candidates = { Read14, Read13, Read910 };
                string[] names = { "XInput 1.4", "XInput 1.3", "XInput 9.1.0" };
                for (int i = 0; i < candidates.Length; i++)
                {
                    try { candidates[i](0, out _); read = candidates[i]; Backend = names[i]; break; }
                    catch (DllNotFoundException) { }
                    catch (EntryPointNotFoundException) { }
                }
                if (read == null) Backend = "XInput DLL unavailable";
            }
            if (read == null) return default;
            RawState raw;
            if (Index < 0 || read((uint)Index, out raw) != 0)
            {
                // First report loss so input edges cannot cross between two controllers.
                if (Index >= 0) { Index = -1; return default; }
                for (uint i = 0; i < 4; i++)
                    if (read(i, out raw) == 0) { Index = (int)i; break; }
                if (Index < 0) return default;
            }
            if (read((uint)Index, out raw) != 0) { Index = -1; return default; }
            var result = new PadState { Connected = true, Buttons = raw.Pad.Buttons };
            PadMath.Stick(raw.Pad.LX, raw.Pad.LY, deadzone, out result.LX, out result.LY);
            PadMath.Stick(raw.Pad.RX, raw.Pad.RY, deadzone, out result.RX, out result.RY);
            result.LT = PadMath.Trigger(raw.Pad.LT, triggerThreshold);
            result.RT = PadMath.Trigger(raw.Pad.RT, triggerThreshold);
            return result;
        }
    }

    internal sealed class InputGate
    {
        private string context;
        private bool armed;
        public bool Armed => armed;
        public PadState Filter(PadState raw, string nextContext, bool focused)
        {
            if (context != nextContext || !raw.Connected || !focused)
            {
                context = nextContext;
                armed = false;
            }
            if (!armed && raw.Connected && focused && raw.Neutral) armed = true;
            return armed && focused && raw.Connected ? raw : default;
        }
    }
}
