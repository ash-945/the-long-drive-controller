# The Long Drive: complete controller support

Status: expanded test build 0.3.2, 2026-10-15. All 68 named game actions have controller routes; controller prompts, an action catalog, text keyboard, and sensitivity settings are implemented. The build compiles and loads under Proton, with 529 core checks passing. Sprint and crouch use toggles; stopping movement cancels sprint. See README.md, docs/ACTIONS.md, docs/COMPATIBILITY.md, and docs/TESTING.md for current scope and remaining hands-on checks. Full release validation is not yet complete.

## Goal and release boundary

A player can launch the game, start or load a session, perform every supported base-game action, change settings, save, and quit using only a controller. Installation can require a desktop; normal play must not require a mouse or keyboard.

Confirmed target: The Long Drive **v2024.11.26b**, with **XInput controllers**, on both **Windows** and **Linux running the Windows game through Proton**. Both operating environments are first-release requirements. The Steam branch name, installed loader, and available test hardware remain unverified. Additional game builds, native Linux game support, and non-XInput input paths are outside the initial target. Steam Deck-specific controls and UX require separate verification. Multiplayer behavior and third-party mod menus need separate compatibility decisions.

Aim for one shared mod package and identical bindings, settings format, and gameplay/UI behavior on both platforms. Allow platform-specific installation instructions where necessary. XInput-style Xbox button prompts are the default; support is based on the input interface presented to the game rather than the physical controller brand.

## Discovery before choosing dependencies

1. Inspect v2024.11.26b and record its Steam branch, Unity/runtime version, assembly format, existing controller behavior, loader, and other installed mods. Confirm the test installation matches this exact build.
2. Enumerate every action from the target build's bindings and observed gameplay. Record analog versus digital input, press/hold/release behavior, and applicable contexts.
3. Inventory all menus, popups, text fields, and UI technologies. Determine which support focus navigation and which require custom handling.
4. Identify how gameplay reads input and whether a loader can run our code early enough to support the title menu. Verify safe input interception and analog values.
5. Choose a compatible loader and input backend only after a small working prototype on Windows and Proton. C# with a thin game-specific adapter is a candidate, not yet a verified stack. Use runtime patching only where required and supported.
6. Compare the game's existing controller input path with an isolated XInput backend. Verify independent triggers, stick ranges, button transitions, connection status, and device selection on both platforms before choosing. Keep platform dependencies behind this boundary.

Deliverable: a compatibility record, action/screen coverage matrix, and runnable feasibility prototype. If a critical action cannot be reached, resolve it or explicitly revise the release scope before proceeding.

## Controller experience

- Separate action maps for menus, walking, driving, manipulating objects, and text entry. Only the active context receives a button press; higher-priority dialogs consume input first.
- Use analog movement, camera, steering, throttle, and braking wherever the target game supports them. Provide separate walking/driving sensitivity, deadzones, inversion, and response curves.
- Keep common actions on direct buttons. Put less frequent actions in contextual radial menus with visible labels. Avoid requiring button combinations for essential actions where practical.
- While manipulating an object, show explicit controls for rotation, distance, placement, attachment/detachment, dropping, and throwing as applicable to the build. Include a precision modifier and a clear exit action.
- Audit all vehicle controls, including starting/stopping, shifting, clutch, handbrake, lights, horn, radio, camera/lean, and entering/exiting wherever present. Resolve control conflicts through contexts rather than silently dropping actions.
- Show prompts derived from current bindings. Offer a manual glyph-family override when device identity is ambiguous.
- Allow keyboard/mouse use alongside the controller. Switch prompts on meaningful input, with a threshold that prevents stick drift from repeatedly changing the display.

Starting layout to validate, using Xbox labels:

| Context | Proposed controls |
| --- | --- |
| Walking | Left stick move; right stick look; A jump; X interact; B crouch/cancel; stick click sprint |
| Driving | Left stick steer; right stick look; RT throttle; LT brake; A handbrake; X interact; dedicated safe exit binding to be playtested |
| Menus | D-pad/left stick focus; A confirm; B back; bumpers tabs; right stick scroll |
| Extra actions | Hold a shoulder button to open a contextual radial menu; select with stick; release to activate; B cancels |

This is a sketch, not a complete binding specification. Final assignments depend on the action inventory and practical conflict testing, including manual transmission and object handling.

## UI support

- Give every supported screen an initial focused control and a visible focus indicator. Preserve useful selection and scroll position when returning.
- Support buttons, sliders, toggles, dropdowns, tabs, long lists, nested dialogs, and dynamically created controls. Keep focused items visible while scrolling.
- Provide predictable directional navigation, repeat timing, confirm/back behavior, and safe confirmation for destructive choices.
- Add a controller-operated virtual pointer for screens that cannot reasonably support directional navigation. Route it through the actual UI interaction path and test dragging and scrolling.
- Support text entry for every required field. Plan an in-game controller keyboard as the shared baseline on Windows and Proton. A platform keyboard can be an optional enhancement after verification. Never assume a physical keyboard or an available Steam overlay.
- Include a fully controller-accessible settings page: remapping, conflict warnings, restore defaults, deadzones, sensitivity, inversion, hold/toggle options, glyph selection, and configuration persistence.
- Ensure remapping can always be cancelled and defaults restored. Prevent the player from losing access to confirm/back or recovery controls.

