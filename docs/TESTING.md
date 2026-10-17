# Controller test route — 0.3.2

Use v2024.11.26b and record OS/Proton, controller/connection type, BepInEx version, and Steam Input configuration. The supplied game folder is unchanged; `.local/test-game` and `.local/proton-smoke` hold the working test copy and isolated prefix/settings/saves.

1. Move and click LS once. Release the click: sprint must continue. Stop moving: sprint must turn off. Move again: walk until LS is clicked. Repeat, then crouch: sprint must stop.
2. Tap B to crouch; release B and stand still: remain crouched. Tap B again: stand. LB+B must lean without toggling crouch.
3. Pick up an expendable object with Y. Hold LB+RB and move RS: rotate at a practical speed. Open View → Start and change Object Rotation Speed; verify only rotation changes. Try distance with LB+D-pad up/down and movement with LT+RS.
4. Open the View action menu, change pages with LB/RB, select with D-pad/LS, perform with A, hold/release supported modifiers with X, and release all holds with Y. Test rare actions from [ACTIONS.md](ACTIONS.md). A tap must not accidentally trigger the game's long-press behavior.
5. Open controller settings via View → Start. Adjust values with left/right; toggle with A; close with B. Reopen repeatedly. Hold a stick off center and verify View still opens/closes the panel. Settings must not disappear on a timer. In native Settings, View also opens controller settings directly.
6. Check interaction prompts and vehicle help: controller labels should appear instead of keyboard/mouse binding names. Open native Settings → Controls and check row labels. Use B to close open dropdowns before leaving the settings screen.
7. Confirm D-pad navigation keeps selected items visible in long lists. Test the virtual pointer, click/release, slider dragging, and trigger scrolling.
8. Focus an editable name/seed field: the on-screen keyboard should open. Type/delete, change case/symbols, clear, confirm, and cancel. Test the in-game computer and its music-mode keys; RS moves the computer cursor. Check notes stop when leaving it.
9. Enter a driver's seat using contextual X. Check steering, simultaneous throttle/brake, ignition, handbrake, shifts/clutch, lights, horn, direct gears via the menu, and exit. Respect native manual/automatic transmission settings. The optional pointer-driving mode uses RS for steering and, when enabled, pedals.
10. Pause/resume and switch back to keyboard/mouse. Disconnect/reconnect while safely stationary and with a trigger held: gameplay input must wait for neutral. Menus must remain recoverable. Verify labels return to keyboard/mouse when the controller is disconnected.

Collect `BepInEx/LogOutput.log` and the precise action/context for failures. Game logging is in `.local/proton-smoke/unity-windowed.log` for the local launcher.

## Evidence

- Installed game inspected: managed Unity 2022.3.12f1, application version `2024.11.26b_test`.
- BepInEx 5.4.23.5 loads under Proton Experimental 11.0-100 with HideManagerGameObject enabled.
- XInput 1.4 detects slot 0; the game identified an 8BitDo Ultimate 2 Wireless Controller.
- User confirmed initial movement/menu prototype works, and most expanded actions work.
- Core checks cover all 68 actual identifiers and input edges, queues, modifiers, triggers, sprint/crouch state, and object rotation rate across frame rates/mouse sensitivities. Latest run is recorded in `.local/build-0.2.1.log`.
- Expanded-build runtime logs confirm thousands of input calls and camera hook execution; 94 prompt presentation sites were rewritten.
- Game-rendered captures verify the action menu and on-screen keyboard render. The earlier controls capture occurred before the native tab finished opening; it does not prove every visible controls row or modal path.
- The timed UI verification that interfered with user testing has been removed. No automated panel navigation remains.
- Final 0.2.1 loads under Proton, detects XInput slot 0, and executes gameplay/camera hooks. The user-opened controller settings screen was captured and visually verified with all eight settings, including the independent 120 deg/s object rotation value. Unlike the prior UI check, this capture did not navigate or close panels.
- Full 0.2.1 toggle/rotation/activity pass: pending. Native Windows, multiplayer/VR and exhaustive screen/activity coverage remain unverified.

Compilation, metadata checks, and rendered UI do not replace hands-on game testing. Do not advertise untested build/device/platform combinations as verified.

## 0.3.1 visual checks

Check all 16 embedded Xbox sprites in native interaction prompts, Controls rows, and custom menus. No literal sprite tags should be visible. Legacy Unity UI Text prompts use a TMP visual replacement; game-owned text values and updates remain intact. Plain LS/RS movement labels remain text. The top-left diagnostic panel and its settings row are removed.

While seated, check the bottom driving hints match the actual bindings. Pause, hide the HUD, open a custom panel, disconnect, and exit the seat: hints must hide. Runtime sprite verification counts parsed sprite elements because this game’s TMP summary spriteCount is stale after mesh generation.

## 0.3.2 steering and hint sizing

The user confirmed the 0.3.1 icons and seated hints look good. Driving hints now use 13px text in a 480×98 reference panel (previously 17px, 610×126). Check readability at the preferred display resolution.

In View → Start, adjust Steering stick sensitivity (last row). A and right increase, left decreases, and Y restores 1.00. Values save automatically. Compare small stick movements at 0.50, 1.00, and 2.00; full left/right must retain full lock at every value. Walking, menus, camera, pedals and clutch should retain their prior response. Verify the optional pointer-steering mode separately. Core checks cover linear default, precision, increased response, symmetry, endpoints, bounds, and monotonic response.

## Action-menu holds and uncommon entries

