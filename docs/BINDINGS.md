# Controller binding design

`ControllerBindings` owns separate on-foot and driving profiles. Each of the 68 named actions has a `ControlChord`: zero (unbound), one, two, or three XInput controls. Start/View are not bindable, preserving menu recovery. Analog directions and triggers retain their values. Exact conflicts swap only after UI confirmation; a longer active chord suppresses its constituent bindings, independent of action ordering. Defaults reproduce the original mapping.

`ActionState` consumes this profile once per frame. Sprint/crouch toggle edges track resolved action input rather than physical button names. Steering sensitivity operates on resolved steering actions so remapping axes cannot change look sensitivity. Pointer-driving consumes resolved look actions.

Both gameplay and `ActionCatalog.Hint` read the same profile. `BindingHints` formats native mouse-axis help and the seated driving panel; the panel measures wrapped rows so combinations are not truncated. Profile revisions force native help/Controls refresh. Menu navigation instructions continue to describe fixed UI controls.

The BepInEx configuration stores versioned profile strings under `[Bindings] OnFoot` and `Driving`. Parsing validates the whole candidate before replacing the live profile; malformed values leave defaults/current state intact. Assignment, clearing, and reset save configuration, release latched actions, and rearm gameplay after neutral.

`BindingCapture` waits for neutral before listening and requires neutral after a candidate to save, allowing button combinations without capturing the opening A press. It supports up to one stick direction in a chord and rejects duplicates/oversized combinations. Start or View cancels through the settings/recovery controls. The settings UI checks capture before page/navigation input, so B/X/Y/LB/RB/triggers can all be assigned.
