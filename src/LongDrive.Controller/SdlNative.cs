using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LongDrive.Controller
{
    // A private filename prevents accidentally binding another mod's SDL version.
    internal static class SdlNative
    {
        internal const string Dll = "TLDControllerSDL2.dll";
        internal const uint Subsystems = 0x00000200 | 0x00002000; // JOYSTICK | GAMECONTROLLER, no window/audio subsystem
        private static bool attempted;
        internal static bool Ready { get; private set; }
        internal static string Failure { get; private set; }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryW(string path);
        internal static bool Initialize()
        {
            if (attempted) return Ready;
            attempted = true;
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Dll);
                if (!File.Exists(path)) throw new FileNotFoundException("Controller runtime missing", path);
                if (LoadLibraryW(path) == IntPtr.Zero) throw new InvalidOperationException("Windows loader error " + Marshal.GetLastWin32Error());
                SDL_SetMainReady();
                SDL_SetHintWithPriority("SDL_XINPUT_ENABLED", "1", 2);
                // Read stable physical positions; SdlInputMath applies Nintendo letter mapping once.
                SDL_SetHintWithPriority("SDL_GAMECONTROLLER_USE_BUTTON_LABELS", "0", 2);
                SDL_SetHintWithPriority("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1", 1);
                SDL_SetHintWithPriority("SDL_JOYSTICK_HIDAPI", "1", 1);
                SDL_SetHintWithPriority("SDL_JOYSTICK_HIDAPI_SWITCH", "1", 1);
                // DirectInput and HIDAPI are enough alongside XInput; avoid WGI duplicate virtual devices.
                SDL_SetHintWithPriority("SDL_JOYSTICK_WGI", "0", 1);
                if (SDL_InitSubSystem(Subsystems) != 0) throw new InvalidOperationException(Error);
                Ready = true;
                string mappings = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gamecontrollerdb.txt");
                if (File.Exists(mappings))
                    foreach (string line in File.ReadAllLines(mappings))
                        if (line.Length > 0 && !line.StartsWith("#")) SDL_GameControllerAddMapping(line);
            }
            catch (Exception ex) { Failure = ex.Message; }
            return Ready;
        }
        internal static string Utf8(IntPtr value) => value == IntPtr.Zero ? "" : Marshal.PtrToStringUTF8(value);
        internal static string Error => Utf8(SDL_GetError());
        internal static void Dispose()
        {
            if (Ready) SDL_QuitSubSystem(Subsystems);
            Ready = false;
            // Mono caches P/Invoke entry points. Keep the module mapped for the process lifetime.
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct GuidData { public ulong First, Last; public byte Driver => (byte)(Last >> 48); }
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_SetMainReady();
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_SetHintWithPriority([MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string value, int priority);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GetHint([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_InitSubSystem(uint flags);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_QuitSubSystem(uint flags);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GetError();
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_JoystickUpdate();
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_FlushEvents(uint first, uint last);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_NumJoysticks();
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_JoystickGetDeviceInstanceID(int index);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern GuidData SDL_JoystickGetDeviceGUID(int index);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_IsGameController(int index);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_GameControllerAddMapping([MarshalAs(UnmanagedType.LPUTF8Str)] string mapping);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GameControllerOpen(int index);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GameControllerGetJoystick(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_GameControllerGetAttached(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_GameControllerName(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_GameControllerGetType(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ushort SDL_GameControllerGetVendor(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ushort SDL_GameControllerGetProduct(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern short SDL_GameControllerGetAxis(IntPtr controller, int axis);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern byte SDL_GameControllerGetButton(IntPtr controller, int button);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_GameControllerClose(IntPtr controller);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_JoystickOpen(int index);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void SDL_JoystickClose(IntPtr joystick);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_JoystickGetAttached(IntPtr joystick);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr SDL_JoystickName(IntPtr joystick);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ushort SDL_JoystickGetVendor(IntPtr joystick);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ushort SDL_JoystickGetProduct(IntPtr joystick);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int SDL_JoystickNumAxes(IntPtr joystick);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern short SDL_JoystickGetAxis(IntPtr joystick, int axis);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern byte SDL_JoystickGetButton(IntPtr joystick, int button);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern byte SDL_JoystickGetHat(IntPtr joystick, int hat);
    }
}
