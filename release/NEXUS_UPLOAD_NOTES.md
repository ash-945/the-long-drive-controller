# Nexus listing fields

**Mod name:** The Long Drive - Controller Support

**Summary:** Xbox, PlayStation and Nintendo controller support for gameplay and menus, automatic button icons, saved family override, editable bindings, toggle sprint/crouch, optional driving hints, and readable controller menus. For v2024.11.26b.

**Version:** 0.6.6

**Status:** Beta

**Main download name:** Controller Support 0.6.6 Beta

**File description:** Adds DirectInput/HID support, automatic Xbox/PlayStation/Nintendo icons, 81 sprites, a persistent family override, a driving-hints toggle, centered action captions, and darker textured buttons. Includes the plugin, required private SDL2 Windows x64 runtime and notices, installation instructions, controls reference, credits, changelog, and hash manifest. Requires BepInEx 5.4.23.5 Windows x64 separately. For The Long Drive v2024.11.26b; Linux/Proton needs the winhttp override. Copy the whole plugin folder when upgrading.

**External requirement:** BepInEx 5.4.23.5 Windows x64 — https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5

**Long description:** NEXUS_DESCRIPTION.md (Markdown/plain text) or NEXUS_DESCRIPTION_BBCODE.txt (BBCode).

Upload LongDrive-Controller-Support-0.6.6-Beta.zip. Do not upload the workspace,
local game, or ControllerPrototype archives. Manual game-root installation is
supported; mod-manager deployment has not been verified. This package has not
been uploaded or published automatically. Update the existing listing rather
than creating a duplicate just because controller support is broader now.

The sprites are supplied Creative Commons assets requiring no attribution,
as confirmed by the author. Match software permissions to the included LICENSE;
controller sprites and the included SDL2 runtime retain their separate terms.
The official SDL2 zlib license and distribution notice are inside the archive.

Validation: 0.6.6 passes 1,525 core checks. The preceding 0.6.5 runtime passed
159 native input/UI/config checks and all 81 sprite identity checks under Proton.
The user confirmed Nintendo and generic DirectInput mode behavior, family
preferences, driving-hint toggle, and centered menu captions. The final darker
0.6.6 button tint has not yet had a hands-on visual approval. Do not describe
this as genuine Sony hardware or native Windows hardware validation.

Suggested images: interaction prompts for each family, driving hints, General
settings showing family selection, and centered uncommon-action cards. Use
captures from 0.6.6 for the darker buttons; 0.6.5 captures use the lighter tint.
No personal configuration, saves, game artwork or source is included.

**Source:** https://github.com/ash-945/the-long-drive-controller

**Installation reminder:** HideManagerGameObject=true is required under
[Chainloader] in BepInEx.cfg. Include TLDControllerSDL2.dll beside the plugin.
A plugin-loading log message alone does not prove the game kept it active.
