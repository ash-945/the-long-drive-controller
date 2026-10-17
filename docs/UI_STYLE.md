# Menu styling — 0.5.4 development preview

The uncommon-actions window and controller settings (Sensitivity and Bindings) share the same menu theme. Selection uses green text and a royal-blue outline on a pale textured button. Headings use dark ink with a small light offset, and each window provides a clickable Close button alongside controller navigation.

`GameMenuSkin` samples the loaded game's native button, panel, frame sprites, and available font. Sampling is delayed until the native menu exists. GPU readback crops atlas sprites even when the source textures are not CPU-readable; render targets are restored and released in a finally block. Extracted textures are kept in memory, cached, and destroyed on plugin teardown. No game artwork is added to the repository or release download.

If native UI resources cannot be sampled, a small generated paper/button texture keeps the menus usable. `PanelStyle` rebuilds its cached GUI styles when native assets become available. Glyph labels retain button sprites and sufficient line height in compact tabs. Ordinary button labels remain text. The driving HUD keeps white text on its existing dark background for contrast; menu colors do not leak into it.

Action-card labels use smaller, inset text centered within the lighter button face. Pale lettering (light green when selected) has a one-pixel black outline; controller sprites retain their original colors. Long labels reduce their font size to fit. Compact settings buttons retain their existing spacing. The Close label avoids the controller token alias for Back.

## Verification

The user tested 0.5.1 and confirmed it works, then reported action labels crossing the dark bevel and exit-dialog navigation needing the same treatment as Save/Load. The runtime log confirms native newmenubutton2, ingamemenubg01, and ingamemenubg01keret sampling. Build and 1,432 core checks pass for 0.5.2; the new label placement/outline and exit confirmation still need in-game verification.

For visual review, open View for Uncommon actions, Start for settings, and both settings tabs with LB/RB. Check outlined action labels inside the light button face, native panel/button textures, legible green/blue selection, button glyphs, all eight settings and binding rows, capture/confirmation states, and mouse Close buttons. Confirm the small driving hints remain readable. Review at 1280×720, the preferred display resolution, and a wide aspect ratio. A capture-enabled launch only records user-opened windows; it never navigates or closes them.

The on-screen keyboard has been removed. Save-name and computer typing use the physical keyboard. See [SAVE_MENU.md](SAVE_MENU.md) for the save navigation changes.

The startup log should identify sampled native button/panel/frame names, or report the paper fallback. If a fallback appears, include that log line with the screenshot so the resource selection can be adjusted.

In 0.5.4 the native menu focus outline, custom panel focus outline, and virtual pointer share an opaque royal-blue color to contrast with tan menu backgrounds. Input/navigation behavior is unchanged. The build passes 1,436 core checks; visual confirmation of the new color is pending.
