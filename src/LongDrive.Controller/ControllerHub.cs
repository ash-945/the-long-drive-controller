using System;
using System.Collections.Generic;

namespace LongDrive.Controller
{
    internal sealed class SdlPad : IDisposable
    {
        private static readonly int[] RawButtons = { 1, 2, 0, 3, 4, 5, 8, 9, 10, 11 };
        private static readonly Buttons[] RawButtonMap = { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LB, Buttons.RB, Buttons.Back, Buttons.Start, Buttons.LS, Buttons.RS };
        private IntPtr controller, joystick;
        private int axisCount;
        internal int Id { get; private set; }
        internal string Name { get; private set; }
        internal ControllerFamily Family { get; private set; }
        internal ControllerTransport Transport { get; private set; }
        internal ushort Vendor { get; private set; }
        internal ushort Product { get; private set; }
        internal int Type { get; private set; }
        internal bool Generic => controller == IntPtr.Zero;
        internal bool Attached => joystick != IntPtr.Zero && SdlNative.SDL_JoystickGetAttached(joystick) != 0;
        internal static SdlPad Open(int index)
        {
            var pad = new SdlPad { Id = SdlNative.SDL_JoystickGetDeviceInstanceID(index) };
            if (SdlNative.SDL_IsGameController(index) != 0)
            {
                pad.controller = SdlNative.SDL_GameControllerOpen(index);
                if (pad.controller != IntPtr.Zero) pad.joystick = SdlNative.SDL_GameControllerGetJoystick(pad.controller);
            }
            if (pad.joystick == IntPtr.Zero) pad.joystick = SdlNative.SDL_JoystickOpen(index);
            if (pad.joystick == IntPtr.Zero) return null;
            pad.axisCount = SdlNative.SDL_JoystickNumAxes(pad.joystick);
            pad.Name = SdlNative.Utf8(SdlNative.SDL_JoystickName(pad.joystick));
            byte driver = SdlNative.SDL_JoystickGetDeviceGUID(index).Driver;
            pad.Transport = driver == 'x' ? ControllerTransport.XInput : driver == 'h' ? ControllerTransport.Hid : driver == 'v' ? ControllerTransport.Virtual : ControllerTransport.DirectInput;
            pad.Vendor = SdlNative.SDL_JoystickGetVendor(pad.joystick); pad.Product = SdlNative.SDL_JoystickGetProduct(pad.joystick);
            pad.Type = pad.controller == IntPtr.Zero ? 0 : SdlNative.SDL_GameControllerGetType(pad.controller);
            pad.Family = ControllerIdentity.Detect(pad.Vendor, pad.Name, pad.Type, pad.Transport);
            return pad;
        }
        internal PadState Read(float deadzone, float threshold)
        {
            if (!Attached) return default;
            var result = new PadState { Connected = true };
            if (controller != IntPtr.Zero)
            {
                for (int i = 0; i < SdlInputMath.ButtonMap.Length; i++)
                    if (SdlNative.SDL_GameControllerGetButton(controller, i) != 0) result.Buttons |= SdlInputMath.ButtonMap[i];
                SdlInputMath.Stick(SdlNative.SDL_GameControllerGetAxis(controller, 0), SdlNative.SDL_GameControllerGetAxis(controller, 1), deadzone, out result.LX, out result.LY);
                SdlInputMath.Stick(SdlNative.SDL_GameControllerGetAxis(controller, 2), SdlNative.SDL_GameControllerGetAxis(controller, 3), deadzone, out result.RX, out result.RY);
                result.LT = SdlInputMath.Trigger(SdlNative.SDL_GameControllerGetAxis(controller, 4), threshold);
                result.RT = SdlInputMath.Trigger(SdlNative.SDL_GameControllerGetAxis(controller, 5), threshold);
            }
            else
            {
                // Unmapped DirectInput devices have no universal layout. Supply the
                // conventional PS-style fallback; gamecontrollerdb.txt can override it.
                for (int i = 0; i < RawButtons.Length; i++) if (SdlNative.SDL_JoystickGetButton(joystick, RawButtons[i]) != 0) result.Buttons |= RawButtonMap[i];
                byte hat = SdlNative.SDL_JoystickGetHat(joystick, 0);
                if ((hat & 1) != 0) result.Buttons |= Buttons.Up;
                if ((hat & 2) != 0) result.Buttons |= Buttons.Right;
                if ((hat & 4) != 0) result.Buttons |= Buttons.Down;
                if ((hat & 8) != 0) result.Buttons |= Buttons.Left;
                SdlInputMath.Stick(SdlNative.SDL_JoystickGetAxis(joystick, 0), SdlNative.SDL_JoystickGetAxis(joystick, 1), deadzone, out result.LX, out result.LY);
                int rightX = axisCount >= 5 ? 3 : 2, rightY = axisCount >= 5 ? 4 : 3;
                SdlInputMath.Stick(axisCount > rightX ? SdlNative.SDL_JoystickGetAxis(joystick, rightX) : (short)0,
                    axisCount > rightY ? SdlNative.SDL_JoystickGetAxis(joystick, rightY) : (short)0, deadzone, out result.RX, out result.RY);
                if (axisCount >= 6)
                {
                    result.LT = SdlInputMath.RawTrigger(SdlNative.SDL_JoystickGetAxis(joystick, 2), threshold);
                    result.RT = SdlInputMath.RawTrigger(SdlNative.SDL_JoystickGetAxis(joystick, 5), threshold);
                }
                else
                {
                    // Four-axis PS-style pads expose digital L2/R2. An absent
                    // centered axis must never be treated as a half-held trigger.
                    result.LT = SdlNative.SDL_JoystickGetButton(joystick, 6) != 0 ? 1 : 0;
                    result.RT = SdlNative.SDL_JoystickGetButton(joystick, 7) != 0 ? 1 : 0;
                }
            }
            result.Buttons = SdlInputMath.FamilyButtons(result.Buttons, Family);
            return result;
        }
        public void Dispose()
        {
            if (controller != IntPtr.Zero) SdlNative.SDL_GameControllerClose(controller);
            else if (joystick != IntPtr.Zero) SdlNative.SDL_JoystickClose(joystick);
            controller = joystick = IntPtr.Zero;
        }
    }

