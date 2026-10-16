# Integration findings: v2024.11.26b

The supplied installation reports `2024.11.26b_test`, Unity `2022.3.12f1`, and a managed Mono runtime. It contains both the legacy input module and Unity Input System assemblies. The game-specific input path inspected for this prototype is `inputscript`, which publishes 68 action identifiers and reads its configurable bindings through `GetAxis`, `GetKey`, `GetKeyDown`, and `GetKeyUp`.

The prototype adds controller values at those methods, preserving ordinary game bindings. Camera deltas are added after `fpscontroller.GetLook` applies mouse sensitivity, keeping controller inversion and speed separate. The UI adapter uses Unity's `EventSystem`, `Selectable` navigation, and pointer events rather than desktop mouse injection.

Loader: official BepInEx 5.4.23.5 Windows x64, for both native Windows and the Windows game under Proton. Runtime startup logging confirmed that the plugin loads and its Harmony hooks install under Proton Experimental. The initial visible plugin manager was disabled/destroyed before its first update; the compatibility fix is `[Chainloader] HideManagerGameObject = true`. With that setting applied, live logs confirm Start/Update and input hook execution, and the game-rendered screenshot confirms the panel appears. Native Windows execution remains unverified.

Controller backend: XInputGetState, trying XInput 1.4, 1.3, then 9.1.0 DLL entry points within the Windows process. Both platforms use the same plugin DLL. Linux host input drivers and Proton's translation path still require device testing. The active controller is the first connected XInput slot; device loss forces neutral rearming before another slot can take over.

## Current coverage (0.4.0)

See [ACTIONS.md](ACTIONS.md) for all 68 named actions with their exact direct bindings and action-menu pages. Automated checks compare names and ordinals against the installed Assembly-CSharp metadata. Normal computer text input and music keys also receive a local controller keyboard adapter; desktop input is not synthesized.

The controller action state is sampled once per frame, so multiple game consumers cannot retoggle sprint/crouch. Sprint cancels on movement stop or crouch; crouch persists after releasing B. Object rotation uses a separate native rate adapter, rather than a per-frame camera delta, and no longer receives an automatic LB slowdown. Normal mouse sensitivity is not changed.

Prompt replacement is limited to presentation sites in mainscript, vehicle help, and controls rows. Native binding-name comparisons used by gameplay retain their original semantics. Runtime logging confirmed 94 rewritten presentation sites. While connected, clicking a controls-row binding opens its editable controller binding rather than the native keyboard rebinding capture. Controller assignments are editable on the Bindings page beside Sensitivity. Native Controls binding rows open their matching binding entry. Menu navigation and Start/View recovery remain fixed.

## Menu recovery and testing incident

An explicitly enabled timed UI-check routine caused panels to appear and disappear during user testing of 0.2.0. Logs showed no exception. This routine and its launcher option have been removed from 0.2.1. Optional screenshot capture only records screens; it never navigates, opens, or closes them.

Custom panels and native UI now read controller button edges independently of the gameplay neutral gate. View can recover/open/close controller screens with a displaced stick. The neutral gate still prevents accidental gameplay actions after context changes/reconnects. Native UI input modules are restored when custom panels close; keyboard text entry has its own button-release check.

## Remaining verification

The user confirmed the first prototype and most of 0.2.0 work on Proton, and reported slow rotation and the timed-panel issue. The final 0.2.1 loads, detects the controller, and executes gameplay/camera hooks; its user-opened settings screen has been visually verified. The final toggle/rotation changes still require a fresh hands-on pass. Action coverage proves routes exist, not that every game condition, vehicle, multiplayer/VR session, or modded screen has been exercised. Native Windows remains untested. Developer/debug shortcuts outside the 68 game binding identifiers are not mapped.

## Xbox prompts and driving UI (0.3.2)

The user confirmed 0.3.1 icons and seated driving hints look good. Sixteen supplied Xbox PNGs are embedded in the plugin; TMP handles inline icons, and legacy Unity UI Text prompts receive a TMP visual replacement. Keyboard labels return on controller disconnect. Movement-stick names remain text because the supplied stick icons depict clicks. The top-left diagnostic overlay and its settings entry are removed.

Driving hints are now smaller. Steering stick sensitivity adjusts the response curve after the deadzone, retaining neutral and full lock. Default 1.00 preserves the existing linear response. It affects ordinary stick steering and optional pointer-steering, without changing walking or look sensitivity. All 529 core checks pass; hands-on validation of the new size and steering setting is pending.

## Editable bindings (0.4.0)

A shared profile now drives gameplay input and presentation, including native interaction/vehicle help, Controls rows, custom action labels, mouse-axis help, settings toggle descriptions, and the seated driving panel. All 68 named actions support independent on-foot/driving assignments with button, trigger, stick direction, or up to three-control combinations. Occupied controls require swap confirmation. Saved profiles are validated atomically. Steering response follows resolved actions after remapping.

Build and 1,420 core checks pass. On 2026-10-16, the end-user installation loaded 0.4.0 under Proton with BepInEx 5.4.23.3 after setting HideManagerGameObject=true. Logs confirm Start/Update, XInput slot 0, all 16 parsed sprites, settings and both binding profiles, controller capture, and a binding save. The saved configuration contains a changed driving Use binding (LS Up). Screenshots confirm the Sensitivity and Bindings pages render, and the user confirmed the feature works. The package DLL matches the tested installation by SHA-256. BepInEx 5.4.23.5 Windows x64 remains the recommended loader. Native Windows, restart persistence, every possible binding combination, and exhaustive vehicle/menu/device coverage are not claimed as independently verified.
