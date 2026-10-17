# The Long Drive — Controller Support

**Play The Long Drive with Xbox, PlayStation, and Nintendo controllers, including menus, driving, and object handling.** Automatic button icons, editable bindings, toggle sprint and crouch, and adjustable steering bring the controls together in one mod.

**Version 0.6.6 beta • For game v2024.11.26b**

## New in this update

- **DirectInput and HID support** alongside XInput, with automatic Xbox, PlayStation, and Nintendo controller identification.
- **81 controller sprites**, including refreshed Xbox artwork, used throughout interaction prompts, native Controls rows, driving hints, and controller menus.
- **Saved controller-family override:** choose Auto, Xbox, PlayStation, or Nintendo in Controller settings → General. A manual choice overrides detection until you select Auto again.
- **Optional driving hints:** turn the seated helper box on or off in General settings.
- **More readable controller menus:** centered uncommon-action captions, darker textured buttons, outlined pale text, and blue focus borders.

## Features

- Analog movement, camera, steering, and independent throttle/brake triggers, plus clutch, ignition, shifting, handbrake, lights, and horn.
- Object pickup, movement, rotation, mounting, detachment, and distance adjustment. Rotation has its own speed setting.
- Menu navigation with the D-pad or left stick, right-stick virtual pointer, scrolling, and controller focus outlines.
- Save/Load navigation that prefers Save when available, skips informational fields, and keeps confirmations focused on No. RB focuses Save and LB returns to slots; cancel backs out of confirmations.
- Uncommon-actions menu for survival actions, tools, direct gears, camera functions, and system actions. Hold confirm for continuous actions, or toggle supported holds.
- Toggle sprint and crouch. Sprint cancels when movement stops.
- Separate editable on-foot/driving bindings, including buttons, triggers, stick directions, and combinations of up to three controls. Hints follow saved bindings.
- Saved camera, object movement/rotation, pointer, deadzone, inversion, and steering-response settings.

All 68 named game actions have controller routes. Survival also includes the native Defecate action, subject to the game's 30% bowel-meter requirement. Actions retain the game's normal restrictions. Text entry uses a physical keyboard; there is no on-screen keyboard.

## Requirements

- The Long Drive **v2024.11.26b**. The tested build reports `2024.11.26b_test`.
- An Xbox/XInput, supported PlayStation/Nintendo, or compatible DirectInput controller.
- [BepInEx 5.4.23.5 Windows x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5), installed separately. Use the Windows loader on Windows and Linux/Proton.

The private Windows x64 SDL2 controller runtime is included. No additional native Linux library is needed under Proton.

## Installation and updating

1. Close the game. Install BepInEx beside the **TheLongDrive.exe you actually launch**. Linux/Proton users must configure the override below before launching.
2. Launch once to generate BepInEx configuration, then close the game.
3. **Required for this game:** in `BepInEx/config/BepInEx.cfg`, under `[Chainloader]`, set `HideManagerGameObject = true` and save.
4. Extract the mod archive into the same game folder. Copy the **whole** `BepInEx/plugins/LongDrive.Controller` folder. It contains both `LongDrive.Controller.dll` and `TLDControllerSDL2.dll`, plus SDL license notices. The managed DLL alone is insufficient for DirectInput support.
5. Connect the controller and launch. Open uncommon actions with View/Back/Select/Minus, then press Start/Options/Plus for controller settings.

When updating from an older version, replace the complete plugin folder contents from the archive. Keep your configuration and any custom `gamecontrollerdb.txt`; the archive does not include or overwrite them. Keep only one copy of the mod installed.

### Linux / Proton / Faugus

For Steam, add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the game's launch options, preserving existing wrappers and overrides.

For Faugus or another Proton launcher, point it at the executable in the modded game folder. Add **WINEDLLOVERRIDES** with the value **winhttp=n,b**. Installing into a different game copy will not enable the mod in the copy you launch. Different Proton prefixes may have separate settings and saves.

See the [official BepInEx Proton guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html) for the equivalent winecfg setup.

## Controller families and quick controls

The following defaults use Xbox names. In-game icons follow the selected family:

