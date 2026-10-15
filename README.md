# The Long Drive controller support — test build 0.4.0

Targets **The Long Drive v2024.11.26b** (`2024.11.26b_test`), XInput controllers, Windows x64 and Linux running the Windows game under Proton. The original game assemblies and keyboard bindings are not modified on disk.

## What changed

- Sprint toggles with LS click while moving, and switches off when movement stops, crouching, entering another context, or disconnecting.
- Crouch toggles with B. Click again to stand. LB+B remains lean and does not toggle crouch.
- All 68 named game actions are reachable through direct controls or the View action menu. The menu contains only uncommon actions. Hold A for continuous actions, or use X to toggle a hold.
- Interaction prompts, vehicle help, and the Controls page display Xbox button sprites where available while a controller is connected. The normal keyboard/mouse labels return when it disconnects.
- The controller settings screen exposes camera, object rotation, object movement/lean, menu pointer speed, deadzones, inversion, and separate on-foot/driving bindings. Values save automatically.
- Object rotation defaults to 120 degrees/second before game weight/zoom effects, independently of mouse sensitivity. LB no longer slows every rotation.
- An on-screen keyboard handles normal menu text fields and the in-game computer, including its musical keyboard. Gameplay and underlying menu inputs are suppressed while custom panels are open.

The earlier timed UI verification has been removed. Opening a menu never schedules automatic navigation or closure.

Steering stick sensitivity is adjustable in View → Start, from 0.25 to 2.00 (default 1.00). Lower values give finer small steering corrections; higher values turn sooner. Full stick travel always reaches full steering lock. It also applies to the optional stick pointer-steering mode.

The top-left diagnostic overlay has been removed. A smaller driving hint panel appears at the bottom while seated, covering steering, pedals, ignition, handbrake, clutch, shifting, lights, horn, and exit. It hides with the HUD, menus, sleep, death, and controller disconnection.

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

The text keyboard opens when an editable field is focused, or when using the in-game computer. D-pad/LS selects keys; A types, X deletes, Y changes case, LB switches symbols, RB inserts space, Start finishes, and B cancels. RS moves the in-game computer cursor. Its letter/number keys also operate the computer's music mode.

## Installation

1. Close the game. Install the official **BepInEx 5.4.23.5 Windows x64** release alongside `TheLongDrive.exe` (also use the Windows loader under Proton): https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5
2. Extract `dist/ControllerPrototype-0.4.0.zip` into the game folder. Set `HideManagerGameObject = true` under `[Chainloader]` in `BepInEx/config/BepInEx.cfg`; this game otherwise destroys the visible plugin manager during startup. The local installer applies this setting and backs up the prior configuration.
3. On Windows, start normally. Under Proton, include `WINEDLLOVERRIDES="winhttp=n,b" %command%` in Steam launch options, preserving existing wrapper/options. Official guide: https://docs.bepinex.dev/articles/advanced/proton_wine.html
4. Start with an XInput controller and no simultaneous controller-to-keyboard mapping. The tested local device is an 8BitDo Ultimate 2 Wireless Controller on XInput slot 0 under Proton Experimental. Record Steam Input settings when testing other setups.

The local working test copy is `.local/test-game`, with its own prefix/settings/saves in `.local/proton-smoke`. Reopen it with `python3 tools/launch.py`. The supplied game folder is unchanged. Configuration lives in the test copy's `BepInEx/config/ashley.thelongdrive.controller.cfg`; the new rotation setting is `ObjectRotationDegreesPerSecond`.

## Verification and limits

The core checks compare all action names/ordinals with the actual installed game's metadata and exercise action edges, menu queues, modifiers, toggle behavior, safe input gating, trigger independence, and rotation rate calculations. Runtime testing confirms Proton loading, controller detection, rewritten prompt paths, and rendered action/keyboard panels. See [testing](docs/TESTING.md), [integration details](docs/COMPATIBILITY.md), and [action inventory](docs/ACTIONS.md).

Every named action has an implementation route; individual activities, multiplayer/VR, native Windows, and every menu/device combination still require hands-on validation. Developer-only/debug shortcuts outside the game's 68 bindable actions are not part of this mapping. The 16 supplied Xbox PNGs are embedded in the plugin and appear inline in interaction prompts, Controls rows, and custom panels. Stick movement remains text because the supplied stick images depict clicks. Arbitrary third-party mod menus are not yet covered. The Bindings page supports all 68 named game actions, including analog directions and combinations of up to three controls. Start/View and menu-navigation buttons remain fixed for recovery.

For problems, collect `BepInEx/LogOutput.log` and describe the exact control/context. View is available for menu recovery even while the gameplay neutral gate is blocked. Remove the plugin DLL to disable the mod; keep BepInEx if other mods need it.

## Build

Requires Python 3, .NET SDK 8, and the official loader extracted into `.tools/bepinex`:

```sh
python3 tools/build.py "/path/to/The Long Drive.v2024.11.26b.Test"
python3 tools/install.py "/path/to/game" --with-loader
```

The build uses the system SDK or `.tools/dotnet/dotnet`, compiles against local game references, runs the checks, and packages only our plugin/docs/hash manifest. Game files, decompiled source, and loader binaries are not redistributed. Loader sources/licenses: https://github.com/BepInEx/BepInEx and https://github.com/BepInEx/HarmonyX.
