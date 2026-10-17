# Controller families and input — 0.6.2 development

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
A specific model match preserves Xbox art for the 8BitDo Ultimate 2 Wireless when Proton exposes it as a named DirectInput device. Other 8BitDo products are not inferred from the vendor alone.
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

Version 0.6.2 passes 1,509 core checks. Earlier isolated Proton runs passed
139 native virtual-device/UI checks on 0.6.0 and 0.6.1: mapped Xbox, PlayStation, Nintendo, unknown,
and unmapped joystick routes; isolated buttons; independent triggers; stick
signs; diagonal raw POV input; disconnection; and existing legacy/TMP labels
switching families and back. Four-axis digital-trigger pads and multi-controller
activity selection/disconnect boundaries are covered too. A Save-button caption
also restores correctly if family switching precedes its next UI update. The actual game text renderer parsed all 81 sprites.
The changed 0.6.2 diagnostic expectations have not yet been rerun. These are
simulated devices, not proof of every physical USB/Bluetooth mapping.
Nintendo-mode gameplay and camera have been tested on an 8BitDo controller
under Proton. The latest face-button correction, PlayStation mode, and native
Windows still need hands-on validation.

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
Version 0.6.2 keeps the corrected letter sprites and removes that extra input
swap. Hardware retesting of the correction remains pending.