## Proposed architecture

Controller backend → normalized actions → context/priority router → gameplay adapter or UI adapter.

Keep controller acquisition replaceable while sharing all action routing, settings, prompts, and UI navigation. Prefer in-process gameplay and UI integration, including an in-game virtual pointer, so ordinary play does not depend on desktop mouse/keyboard injection or host desktop APIs. The exact input API and loader remain decisions for the feasibility prototype.

Bindings/settings and prompt rendering share the same action definitions. Keep game-specific hooks isolated from input interpretation and menu behavior. Clear held inputs when focus is lost, a device disconnects, a scene changes, or a context closes. Prevent duplicate actions from the game's existing input path or Steam Input mappings. Document the supported Steam Input configuration.

Use runtime-provided paths for mod configuration and diagnostics. Avoid hardcoded Windows paths or assumptions about Proton prefix locations; explicitly verify file naming, configuration persistence, and loader installation on both platforms. Select one active controller predictably and handle reconnects without relying on a permanently assigned device index.

On unknown builds, report the mismatch clearly; disable incompatible hooks without breaking ordinary game input. Keep diagnostics available for device detection, current context, action routing, and missing hooks.

## Milestones and completion gates

| Stage | Work | Completion gate |
| --- | --- | --- |
| 0. Discovery | Inspect v2024.11.26b, input/UI paths, loader options, and action coverage | Build-specific integration plan and dependency loading verified on Windows and Proton |
| 1. End-to-end prototype | Detect XInput controller; title/pause UI; load game; walk/look; interact; drive; return to menu | Complete this loop on Windows and Proton without mouse/keyboard and without duplicate inputs |
| 2. Gameplay coverage | Vehicle functions, survival/tool actions, precise object manipulation, contextual radials | Every inventoried gameplay action reachable; transitions release held inputs |
| 3. UI coverage | All screens, dialogs, scrolling, pointer fallback, text entry | Every inventoried base-game screen usable with controller alone |
| 4. Configuration and polish | Remapping, prompts, sensitivity, accessibility settings, device recovery | Settings survive restart; recovery and reset paths remain reachable |
| 5. Release validation | Long sessions, supported-device matrix, installation/uninstallation, documentation | Acceptance checklist passes on every advertised build/platform/device |

Do not estimate a reliable release date until discovery and the end-to-end prototype reveal the integration difficulty.

## Acceptance and testing

- Run the release checklist on v2024.11.26b on both Windows and Linux/Proton. Record Windows/Linux versions, Proton version, loader version, controller model/connection type, and Steam Input setting for each result. A pass on one platform does not stand in for the other.
- Establish at least one working, documented Steam Input configuration per platform. Probe enabled/disabled configurations during discovery to identify supported paths and duplicate-input risks; do not assume both will work.
- Verify separate simultaneous LT/RT input, full stick ranges, diagonal deadzones, wired/wireless connections where available, and controller reconnection or changed device ordering.
- Perform a controller-only run from title screen through starting/loading, exploration, maintenance, driving, saving, reloading, settings, and quitting.
- Track every action and screen as unverified, working, or blocked, with a test scenario and tested build.
- Check menu-to-game transitions for accidental interactions and vehicle-to-foot transitions for stuck throttle or camera input.
- Check disconnect/reconnect, device absent at launch, focus loss, scene loading, controller drift, and switching to/from keyboard/mouse. Clear vehicle inputs on disconnect; pause where supported and appropriate to the session mode.
- Verify analog ranges, frame-rate-independent camera behavior, and sensible behavior across different frame rates and UI resolutions.
- Test Steam Input in each documented configuration, especially duplicate input and wrong glyphs.
- Use focused automated checks for context priority, remapping persistence/conflicts, and analog processing once those components exist. In-game verification remains necessary for hooks and screen coverage.
- Test existing saves and uninstallation. Package only redistributable mod files, dependency instructions, supported build details, known limitations, and troubleshooting steps.

## Research and remaining uncertainty

The historical WorkshopTLDMods repository describes a loader/mod ecosystem for The Long Drive but is archived and explicitly marked outdated/moved. It does not establish compatibility with the user's installed build: https://github.com/KolbenLP/WorkshopTLDMods-OUTDATED-Moved-to-GitLab

The official Steam page describes interactive vehicle parts and adjustable viewpoints, supporting the need to include precision interactions in discovery: https://store.steampowered.com/app/1017180/The_Long_Drive/

The supplied build's Unity/runtime, central input and UI paths, BepInEx compatibility setting, and Proton loading are now verified and recorded in docs/COMPATIBILITY.md. Native Windows, exhaustive activities/screens, multiplayer/VR, and additional devices still require runtime validation. Existing controller mods have not been exhaustively surveyed.
