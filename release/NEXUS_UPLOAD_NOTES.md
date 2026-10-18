# Nexus listing fields

**Mod name:** The Long Drive - Controller Support

**Summary:** Xbox, PlayStation and Nintendo controller support with gradual adjustable steering, automatic button icons, saved family override, editable bindings, optional driving hints, and controller menus. For v2024.11.26b.

**Version:** 0.6.7

**Status:** Beta

**Main download name:** Controller Support 0.6.7 Beta

**File description:** Adds gradual steering with a saved time-to-full-lock setting (default 0.75 seconds), separate stick response curve, and guards against duplicate native joystick steering. Retains Xbox/PlayStation/Nintendo support, 81 icons, saved family override, optional driving hints, and darker centered menu buttons. Includes the plugin, required private SDL2 Windows x64 runtime and notices, installation instructions, controls reference, credits, changelog, and hash manifest. Requires BepInEx 5.4.23.5 Windows x64 separately. For The Long Drive v2024.11.26b; Linux/Proton needs the winhttp override. Copy the whole plugin folder when upgrading.

**External requirement:** BepInEx 5.4.23.5 Windows x64 — https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5

**Long description:** NEXUS_DESCRIPTION.md (Markdown/plain text) or NEXUS_DESCRIPTION_BBCODE.txt (BBCode).

Upload LongDrive-Controller-Support-0.6.7-Beta.zip. Do not upload the workspace,
local game, or ControllerPrototype archives. Manual game-root installation is
supported; mod-manager deployment has not been verified. This package has not
been uploaded or published automatically. Update the existing listing rather
than creating a duplicate just because controller support is broader now.

The sprites are supplied Creative Commons assets requiring no attribution,
as confirmed by the author. Match software permissions to the included LICENSE;
controller sprites and the included SDL2 runtime retain their separate terms.
The official SDL2 zlib license and distribution notice are inside the archive.

Validation: 0.6.7 passes 1,554 core and 172 native input/UI/config checks under
Proton, including a fixture using the actual DriveStuff method, saved steering
time reload, and all 81 sprite identities. Core timelines cover 30/60/144 FPS,
partial targets, reversal, centering, stalls and resets. Prior interactive tests
confirmed controller modes, family preferences, hint toggle and centered cards.
The user approved preparing this package after the steering test session. Do not
claim exhaustive vehicle handling, genuine Sony hardware, or native Windows
hardware validation. The 0.6.6 changelog retains its historical validation notes.

Suggested images: interaction prompts for each family, driving hints, General
settings showing family selection, and centered uncommon-action cards. Use
captures from 0.6.7 for the darker buttons; 0.6.5 captures use the lighter tint.
No personal configuration, saves, game artwork or source is included.

**Source:** https://github.com/ash-945/the-long-drive-controller

**Installation reminder:** HideManagerGameObject=true is required under
[Chainloader] in BepInEx.cfg. Include TLDControllerSDL2.dll beside the plugin.
A plugin-loading log message alone does not prove the game kept it active.
