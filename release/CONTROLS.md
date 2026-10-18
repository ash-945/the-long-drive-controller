# Controller reference — 0.6.7 beta

The tables below describe default bindings. Gameplay assignments can be changed independently for on-foot/driving in the Bindings page of controller settings; current in-game hints follow your saved bindings.

Xbox names are used in the tables. In-game icons follow the automatic or saved
manual family selection. The equivalent controls are:

| Xbox / table name | PlayStation | Nintendo |
| --- | --- | --- |
| A | Cross | A (right) |
| B | Circle | B (lower) |
| X | Square | X (upper) |
| Y | Triangle | Y (left) |
| LB / RB | L1 / R1 | L / R |
| LT / RT | L2 / R2 | ZL / ZR |
| View / Back | Select / Share | Minus |
| Start | Start / Options | Plus |

Nintendo follows printed letters, rather than Xbox face-button positions.
LS and RS mean the left and right sticks; a stick click means pressing the stick inward. View may be labelled Back or Select on your controller.

## On foot

| Action | Control |
| --- | --- |
| Move / look | LS / RS |
| Sprint | LS click; toggles, stops when movement stops |
| Crouch | B; toggle |
| Jump | A |
| Use / enter / exit | X |
| Pick up / drop | Y |
| Equip / recenter | RS click |
| Mount / detach | RB |
| Primary action | RT |
| Secondary action / move object | LT; RS manipulates the object |
| Rotate held object | Hold LB + RB and move RS |
| Reload / adjust | LB + X |
| Push | LB + A |
| Lean | LB + B |
| Attach with glue | LB + Y |
| Aim / target | LB + RS click |
| Object distance / scroll | LB + D-pad up/down |
| Inventory slots 1 / 2 / 3 / 4 | D-pad up / right / down / left |
| Temporary run modifier for compound actions | Hold LB + LS click |

## Driving

| Action | Control |
| --- | --- |
| Steer / look | LS / RS |
| Throttle / brake | RT / LT |
| Clutch | LS click |
| Handbrake | A; native toggle |
| Ignition | Hold B |
| Shift down / up | LB / RB |
| Lights / horn | D-pad up / down |
| Scroll | D-pad right / left |
| Contextual use / exit | X |
| Select a specific gear | View → Gears |

Native vehicle/transmission rules still apply. Small hints appear while seated and hide with the HUD and menus. Steering time to full lock, Steering stick curve, and the Driving hint box toggle are in General settings.

### Steering rate

Holding full stick approaches full lock gradually. **Steering time to full lock**
sets the center-to-lock time from 0.10 to 3.00 seconds (default 0.75); higher is
slower. Partial-stick input approaches its smaller target, release returns toward
center gradually, and full reversal takes about twice the center-to-lock time.
**Steering stick curve** is the old sensitivity setting (0.25–2.00, default 1.00).
It shapes the target separately; existing saved values remain valid. Both options
save automatically. General defaults restores the default curve and 0.75-second
rate. Pointer steering uses the time-to-lock setting too.

While a mod controller is connected, duplicate native Joy-axis wheel input and
joystick steering bindings are suppressed so they cannot bypass the ramp.
Keyboard/mouse steering bindings remain available. Native joystick steering
returns when the mod controller disconnects; native configuration is preserved.

## Menus and settings

| Action | Control |
| --- | --- |
| Pause / resume | Start |
| Uncommon actions | View / Back |
| Controller settings | View, then Start; or View while native Settings is open |
| Navigate / confirm / back | D-pad or LS / A / B |
| Virtual pointer / click | RS / A |
| Scroll native menus | LT / RT |
| Focus Save / save slots in Save/Load | RB / LB |

In the action menu, LB/RB changes pages. A performs an action; hold A for supported continuous actions. The menu hides while holding and returns on release. X toggles supported holds; Y releases toggled holds. B or View closes. Survival is the first page and includes Defecate, which requires at least 30% bowel meter. Tools, Gears, Camera, and System contain other uncommon actions. Multiplayer and VR actions still require their native contexts.

In controller settings, up/down selects and left/right adjusts. A increases or toggles, Y restores defaults, and B/Start/View closes. Values save automatically. General includes Controller family (Auto, Xbox,
PlayStation, Nintendo) and Driving hint box (On/Off). A manual family choice
persists and overrides detection until Auto is selected again. It changes the
icons without changing SDL input normalization. General defaults resets Auto
and enables driving hints; saved bindings are separate.

## Save/Load, confirmations, and text entry

Save/Load initially selects Save when available, otherwise a visible save slot. RB focuses Save; LB returns to save slots. A activates the selected native control. D-pad/LS can leave the name field; read-only save details are skipped. Save/load/delete and in-game exit confirmations select No initially and keep controller navigation within the dialog. B cancels a confirmation before backing out of the screen.

Opening the pause/title menu or returning from a submenu prefers Continue, or New Game when Continue is unavailable. Blue borders show controller focus.

The on-screen keyboard has been removed. Type custom save names and computer text with a physical keyboard. The game still generates a default new-save name. RS moves the in-game computer cursor; B or Start exits the computer.

After reconnecting or changing contexts, release all controls to neutral before resuming gameplay. Controller prompts return to keyboard/mouse labels on disconnect. Free stick movement and directions use the supplied stick artwork.

## Rebinding

LB/RB switches settings pages. On Bindings, X switches profile; LT/RT changes category. Select with D-pad/LS and press A. Release all controls, press the desired control or combination (up to three), and release to save. Start cancels capture, View closes, RS click clears the selected binding, and Y asks before resetting the current profile. Confirm an occupied-control swap with A or cancel with B. Start/View and menu navigation remain fixed.
