# Menu styling — 0.5.5 release

The uncommon-actions window and controller settings (Sensitivity and Bindings) share the same menu theme. Selection uses green text and a royal-blue outline on a pale textured button. Headings use dark ink with a small light offset, and each window provides a clickable Close button alongside controller navigation.

`GameMenuSkin` samples the loaded game's native button, panel, frame sprites, and available font. Sampling is delayed until the native menu exists. GPU readback crops atlas sprites even when the source textures are not CPU-readable; render targets are restored and released in a finally block. Extracted textures are kept in memory, cached, and destroyed on plugin teardown. No game artwork is added to the repository or release download.

If native UI resources cannot be sampled, a small generated paper/button texture keeps the menus usable. `PanelStyle` rebuilds its cached GUI styles when native assets become available. Glyph labels retain button sprites and sufficient line height in compact tabs. Ordinary button labels remain text. The driving HUD keeps white text on its existing dark background for contrast; menu colors do not leak into it.

Action-card labels use smaller, inset text centered within the lighter button face. Pale lettering (light green when selected) has a one-pixel black outline; controller sprites retain their original colors. Long labels reduce their font size to fit. Settings buttons use 16-point inset lettering with the same thin black outline and pale/selected-green colors. Compact buttons keep enough horizontal and vertical room for Close, Clear, trigger glyphs, and +/- controls. Ordinary setting/action names remain plain text and are never parsed as controller aliases. The Close label avoids the controller token alias for Back.

## Verification

The final 0.5.5 build passes all 1,436 core checks with no build warnings or errors. The user completed the final interactive Linux/Proton test and approved release packaging. Startup logs confirm 0.5.5, XInput slot 0, all 16 controller sprites, and native menu sampling. This is not an exhaustive resolution/device/platform test; native Windows remains unverified.

For visual review, open View for Uncommon actions, Start for settings, and both settings tabs with LB/RB. Check outlined action labels inside the light button face, native panel/button textures, legible green/blue selection, button glyphs, all eight settings and binding rows, capture/confirmation states, and mouse Close buttons. Confirm the small driving hints remain readable. Review at 1280×720, the preferred display resolution, and a wide aspect ratio. A capture-enabled launch only records user-opened windows; it never navigates or closes them.

The on-screen keyboard has been removed. Save-name and computer typing use the physical keyboard. See [SAVE_MENU.md](SAVE_MENU.md) for the save navigation changes.

The startup log should identify sampled native button/panel/frame names, or report the paper fallback. If a fallback appears, include that log line with the screenshot so the resource selection can be adjusted.

In 0.5.4 the native menu focus outline, custom panel focus outline, and virtual pointer share an opaque royal-blue color to contrast with tan menu backgrounds. Input/navigation behavior is unchanged. The final color was approved in the interactive test.

In 0.5.5 focus borders increase by one pixel: native-menu outlines are four pixels, custom-panel outlines three reference pixels. The pointer size is unchanged. Settings tabs, sensitivity rows, binding names/values, and dialog buttons now share the outlined lettering treatment. The final styling was approved in the interactive test.

In 0.6.5 the General tab fits ten settings, including the saved controller-family
override and driving hint toggle. Uncommon-action cards center each wrapped or
explicit caption line as a group of text and controller icons, within symmetric
horizontal insets. The existing light button face, outlined lettering, and blue
focus border remain in use.

The user tested and approved the 0.6.5 options and centered cards under Proton.
The captured General page shows all ten settings without overlapping the footer;
the captured Survival action cards show centered single- and two-line captions,
including the longer Defecate instruction.

In 0.6.6 the uncommon-actions and controller-settings buttons tint their native
texture to 55% brightness. Selected buttons retain a subtle warm tint. The pale
outlined lettering, original icon colors, and royal-blue focus border are drawn
at full brightness after the background tint is restored. All button states use
the darker background, including tabs, binding rows, and small action buttons.