- Xbox A/B/X/Y correspond to PlayStation Cross/Circle/Square/Triangle.
- Nintendo uses its printed A/B/X/Y letters: A on the right confirms, B below cancels, X is upper, and Y is left.
- Xbox LB/RB/LT/RT correspond to PlayStation L1/R1/L2/R2 and Nintendo L/R/ZL/ZR.
- View/Back corresponds to PlayStation Select/Share and Nintendo Minus; Start corresponds to Start/Options and Plus. The shared PlayStation artwork uses Select/Start icons.

**Default Xbox controls:** LS/RS move/look; LS click toggles sprint; B toggles crouch; A jumps; X uses/enters/exits; Y picks up/drops; RT/LT are primary/secondary actions. Hold LB+RB and move RS to rotate a held object. Driving uses LS steering, RT/LT throttle/brake, A handbrake, held B ignition, LB/RB shifting, and LS click clutch.

The included **CONTROLS.md** covers modifiers, inventory, menus, bindings, and text entry.

In Controller settings, LB/RB switches General/Bindings. On General, select with up/down, adjust with left/right or +/−, confirm increases or toggles, and the upper face button restores defaults. **Controller family** saves a manual icon choice across restarts and reconnections; choose Auto to restore detection. **Driving hint box** hides or shows the seated helper without hiding other interaction prompts. General defaults restores Auto and enables driving hints; it does not reset bindings.

On Bindings, the left face button switches profiles, LT/RT changes categories, confirm begins capture, and RS click clears a binding. Release all controls before entering a new combination, then release again to save. Occupied bindings require swap confirmation. Start/View and menu navigation remain fixed for recovery.

## Detection and compatibility

Known devices use their vendor identity, name, and SDL controller type. Unknown XInput devices use Xbox icons; unknown DirectInput/HID devices use PlayStation icons. If detection is wrong, choose a family manually. This setting changes icons; physical input remains normalized by SDL.

Steam Input and other mappers may expose a physical controller as a virtual Xbox device. Avoid simultaneous controller-to-keyboard mappings that cause duplicate input. Unmapped DirectInput devices use a conventional PlayStation-style fallback; unusual layouts may need an SDL mapping in `gamecontrollerdb.txt` beside the plugin. Device-specific USB/Bluetooth behavior is not universal.

This is a **beta**, tested interactively on Linux/Proton with an 8BitDo controller in Xbox, Nintendo, and generic DirectInput modes. Nintendo letter mapping and the PlayStation fallback prompts were confirmed during testing, as were manual family selection, driving-hint preferences, and centered action captions. Version 0.6.6 passes 1,525 core checks; the preceding 0.6.5 runtime passed 159 native input/UI/config checks and verified all 81 sprite identities. Native Windows hardware and genuine Sony-controller hardware have not been independently tested. Other game versions, every vehicle/menu, multiplayer/VR activity, and third-party mod menus have not been exhaustively tested.

## Text entry, removal, and support

Use a physical keyboard for custom save names and the in-game computer. The game provides default save names. RS moves the computer cursor, and cancel or Start exits it.

To uninstall, remove `BepInEx/plugins/LongDrive.Controller`; optionally remove the mod config. Keep BepInEx if other mods need it.

If the mod does not load, check the executable path, BepInEx installation, `HideManagerGameObject`, and Proton override. Look for **The Long Drive Controller 0.6.6** in `BepInEx/LogOutput.log`. If only XInput works, check that `TLDControllerSDL2.dll` is present and that the log does not report an SDL runtime failure. If icons are wrong, select Controller family manually or return it to Auto.

For support, include game/mod versions, OS/Proton, controller model/mode, Steam Input setup, the failed action, and the log. After reconnecting or changing contexts, release all controls to neutral before resuming gameplay.

## Credits and source

An unofficial mod for The Long Drive. Uses BepInEx and its HarmonyX runtime dependency, installed separately. The bundled SDL2 runtime uses the zlib license; its license and distribution notice are included. Xbox, PlayStation, and Nintendo button graphics are supplied Creative Commons assets requiring no attribution, as confirmed by the mod author. The mod software license is included separately as LICENSE.

[Source and development](https://github.com/ash-945/the-long-drive-controller)
