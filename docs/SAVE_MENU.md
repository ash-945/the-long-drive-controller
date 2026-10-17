# Save/Load controller navigation — 0.5.1

The on-screen keyboard and computer text/music injection are removed. Native text fields and the computer use physical keyboard input. The RS computer-cursor adapter preserves native keyboard arrow input. B or Start exits the computer and releases its music notes without an overlay.

On controller entry to Save/Load, focus prefers the active Save button. If saving is unavailable (for example, on the title screen), it prefers a visible save slot, then autosaves/Load Last and other actionable controls. Read-only fields and the native save-information/screenshot-path fields are excluded. A focused name field can be left with D-pad/LS instead of capturing navigation in its text caret.

RB focuses Save; LB returns to the first visible normal save slot. These shortcuts select only: neither writes, overwrites, loads, or deletes a save. A activates the native UI control. A small RB-focus caption is added to the native Save label only while the controller owns navigation; it is restored on handoff/close. Existing native filenames and confirmation behavior are unchanged.

Save/load/delete confirmation dialogs restrict controller navigation to their own controls and prefer their No button by native event identity. B cancels the dialog first, finishes name-field editing if focused, or backs out of Save/Load. The virtual pointer respects the same save scope. A connected idle controller no longer takes keyboard/mouse navigation ownership back until a controller input is used.

## Verification

Build and 1,432 core checks pass, including initial Save priority, load-only fallback, hidden controls, geometric movement from the name field to Save and between save slots, and safe confirmation focus/scope. The user tested 0.5.1 and reported that the Save/Load changes work. No test writes or deletes a game save.

Later, open Save/Load during gameplay. Confirm Save is selected and no keyboard overlay appears. Use LB to visit slots and RB to return to Save. Use D-pad/LS between the list, name, and actions; try the pointer too. Open an overwrite confirmation and cancel it with B; no file should change. Use a disposable new save if testing actual Save. Check title-screen Load, keyboard typing, mouse handoff, and a disconnected controller separately.

## Exit confirmation — 0.5.2

The in-game exit confirmation uses the same scoped controller navigation: only its own controls are reachable by D-pad/LS or the virtual pointer, No receives initial focus, and B invokes the native cancel action. Opening or closing a dialog resets pointer interaction before focusing the new scope. A still invokes the selected native button. The title-screen immediate-exit behavior remains the game's own behavior.

In-game verification is pending: open Exit from the pause menu, check that No is selected, move between Yes and No, and cancel with B. Check pointer navigation too. No automated check invokes Yes or quits the game.
