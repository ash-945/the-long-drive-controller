using System;

namespace LongDrive.Controller
{
    internal enum ControllerFamily { Xbox, PlayStation, Nintendo }
    internal enum ControllerTransport { XInput, DirectInput, Hid, Virtual }

    internal static class ControllerIdentity
    {
        internal static ControllerFamily Detect(ushort vendor, string name, int sdlType, ControllerTransport transport)
        {
            // Physical identity takes precedence over a driver's normalized button layout.
            if (vendor == 0x054c) return ControllerFamily.PlayStation;
            if (vendor == 0x057e) return ControllerFamily.Nintendo;
            if (vendor == 0x045e) return ControllerFamily.Xbox;
            string text = (name ?? "").ToLowerInvariant();
            if (text.Contains("playstation") || text.Contains("dualshock") || text.Contains("dualsense") || text.Contains("ps3") || text.Contains("ps4") || text.Contains("ps5")) return ControllerFamily.PlayStation;
            if (text.Contains("nintendo") || text.Contains("switch") || text.Contains("joy-con") || text.Contains("joycon")) return ControllerFamily.Nintendo;
            if (text.Contains("xbox") || text.Contains("x-box")) return ControllerFamily.Xbox;
            switch (sdlType)
            {
                case 1: case 2: return ControllerFamily.Xbox;
                case 3: case 4: case 7: return ControllerFamily.PlayStation;
                case 5: case 11: case 12: case 13: return ControllerFamily.Nintendo;
            }
            // This model uses Xbox face legends even when Proton re-exposes its
            // XInput device as a named DirectInput joystick. Other 8BitDo models
            // deliberately remain subject to their own identity/type or fallback.
            if (text.Contains("8bitdo") && text.Contains("ultimate 2 wireless")) return ControllerFamily.Xbox;
            return transport == ControllerTransport.XInput ? ControllerFamily.Xbox : ControllerFamily.PlayStation;
        }

        internal static string ButtonName(string canonical, ControllerFamily family)
        {
            if (family == ControllerFamily.PlayStation)
                switch (canonical)
                {
                    case "A": return "Cross"; case "B": return "Circle"; case "X": return "Square"; case "Y": return "Triangle";
                    case "LB": return "L1"; case "RB": return "R1"; case "LT": return "L2"; case "RT": return "R2";
                    case "Select": return "Select / Share"; case "Start": return "Start / Options";
                }
            if (family == ControllerFamily.Nintendo)
                switch (canonical)
                {
                    case "LB": return "L"; case "RB": return "R"; case "LT": return "ZL"; case "RT": return "ZR";
                    case "Select": return "Minus"; case "Start": return "Plus";
                }
            return canonical == "Select" ? "View" : canonical == "Start" ? "Menu" : canonical.Replace('_', ' ');
        }
    }
}
