# The Long Drive — XInput Controller Support

**Play The Long Drive with an XInput controller, from walking and driving to menus and object handling.** Xbox button prompts, toggle sprint and crouch, adjustable steering response, and menus styled to match the game bring the controls together in one mod.

**Version 0.5.5 beta • For game v2024.11.26b**

## Features

- **Walking and driving controls:** analog movement, camera, steering, and independent throttle/brake triggers, plus clutch, ignition, shifting, handbrake, lights, and horn.
- **Object handling:** pick up, move, rotate, mount, detach, and adjust object distance. Rotation has its own speed setting.
- **Controller UI:** navigate menus with the D-pad or left stick, or use the right stick as a virtual pointer. Includes scrolling, sensible default selections, and clear blue focus outlines. Text entry uses a physical keyboard.
- **Readable menu styling:** native textured panels and buttons, inset text with a thin black outline, and thicker royal-blue selection borders.
- **Save/Load navigation:** starts on Save when available; RB focuses Save, LB returns to slots, and confirmations default to No. B cancels confirmations.
- **Xbox button prompts:** controller sprites appear in interaction prompts, the Controls page, and custom panels. Keyboard/mouse labels return when the controller disconnects.
- **Uncommon-actions menu:** press View/Back for survival actions, tools, direct gears, camera functions, and system actions. Hold A for continuous actions, or use X to toggle supported holds.
- **Toggle sprint and crouch:** click LS to toggle sprint; stopping movement cancels it. B toggles crouch.
- **Adjustable response:** camera speed, object movement and rotation, pointer speed, deadzones, camera inversion, and steering stick sensitivity save automatically.
- **Compact driving hints:** seated controls appear at the bottom of the screen. No diagnostic overlay in the top-left corner.

All 68 named game actions have controller routes. Survival also includes the native Defecate action, subject to the game's 30% bowel-meter requirement. Actions retain the game's normal restrictions.

## Requirements

- The Long Drive **v2024.11.26b**. The tested build reports `2024.11.26b_test`.
- An **XInput-compatible controller**.
- [BepInEx 5.4.23.5 Windows x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5), installed separately. Use this Windows loader on both Windows and Linux/Proton.

## Installation

1. Close the game. Install BepInEx beside the **TheLongDrive.exe you actually launch**. Linux/Proton users must configure the override below before launching.
2. Launch once to generate BepInEx configuration, then close the game.
3. **Required for this game:** open `BepInEx/config/BepInEx.cfg`. Under `[Chainloader]`, set `HideManagerGameObject = true` and save. This game can otherwise destroy the plugin manager.
4. Extract this mod into the same game folder. The plugin should be at `BepInEx/plugins/LongDrive.Controller/LongDrive.Controller.dll`.
5. Connect your controller and launch. Press **View/Back** for uncommon actions, then **Start** for controller settings.

### Linux / Proton / Faugus

For Steam, add `WINEDLLOVERRIDES="winhttp=n,b" %command%` to the game's launch options, preserving any existing wrappers and overrides.

For Faugus or another Proton launcher, point it at the executable in the folder where you installed the mod. Add the environment variable **WINEDLLOVERRIDES** with the value **winhttp=n,b**. Installing into one game copy will not enable the mod in another. Different Proton prefixes can also have separate settings and saves.

See the [official BepInEx Proton guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html) for the equivalent winecfg setup.

Use XInput mode on controllers that offer multiple modes. Avoid running a simultaneous controller-to-keyboard mapping that produces duplicate input.

## Quick controls

- **Move / look:** LS / RS
- **Sprint / crouch / jump:** LS click / B / A
- **Use, enter, exit / pick up or drop:** X / Y
- **Primary / secondary:** RT / LT
- **Rotate held object:** hold LB + RB and move RS
- **Steer / throttle / brake:** LS / RT / LT
- **Ignition / handbrake:** hold B / A while driving
- **Shift down / up / clutch:** LB / RB / LS click
- **Pause / uncommon actions:** Start / View
- **Controller settings:** View, then Start

The included **CONTROLS.md** covers modifiers, inventory slots, menu navigation, the action menu, and text entry.

Steering sensitivity defaults to **1.00**. Lower values make small corrections gentler; higher values respond sooner. Full stick travel always reaches full steering lock.

## Compatibility and current limits

This is a **beta**, tested interactively on Linux/Proton with an 8BitDo Ultimate 2 Wireless Controller in XInput mode, including the 0.5.5 menu styling and navigation updates. Windows x64 is targeted by the same plugin but has not yet been independently verified. Other game versions are unsupported; other controllers and every vehicle, menu, multiplayer/VR activity, and third-party mod menu have not been exhaustively tested.

Gameplay bindings are editable on a Bindings page beside Sensitivity, with separate on-foot/driving profiles. Use LB/RB to switch settings pages, X to switch profiles, LT/RT to change categories, and A to capture a control or combination. Release all controls first, then press and release the new input. Occupied bindings require confirmation before swapping. Start/View and menu navigation remain fixed for recovery. Free stick movement uses LS/RS text because the supplied stick images depict clicks. Multiplayer and VR actions require their respective game contexts.

## Text entry and menu focus

The on-screen keyboard has been removed. Use a physical keyboard for custom save names and the in-game computer; the game generates a default save name when creating a save. RS still moves the computer cursor, and B or Start leaves it.

The pause/title menu prefers Continue when available, otherwise New Game. Save/Load skips informational fields and lets you leave the name field with D-pad/LS. Save/load/delete and in-game exit confirmations keep controller focus inside the dialog and prefer No; B cancels. Native title-screen Exit retains its immediate-exit behavior.

## Updating, uninstalling, and support

Close the game before updating and replace the existing plugin DLL. Keep only one copy installed. To uninstall, remove `BepInEx/plugins/LongDrive.Controller`; keep BepInEx if other mods need it.

If the mod does not load, check the executable path, BepInEx installation, `HideManagerGameObject` setting, and Proton override. Look for **The Long Drive Controller 0.5.5** in `BepInEx/LogOutput.log`.

When reporting a problem, include the game/mod version, OS and Proton version, controller model/mode, Steam Input setup, the failed action, and the log. After reconnecting or changing contexts, release all controls to neutral before continuing.

## Credits

An unofficial mod for The Long Drive. Uses BepInEx and its HarmonyX runtime dependency; both remain separate from this download. Xbox-style button sprites are Creative Commons assets supplied for the mod with no attribution requirement.

[Source code and development](https://github.com/ash-945/the-long-drive-controller)
