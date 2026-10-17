# The Long Drive — Controller Support

Targets **The Long Drive v2024.11.26b** (`2024.11.26b_test`), Xbox, PlayStation, and Nintendo controllers, Windows x64 and Linux running the Windows game under Proton. The original game assemblies and keyboard bindings are not modified on disk.

## Development: 0.6.5

Controller settings now include a saved **Controller family** choice (Auto, Xbox, PlayStation, Nintendo) and a **Driving hint box** toggle. Manual family choices take priority over detection until Auto is restored. The uncommon-actions buttons center their text and icons.

Adds DirectInput/HID controller input, automatic Xbox/PlayStation/Nintendo sprite selection, and updated Xbox art. All 81 supplied sprites share one UI path, including existing native labels when changing controllers. Unknown devices use Xbox for XInput and PlayStation for DirectInput. PlayStation Cross confirms menus; Nintendo uses printed A to confirm and B to cancel. Nintendo A/B and X/Y mappings and sprites follow their printed letters.

The development package includes a private SDL2 Windows x64 runtime and its license; extract the whole plugin folder. No additional Linux library is needed under Proton. See [controller families and verification](docs/CONTROLLER_FAMILIES.md) for detection, mapping overrides, tests, and hardware checks still pending. The tested 0.5.5 release remains available on Nexus.

## Current release

**0.5.5 beta** adds native menu styling, inset text with thin black outlines, thicker royal-blue focus borders, and improved Save/Load, pause/title, and exit-confirmation navigation. The on-screen keyboard has been removed; text entry uses a physical keyboard. All 1,436 core checks pass, and the final build was tested and approved under Linux/Proton.

## Features

- Sprint toggles with LS click while moving, and switches off when movement stops, crouching, entering another context, or disconnecting.
- Crouch toggles with B. Click again to stand. LB+B remains lean and does not toggle crouch.
- All 68 named game actions are reachable through direct controls or the View action menu. The menu contains only uncommon actions. Hold A for continuous actions, or use X to toggle a hold.
- Interaction prompts, vehicle help, and the Controls page display Xbox button sprites where available while a controller is connected. The normal keyboard/mouse labels return when it disconnects.
- The controller settings screen exposes camera, object rotation, object movement/lean, menu pointer speed, deadzones, inversion, and separate on-foot/driving bindings. Values save automatically.
- Object rotation defaults to 120 degrees/second before game weight/zoom effects, independently of mouse sensitivity. LB no longer slows every rotation.
- Save/Load starts on Save when available, skips metadata fields, and offers RB to focus Save and LB to return to save slots. Confirmation dialogs keep their own selection. Text entry uses your physical keyboard; no on-screen keyboard opens.

Steering stick sensitivity is adjustable in View → Start, from 0.25 to 2.00 (default 1.00). Lower values give finer small steering corrections; higher values turn sooner. Full stick travel always reaches full steering lock. It also applies to the optional stick pointer-steering mode.

A compact driving hint panel appears at the bottom while seated, covering steering, pedals, ignition, handbrake, clutch, shifting, lights, horn, and exit. It hides with the HUD, menus, sleep, death, and controller disconnection.

## Controls

| Action | Controller |
| --- | --- |
| Move / look | LS / RS |
| Sprint toggle / crouch toggle / jump | LS click / B / A |
| Use, enter, exit / pick up or drop | X / Y |
| Equip or recenter / mount or detach part | RS click / RB |
| Primary / secondary action or move object | RT / LT |
| Rotate held object | Hold LB+RB and move RS |
| Reload or adjust / push | LB+X / LB+A |
| Lean / attach with glue / aim | LB+B / LB+Y / LB+RS click |
| Object distance or scroll adjustment | LB+D-pad up/down |
| Inventory slots 1 / 2 / 3 / 4 | D-pad up / right / down / left |
| Temporary run modifier for compound actions | Hold LB+LS click |
| Steer / throttle / brake while driving | LS / RT / LT |
| Handbrake toggle / ignition | A / B while driving |
| Shift down / up / clutch | LB / RB / LS click while driving |
| Lights / horn / scroll while driving | D-pad up / down / right-left |
| Pause / resume | Start |
| Uncommon actions | View (Back) |
| Controller settings | View, then Start; or View while the game's Settings screen is open |
| Menu selection / confirm / back | D-pad or LS / A / B |
| Virtual menu pointer / click / scroll | RS / A / LT-RT |

Survival is page 1; Tools is page 2. Survival includes Defecate, which requires at least 30% bowel meter and uses the native game action.

The menu hides during an A hold and returns on release. In the action menu, use LB/RB to switch pages, D-pad/LS to select, A to perform an action (hold A for continuous actions; release to stop), X to toggle supported holds, and Y to release all held actions. B or View closes it. Actions still obey native restrictions: direct gears require the appropriate transmission settings, voice/chat requires a multiplayer session, and VR recenter requires VR.

