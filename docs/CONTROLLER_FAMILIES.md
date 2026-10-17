# Controller families and input — 0.6.5 development

Xbox and PlayStation use their usual physical layout: Cross confirms and Circle
cancels on PlayStation. Nintendo follows the letters printed on its buttons:
A (right) confirms, B (lower) cancels, X is upper, and Y is left. A/B and X/Y
are normalized by SDL, and sprites follow the same letter mapping.
Gameplay defaults, saved bindings, menu navigation, and capture all consume
these normalized controls. Existing binding strings do not need conversion.
The 0.6.0 positional Nintendo mapping was corrected after the physical test.

## Input backend

The private Windows x64 SDL2 2.32.10 runtime supports XInput, DirectInput and
HIDAPI through one controller layer. It runs inside the Windows game on both
Windows and Proton. No native Linux plugin is required. SDL is initialized only
for joystick/game-controller input, not audio or video; the mod never replaces
Unity's event system. SDL's button-label hint is explicitly enabled. Mapped
buttons pass through without an additional family swap, including Proton
DirectInput mappings that already report Nintendo letters.

SDL's built-in mappings normalize recognized PlayStation/Nintendo controllers,
including separate sticks, triggers, D-pad, shoulder buttons, and menu buttons.
Digital Nintendo triggers become 0/1 values without changing action handling.
Generic unmapped joysticks use a conventional PlayStation-style raw layout;
DirectInput has no universal button/axis numbering, so uncommon devices may need
a mapping. Add SDL mapping lines to `gamecontrollerdb.txt` beside the plugin DLL
and restart. The mapping file is optional and is not overwritten by installation.

If the SDL runtime is missing or fails to initialize, a logged native XInput-only
fallback preserves controller access. Native XInput is also checked when SDL
enumerates no devices. DirectInput requires the supplied runtime.
The installer and generated development package include the privately named
`TLDControllerSDL2.dll`, its zlib license, and SDL's distribution notice beside
the managed plugin. Other mods' SDL installations are not modified.

## Automatic identity and switching

Detection checks USB vendor identity, recognizable device names, and SDL's
controller type. Sony maps to PlayStation, Nintendo to Nintendo, and Microsoft
to Xbox. Physical identity takes precedence over normalized driver type.
The tested Xbox-mode VID/PID `2dc8:310b` preserves Xbox art for the 8BitDo
Ultimate 2 Wireless when Proton exposes it through DirectInput. Other modes
can share the same name: an unidentified DirectInput mode such as `2dc8:6012`
uses the PlayStation fallback. Other 8BitDo products are not inferred from the
vendor alone.
An unidentified XInput device uses Xbox art; unidentified DirectInput/HID devices
use PlayStation art. Detection uses the identity exposed by Windows/Proton. If
Steam Input or another mapper hides the physical device behind a virtual Xbox
controller, the original controller family cannot reliably be recovered.

All connected SDL devices are polled. The first device is selected initially;
fresh activity on another selects it. An idle device, drift below deadzone, or
releasing another device's stick does not take focus. A switch exposes a clean
input disconnect before the new device state so held actions cannot cross
between controllers. Gameplay still rearms only after neutral. Native keyboard
and mouse input continue to work.

## Presentation

There are 27 unmodified PNGs per family, selected from the supplied Xbox Series,
PlayStation Series, and Nintendo Switch sets. This replaces the earlier Xbox
art and adds stick movement/direction and D-pad artwork. The source manifest in
`Assets/sprite-sources.json` records relative source names and SHA-256 hashes.
Nintendo's supplied face-button art uses position diagrams; its supplied stick
images are also used for stick-click prompts. PlayStation uses the supplied
Select/Start icons for those shared controls, including Share/Options devices.

One TMP atlas contains all 81 images. Family-neutral tokens are resolved only
when rendering trusted controller instructions; native game bindings and saved
profiles retain canonical IDs. In-game prompts, native Controls rows, vehicle
help, driving hints, action cards, settings, binding capture/conflicts, menu
instructions, and the Save-button shortcut all use the same family selection.
Existing native legacy Text/TMP markup is retargeted on a family change,
including hidden labels. Plain action names and typed text are not tokenized.
Text fallback uses family-correct button names if sprite rendering is unavailable.

