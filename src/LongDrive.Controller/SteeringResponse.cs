using System;

namespace LongDrive.Controller
{
    internal sealed class SteeringResponse
    {
        internal const float DefaultSeconds = .75f;
        internal float Value { get; private set; }
        internal void Reset() { Value = 0; }
        internal float Step(float target, float secondsToLock, float elapsed, bool enabled)
        {
            if (!enabled) { Reset(); return 0; }
            target = Math.Max(-1, Math.Min(1, target));
            float step = Math.Max(0, Math.Min(.05f, elapsed)) / Math.Max(.1f, Math.Min(3, secondsToLock));
            Value += Math.Max(-step, Math.Min(step, target - Value));
            return Value;
        }
        internal static bool IsJoystickAxis(string name) => !string.IsNullOrEmpty(name) && name.StartsWith("Joy", StringComparison.OrdinalIgnoreCase);
        internal static bool AllowNativeAxis(bool enabled, bool controllerConnected, string name) =>
            enabled && !(controllerConnected && IsJoystickAxis(name));
    }
}