    internal sealed class ControllerHub : IDisposable
    {
        private readonly Func<SdlPad, bool> filter;
        internal ControllerHub(Func<SdlPad, bool> filter = null) { this.filter = filter; }
        private readonly XInputPad fallback = new XInputPad();
        private readonly Dictionary<int, SdlPad> devices = new Dictionary<int, SdlPad>();
        private readonly Dictionary<int, PadState> states = new Dictionary<int, PadState>();
        private readonly Dictionary<int, PadState> previous = new Dictionary<int, PadState>();
        private int active = -1, next = -1;
        private long refreshAt;
        private bool warned;
        internal ControllerFamily Family { get; private set; }
        internal int Revision { get; private set; }
        internal string Description { get; private set; } = "No controller";
        internal PadState Poll(float deadzone, float threshold, Action<string> log)
        {
            if (!SdlNative.Initialize())
            {
                if (!warned) { warned = true; log("SDL controller runtime unavailable: " + SdlNative.Failure + "; using XInput fallback."); }
                return ReadXInput(deadzone, threshold);
            }
            SdlNative.SDL_JoystickUpdate();
            // SDL updates controller state through event watchers. Clear only its
            // private joystick queue afterwards; never touch Unity's event stream.
            SdlNative.SDL_FlushEvents(0x600, 0x6ff);
            long now = System.Diagnostics.Stopwatch.GetTimestamp() * 1000 / System.Diagnostics.Stopwatch.Frequency;
            if (now >= refreshAt)
            {
                refreshAt = now + 500;
                Refresh(log);
            }
            if (devices.Count == 0 && filter == null)
            {
                if (active >= 0) { active = next = -1; Revision++; return default; }
                return ReadXInput(deadzone, threshold);
            }
            states.Clear();
            foreach (var pair in devices)
            {
                var state = pair.Value.Read(deadzone, threshold);
                states[pair.Key] = state;
                if (state.Connected && previous.TryGetValue(pair.Key, out var before) && SdlInputMath.NewActivity(state, before)) next = pair.Key;
                previous[pair.Key] = state;
            }
            if (active >= 0 && (!states.TryGetValue(active, out var current) || !current.Connected)) next = -1;
            if (next < 0)
                foreach (var pair in states) if (pair.Value.Connected) { next = pair.Key; break; }
            if (next != active)
            {
                active = next; Revision++;
                if (active >= 0)
                {
                    var selected = devices[active]; Family = selected.Family;
                    Description = selected.Name + " (" + Family + ", " + selected.Transport + ", SDL instance " + active + ")";
                    log("Active controller: " + Description);
                }
                else Description = "No controller";
                // Always expose a disconnect between physical devices. Held actions,
                // UI edges, and the neutral gate must not cross the boundary.
                return default;
            }
            return active >= 0 && states.TryGetValue(active, out var value) ? value : default;
        }
        private PadState ReadXInput(float deadzone, float threshold)
        {
            var state = fallback.Poll(deadzone, threshold);
            Family = ControllerFamily.Xbox;
            Description = "XInput slot " + fallback.Index + " via " + fallback.Backend;
            return state;
        }
        private void Refresh(Action<string> log)
        {
            var removed = new List<int>();
            foreach (var pair in devices) if (!pair.Value.Attached) removed.Add(pair.Key);
            foreach (int id in removed) { devices[id].Dispose(); devices.Remove(id); previous.Remove(id); }
            int count = SdlNative.SDL_NumJoysticks();
            for (int i = 0; i < count; i++)
            {
                int id = SdlNative.SDL_JoystickGetDeviceInstanceID(i);
                if (id < 0 || devices.ContainsKey(id)) continue;
                var device = SdlPad.Open(i);
                if (device == null) continue;
                if (filter != null && !filter(device)) { device.Dispose(); continue; }
                devices.Add(id, device);
                previous[id] = device.Read(.18f, .08f);
                log("Controller found: " + device.Name + "; family=" + device.Family + "; transport=" + device.Transport + "; VID/PID=" + device.Vendor.ToString("x4") + "/" + device.Product.ToString("x4") + "; type=" + device.Type + "; mapping=" + (device.Generic ? "generic PS-style; custom mapping may be needed" : "SDL database"));
            }
        }
        public void Dispose()
        {
            foreach (var device in devices.Values) device.Dispose();
            devices.Clear(); previous.Clear(); states.Clear();
        }
    }
}