In controller settings, up/down selects a value and left/right adjusts it. A increases or toggles; Y restores defaults; B, Start, or View closes. The screen does not require neutral sticks to reopen. Gameplay still requires neutral after a context change or reconnect to prevent accidental actions.

The settings window has **Sensitivity** and **Bindings** pages; switch with LB/RB or the tabs. On Bindings, X switches on-foot/driving profiles, LT/RT changes action categories, and D-pad/LS selects an action. Press A, release all controls, press a button/trigger/stick direction or up to three controls together, then release to save. Start cancels capture; View closes the window. B can be assigned during capture. Occupied bindings require confirmation before swapping; unbound actions transfer their former control as Unbound. RS click clears the selected binding; Y asks before resetting the current profile. Mouse buttons are available for tabs, rebind, clear, confirm, and cancel. Changes save automatically and update interaction prompts, native Controls rows, action labels, and driving hints immediately. Gameplay waits for neutral after edits. Clicking a native Controls binding row opens its entry in this page.

In Save/Load, RB focuses Save and LB returns to the save slots. A activates the selected native control; it does not bypass overwrite confirmation. Read-only details and screenshot paths are excluded from controller navigation. D-pad/LS can leave the save-name field, and B cancels a confirmation before leaving the screen. Typing save names and computer text uses a physical keyboard. The game still generates a default save name, so a new save does not require typing. RS can still move the in-game computer cursor; B or Start leaves the computer.

## Installation

Requires **BepInEx 5.4.23.5 Windows x64**, installed separately on Windows and Linux/Proton. Download it from the [official BepInEx release](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5).

1. Install BepInEx beside the `TheLongDrive.exe` you launch. On Proton, configure the override below first. Launch once to generate configuration, then close the game.
2. In `BepInEx/config/BepInEx.cfg`, set `HideManagerGameObject = true` under `[Chainloader]`.
3. Extract the mod download into that game folder. The plugin belongs at `BepInEx/plugins/LongDrive.Controller/LongDrive.Controller.dll`.
4. Connect an XInput controller and launch. Avoid simultaneous controller-to-keyboard mappings that cause duplicate input.

On Steam with Proton, add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the launch options, preserving existing wrappers and overrides. In Faugus or another Proton launcher, set the environment variable `WINEDLLOVERRIDES` to `winhttp=n,b` and select the executable in the modded game folder. See the [official Proton setup guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html).

Configuration is saved in `BepInEx/config/ashley.thelongdrive.controller.cfg` inside your game installation. See the [installation and troubleshooting guide](release/INSTALL.txt) and [controller reference](release/CONTROLS.md) for details. Source checkouts need to be built before installation; generated downloads are not committed to this repository.

## Verification and limits

Version 0.5.5 passes 1,436 automated checks and has been tested interactively under Linux/Proton, including the menu styling and navigation updates. Native Windows and exhaustive device/activity coverage remain unverified.

The core checks compare all action names/ordinals with the actual installed game's metadata and exercise action edges, menu queues, modifiers, toggle behavior, safe input gating, trigger independence, and rotation rate calculations. Runtime testing confirms Proton loading, controller detection, rewritten prompt paths, and rendered controller panels. See [testing](docs/TESTING.md), [integration details](docs/COMPATIBILITY.md), and [action inventory](docs/ACTIONS.md).

Every named action has an implementation route; individual activities, multiplayer/VR, native Windows, and every menu/device combination still require hands-on validation. Developer-only/debug shortcuts outside the game's 68 bindable actions are not part of this mapping. The development build embeds 81 supplied PNGs across three controller families, including stick movement artwork. Interaction prompts, Controls rows, and custom panels share the active family. Arbitrary third-party mod menus are not yet covered. The Bindings page supports all 68 named game actions, including analog directions and combinations of up to three controls. Start/View and menu-navigation buttons remain fixed for recovery.

For problems, collect `BepInEx/LogOutput.log` and describe the exact control/context. View is available for menu recovery even while the gameplay neutral gate is blocked. Remove the plugin DLL to disable the mod; keep BepInEx if other mods need it.

## Development

The repository contains the C# plugin, controller graphics, Python build/package tools, and automated core checks. Building requires Python 3, .NET SDK 8, a compatible game installation, and the official BepInEx loader. See [CONTRIBUTING.md](CONTRIBUTING.md) for setup and build commands.

BepInEx and HarmonyX are separate dependencies: [BepInEx](https://github.com/BepInEx/BepInEx), [HarmonyX](https://github.com/BepInEx/HarmonyX).

## Releases
At nexusmods: https://www.nexusmods.com/thelongdrive/mods/248
