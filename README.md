[Downloads on Nexus Mods](https://www.nexusmods.com/thelongdrive/mods/248)

# The Long Drive — Controller Support

Play with an Xbox, PlayStation, or Nintendo controller, including driving, object handling, and menus. Supports XInput, DirectInput, and HID devices, with matching button prompts.

Version 0.6.7 beta
For The Long Drive v2024.11.26b on Windows x64 and Linux/Proton.

# Features

• Analog movement, camera controls, steering, throttle, and braking.
• Pick up, rotate, attach, and detach objects without switching to a mouse.
• Rebind controls separately for walking and driving, including combinations of up to three inputs. Button prompts update to match.
• Adjustable sensitivity, object rotation speed, and steering response.
• Gradual steering instead of instantly snapping to full lock.
• Toggle sprint and crouch, plus an extra-actions menu for less common controls.
• Controller navigation for menus and save slots, with a right-stick pointer where needed.
• Automatic controller icons, or a saved Xbox, PlayStation, or Nintendo override.

Custom text still requires a physical keyboard; there’s no on-screen keyboard.

# Quick controls

These use Xbox button names. On other controllers, follow the matching icons.

• Move / look: left stick / right stick
• Sprint / crouch / jump: left-stick click / B / A
• Use, enter, or exit: X
• Pick up or drop: Y
• Primary / secondary action: RT / LT
• Rotate a held object: hold LB + RB and move the right stick
• Steer / accelerate / brake: left stick / RT / LT
• Handbrake / ignition: A / hold B while driving
• Shift down / up: LB / RB
• Clutch: left-stick click
• Pause / extra actions: Start / View

To open controller settings, press View, then Start, or press View while the game’s Settings menu is open. Use LB/RB to switch between General and Bindings.

General settings includes controller icons, sensitivity, steering response, and optional driving hints. Start/View and basic menu controls stay fixed so you can recover from a bad binding.

# Installation

Requires BepInEx 5.4.23.5 Windows x64, installed separately. Use the Windows loader on both Windows and Proton.

1. Close the game and install BepInEx beside the TheLongDrive.exe you launch.
2. On Proton, set the launch option below before the first launch.
3. Launch the game once, then close it.
4. In BepInEx/config/BepInEx.cfg, set HideManagerGameObject = true under [Chainloader].
5. Extract the mod into the game folder. Keep the entire BepInEx/plugins/LongDrive.Controller folder, including both DLLs and the license files.
6. Connect your controller and launch the game.

Steam/Proton launch option:

WINEDLLOVERRIDES="winhttp=n,b" %command%

Preserve any existing launch wrappers or DLL overrides. You don’t need an additional native Linux library.

When updating, replace all plugin files and keep only one installed copy. Preserve your configuration and any custom gamecontrollerdb.txt.

To uninstall, remove the LongDrive.Controller plugin folder. Leave BepInEx installed if other mods use it.

# Compatibility and troubleshooting

Tested under Proton with an 8BitDo controller in Xbox, Nintendo, and generic DirectInput modes. Native Windows hardware and genuine PlayStation controllers haven’t been independently verified.

Steam Input may present your controller as an Xbox device. If the icons are wrong, choose the correct family in General settings. Unusual DirectInput layouts may need a custom SDL mapping in gamecontrollerdb.txt beside the plugin.

If only XInput works, check that the bundled TLDControllerSDL2.dll is installed. Release the sticks and triggers after reconnecting or leaving menus before resuming play.

For bug reports, include your game and mod versions, OS/Proton version, controller model and mode, Steam Input settings, what went wrong, and BepInEx/LogOutput.log.
