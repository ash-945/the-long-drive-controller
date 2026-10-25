# The Long Drive — Controller Support

**Xbox, PlayStation, and Nintendo controller support for gameplay, driving, and menus.** Supports XInput, DirectInput, and HID controllers, with automatic button icons and a saved manual family override.

**Version 0.6.7 beta • The Long Drive v2024.11.26b • Windows x64 and Linux/Proton**

[Downloads on Nexus Mods](https://www.nexusmods.com/thelongdrive/mods/248) · [Installation guide](release/INSTALL.txt) · [Controls reference](release/CONTROLS.md)

The source for 0.6.7 is on the [main branch](https://github.com/ash-945/the-long-drive-controller/tree/main).

## Features

- **Three controller families:** automatically identifies Xbox, PlayStation, and Nintendo devices and selects matching icons throughout interaction prompts, vehicle help, native Controls rows, and custom controller menus. Includes 81 supplied sprites and refreshed Xbox artwork.
- **Manual family selection:** General settings offers Auto, Xbox, PlayStation, and Nintendo. A manual choice saves and overrides detection across restarts and reconnections. Selecting Auto restores detection.
- **Gradual steering:** the wheels approach the stick target over time, with full lock still available when held. Steering time to full lock defaults to 0.75 seconds; higher values turn more slowly. Steering stick curve separately shapes partial-stick input. Both settings save automatically.
- **Gameplay and object handling:** analog movement, camera, pedals and steering, plus pickup, movement, rotation, mounting, detachment, and object-distance adjustment. Object rotation has its own speed setting.
- **Controller menus:** D-pad/left-stick navigation, a right-stick virtual pointer, scrolling, blue focus borders, and darker native textured buttons with outlined text. Uncommon-action captions and their icons are centered.
- **Editable bindings:** all 68 named game actions have controller routes. Separate on-foot/driving profiles accept buttons, triggers, stick directions, and combinations of up to three controls. Hints follow saved bindings.
- **Toggle sprint and crouch:** sprint stops when movement stops. The uncommon-actions menu covers survival, tools, direct gears, camera, and system actions.
- **Optional driving hints:** the seated helper panel can be disabled in General settings without hiding other interaction prompts.
- **Save/Load navigation:** sensible default selections, RB to focus Save, LB to return to slots, and confirmations that prefer No. Custom text uses a physical keyboard; there is no on-screen keyboard.

## Controller families

Known controllers are identified through vendor information, device names, and SDL controller type. Unknown XInput controllers use Xbox icons; unknown DirectInput/HID controllers use PlayStation icons. A manual family choice takes priority over both.

Steam Input or another mapper can expose a physical controller as a virtual Xbox device. Unusual DirectInput layouts may need a custom SDL mapping in `gamecontrollerdb.txt` beside the plugin.

## Quick controls

| Action | Default Xbox names |
| --- | --- |
| Move / look | LS / RS |
| Sprint / crouch / jump | LS click / B / A |
| Use, enter, exit / pick up, drop | X / Y |
| Primary / secondary | RT / LT |
| Rotate held object | Hold LB + RB and move RS |
| Steer / throttle / brake | LS / RT / LT |
| Handbrake / ignition | A / hold B while driving |
| Shift down / up / clutch | LB / RB / LS click |
| Pause / uncommon actions | Start / View |
| Controller settings | View, then Start; or View while native Settings is open |
| Menu select / confirm / cancel | D-pad or LS / A / B |

Open Controller settings → General for family selection, driving hints, sensitivity, and steering rate. **Steering time to full lock** ranges from 0.10 to 3.00 seconds, default 0.75. **Steering stick curve** ranges from 0.25 to 2.00, default 1.00; existing saved sensitivity values remain valid. General defaults restores Auto family, enabled driving hints, and default response settings without resetting bindings.

LB/RB switches between General and Bindings. Bindings has separate walking/driving profiles, control capture, clear/reset, and occupied-binding confirmation. Start/View and menu navigation stay fixed for recovery. See the [complete controller reference](release/CONTROLS.md) for modifiers, inventory, action-menu controls, rebinding, Save/Load, and computer controls.

## Installation

Requires **The Long Drive v2024.11.26b** (tested build: `2024.11.26b_test`) and **[BepInEx 5.4.23.5 Windows x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5)**, installed separately. Use the Windows loader and included Windows SDL2 runtime on Windows and Linux/Proton.

1. Close the game. Install BepInEx beside the `TheLongDrive.exe` you actually launch. On Proton, configure the override below before the first launch.
2. Launch once to generate configuration, then close the game. In `BepInEx/config/BepInEx.cfg`, set `HideManagerGameObject = true` under `[Chainloader]`.
3. Extract the mod download into that game folder. Copy the **whole** `BepInEx/plugins/LongDrive.Controller` folder, including `LongDrive.Controller.dll`, `TLDControllerSDL2.dll`, and SDL license notices. The managed DLL alone is insufficient for DirectInput support.
4. Connect your controller and launch. Release controls to neutral after reconnecting or leaving menus before resuming gameplay.

For Steam/Proton, add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to launch options, preserving existing wrappers and overrides. No additional native Linux library is required. See the [BepInEx Proton guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html).

When updating, copy all files from the plugin folder and keep only one installed copy. Preserve your config and custom `gamecontrollerdb.txt`; neither is included in the download. Preferences save in `BepInEx/config/ashley.thelongdrive.controller.cfg`. To uninstall, remove the plugin folder; keep BepInEx if other mods need it. See [installation and troubleshooting](release/INSTALL.txt).

## Verification and compatibility

Version 0.6.7 passes **1,554 core checks**, **172 native input/UI/config checks under Proton**, and verification of all **81 parsed sprite identities**. Steering checks cover multiple frame rates and an inactive fixture using the game's actual steering calculation. The controller modes and menu preferences have been tested interactively with an 8BitDo controller in Xbox, Nintendo, and generic DirectInput modes.

Native Windows hardware and genuine Sony hardware have not been independently verified. Vehicle handling, multiplayer/VR contexts, every controller/USB/Bluetooth combination, and third-party mod menus are not exhaustively tested. Native keyboard/mouse controls remain available; duplicate native joystick steering is suppressed while a mod controller is connected so it cannot bypass the steering ramp.

For a bug report, include game/mod versions, OS/Proton, controller model and mode, Steam Input setup, the failed action, and `BepInEx/LogOutput.log`. If icons are wrong, check Controller family; if only XInput works, check the bundled SDL runtime and its log messages.

## Source and development

Build from the [main branch](https://github.com/ash-945/the-long-drive-controller/tree/main). The repository includes C# source, supplied controller graphics, Python build/package tools, tests, and release documentation. Building requires Python 3, .NET SDK 8, the compatible game installation, and the official BepInEx loader. See [CONTRIBUTING.md](CONTRIBUTING.md) and [controller backend details](docs/CONTROLLER_FAMILIES.md).

Game files, decompiled source, local runtimes/tool caches, saves, user config, and generated downloads are not committed. Mod code uses the included [LICENSE](LICENSE). Supplied controller sprites retain their separate Creative Commons terms; bundled SDL2 includes its zlib license and distribution notice. BepInEx and HarmonyX are separate loader dependencies.