Hold A on Hum, Urinate, Smoke, Zoom, or Voice chat in a supported native context: the menu hides and the action continues until release, then the menu returns. No jump, primary action, navigation, or delayed tap should leak through. Test a short A tap, X toggle and release, Y release-all, B/View close, native pause, focus loss, and disconnect during a hold. Common walking/driving bindings must be absent; uncommon actions and direct gears remain available. Restart the test game to load the updated DLL.

## 0.3.4 Survival menu

Confirm Survival is page 1 and Tools is page 2. Select the eighth Survival entry, Defecate, with A: below 30% bowel meter it shows a message; at or above 30% it calls the native player action once. X must not toggle this one-shot action. Verify the bowel meter, spawned item, and sound in game.

## 0.4.0 binding profiles

Open controller settings and switch to Bindings with LB/RB. Check all nine categories in both on-foot/driving profiles (X changes context, LT/RT category). Select with D-pad/LS and A. Confirm A does not capture itself: release first, press the desired control/chord, and release to finish. Try B, a trigger, stick direction, LB+button, and three buttons. Start cancels; View closes; disconnect/focus loss must discard capture. Invalid oversized chords should permit retry.

Try assigning Jump to X: confirm the swap with Use, then verify both actions and both native prompt labels. Change a driving pedal and ignition; verify seated hints, vehicle help, and native Controls rows update. Change camera and steering axes; sensitivity must follow steering rather than the old physical stick. Test longer chords for wrapping in driving hints. Test clearing, swap cancellation, reset cancellation, reset confirmation, and persistence after restarting. Sensitivity defaults must not reset bindings. UI navigation/Start/View remain fixed.

Automated coverage includes every named action in both contexts, analog values, configuration round trips, atomic rejection of malformed/duplicate mappings, swapping, toggle behavior, nested chord suppression, capture gating and recovery, dynamic driving/mouse hints, and steering response after axis swaps. Runtime screenshots with --capture only record user-opened screens and gameplay; they never navigate or open/close menus.

## 0.4.0 release verification — 2026-10-16

The user tested the end-user installation in their game folder and confirmed it works. BepInEx 5.4.23.3 loaded 0.4.0 successfully after HideManagerGameObject was corrected to true. The plugin remained active, detected XInput slot 0, and parsed all 16 sprites. Captures show Sensitivity and both on-foot/driving binding pages, plus capture mode. The log records a binding save, and the configuration on disk contains Driving/use=LSUp. The user confirmed the feature works before requesting packaging. The release plugin is byte-identical to this tested DLL. All 1,420 core checks passed on this source. This evidence does not claim an exhaustive pass across every binding, device, activity, or native Windows.

## 0.5.0 native menu theme

See [UI_STYLE.md](UI_STYLE.md) for the visual checklist and native-resource selection. Build and existing input/binding checks pass. Visual verification is deferred at the user's request to test later; 0.5.0 has not been launched.

## 0.5.1 Save/Load and keyboard removal

The earlier on-screen-keyboard test steps are historical and no longer apply. The overlay and synthetic computer text/music input are removed. Type with a physical keyboard; default new-save names remain generated by the game. See [SAVE_MENU.md](SAVE_MENU.md) for selection, RB/LB focus shortcuts, confirmation scoping, and the later in-game checklist. The 1,432 core checks pass. The user subsequently tested 0.5.1 and confirmed the Save/Load changes work; action-label contrast and exit-dialog navigation required follow-up.

## 0.5.2 action labels and exit confirmation

Check all five uncommon-action pages: text should lie inside the light button face, with a thin black outline and complete labels/hints. Check selected and unselected cards, Close text and B glyph, and compact settings buttons. In the in-game pause menu, open Exit and confirm No receives focus. D-pad/LS should reach Yes and No without leaving the dialog; B should return to the pause menu. Repeat with the virtual pointer. These checks are pending; no automatic test invokes Exit Yes.

## 0.5.3 top-level menu selection

Opening the pause menu or returning from Save/Load, settings, or Exit should select Continue. The title screen prefers Continue when available, otherwise New Game. D-pad/LS and the virtual pointer stay within the active top-level menu. Keyboard/mouse use keeps native navigation; returning to controller use selects the default again. Four new focus-policy checks cover resume, title Continue, unavailable Continue, and stable fallback ordering; 1,436 core checks pass. In-game verification is pending.

## 0.5.4 focus contrast

Check the royal-blue focus outline on the title/pause menu, Save/Load, Exit confirmation, and both custom controller panels. Check that the virtual pointer uses the same blue and remains visible. No navigation behavior changes.

## 0.5.5 settings legibility and focus width

Check the slightly thicker blue borders in native and custom menus. In controller settings, check inset outlined text on Sensitivity and Bindings tabs, every sensitivity row, +/- controls, profile selector, LT/RT, action names, binding glyphs, Clear, and capture/conflict/reset buttons. Verify narrow buttons retain complete labels and action names are not interpreted as controller tokens. All 1,436 core checks pass; visual verification is pending.

## 0.5.5 release verification

The user completed the final interactive Linux/Proton test and approved release packaging after the settings readability and border-width changes. The tested DLL is preserved unchanged in the release archive. Build output records zero warnings/errors and all 1,436 core checks passing. Archive CRCs, file hashes, and equality with the tested installation are checked during packaging. This supersedes the pending notes above for the reported menu fixes; it does not claim every historical checklist item, resolution, device, vehicle, or platform was tested. Native Windows remains unverified.