## Verification

Version 0.6.5 passes 1,525 core checks and 159 isolated native virtual-device/UI
checks under Proton: mapped Xbox, PlayStation, Nintendo, unknown,
and unmapped joystick routes; isolated buttons; independent triggers; stick
signs; diagonal raw POV input; disconnection; and existing legacy/TMP labels
switching families and back. Four-axis digital-trigger pads and multi-controller
activity selection/disconnect boundaries are covered too. A Save-button caption
also restores correctly if family switching precedes its next UI update. The actual game text renderer parsed all 81 sprites.
All 81 parsed TMP icons resolve to their intended sprite asset, index, and
character entry. These are simulated devices, not proof of every physical
USB/Bluetooth mapping.
Nintendo-mode gameplay and camera have been tested on an 8BitDo controller
under Proton. The user confirmed the 0.6.2 Nintendo face-button correction works. PlayStation
mode was also confirmed working by the user on 0.6.4, including the corrected
Square/Cross and Push hints. Native Windows hardware testing remains outside
this Proton validation.

For an isolated opt-in diagnostic run, use `python3 tools/launch.py --capture --self-test`.
Virtual input is read directly by diagnostics and detached before gameplay
polling resumes. It never submits menus, writes saves, or drives gameplay.
`--seconds` optionally bounds an isolated smoke test. Plain launches never create
virtual devices. Use a plain launch for manual controller testing: the user
reported camera input loss during a diagnostic launch and confirmed it recovered
on a normal launch of the same build. Diagnostics no longer compare live native
XInput while SDL owns the device. The log reports the selected device, family, transport,
VID/PID, SDL type, and whether a built-in or generic mapping is used.

To test hardware, connect each controller via USB and Bluetooth as applicable.
Check walking/look, both triggers, inventory, pause, actions, settings, binding
capture/conflict, native Controls, driving hints, and Save/Load without writing
over an existing save. Switch controllers with a prompt already visible; check
hotplug and that toggled sprint/crouch/holds clear on device loss.

SDL references: [controller API](https://wiki.libsdl.org/SDL2/CategoryGameController),
[physical versus label mapping](https://wiki.libsdl.org/SDL2/SDL_HINT_GAMECONTROLLER_USE_BUTTON_LABELS),
[virtual diagnostic API](https://wiki.libsdl.org/SDL2/SDL_JoystickAttachVirtualEx).

Nintendo-mode testing exposed two separate mismatches: 0.6.0 used positional
sprites, and 0.6.1 then also swapped input already normalized by Proton.
Version 0.6.2 removed that extra input swap; the user confirmed it works.
Version 0.6.3 scoped the Xbox-mode exception to its tested device ID so
unknown DirectInput modes can use PlayStation sprites. Version 0.6.4 adds
verification of all 81 parsed sprite identities; the user confirmed the
PlayStation-mode correction works.

## Manual family and driving-hint preferences — 0.6.5

Open Controller settings and use the General tab. Controller family cycles
through Auto, Xbox, PlayStation, and Nintendo with left/right, +/- or confirm.
A manual choice takes priority over detection immediately and after restarting,
reconnecting, or changing active controllers. Select Auto to follow detection
again. This changes button icons; SDL continues to normalize the physical input.
Driving hint box toggles the entire driving helper panel; other interaction
prompts remain available. Both preferences save automatically in the plugin's
BepInEx config, under UI: ControllerFamily and ShowDrivingHints. New installations
use Auto and show driving hints. General defaults restores those two defaults
along with the existing sensitivity settings; saved bindings are separate.

The 0.6.5 diagnostic run also verifies actual BepInEx config save-and-reload
for all four family choices with driving hints disabled. The user confirmed all
three changes work. Captured settings show ten accessible rows; captured action
cards show centered labels/icons and a manual PlayStation selection while the
connected device is detected as Xbox. The override and hint toggle remain saved
in the installed test config. This validation was performed under Proton.
