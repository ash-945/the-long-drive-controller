# Menu styling — 0.5.1 development preview

The uncommon-actions window and controller settings (Sensitivity and Bindings) share the same menu theme. Selection uses green text and a thin gold outline on a pale textured button. Headings use dark ink with a small light offset, and each window provides a clickable Back button alongside controller navigation.

`GameMenuSkin` samples the loaded game's native button, panel, frame sprites, and available font. Sampling is delayed until the native menu exists. GPU readback crops atlas sprites even when the source textures are not CPU-readable; render targets are restored and released in a finally block. Extracted textures are kept in memory, cached, and destroyed on plugin teardown. No game artwork is added to the repository or release download.

If native UI resources cannot be sampled, a small generated paper/button texture keeps the menus usable. `PanelStyle` rebuilds its cached GUI styles when native assets become available. Glyph labels retain button sprites and sufficient line height in compact tabs. Ordinary button labels remain text. The driving HUD keeps white text on its existing dark background for contrast; menu colors do not leak into it.

## Verification

Build succeeds without warnings and all 1,432 existing input/binding checks pass. The 0.5.1 DLL is installed in the local test copy. The user elected to test later, so rendering of native textures, font selection, and the final visual match remain unverified. No 0.5.1 game launch was performed.

For visual review, open View for Uncommon actions, Start for settings, and both settings tabs with LB/RB. Check dark text, native panel/button textures, legible green/gold selection, button glyphs, all eight settings and binding rows, capture/confirmation states, and mouse Back buttons. Confirm the small driving hints remain readable. Review at 1280×720, the preferred display resolution, and a wide aspect ratio. A capture-enabled launch only records user-opened windows; it never navigates or closes them.

The on-screen keyboard has been removed. Save-name and computer typing use the physical keyboard. See [SAVE_MENU.md](SAVE_MENU.md) for the save navigation changes.

The startup log should identify sampled native button/panel/frame names, or report the paper fallback. If a fallback appears, include that log line with the screenshot so the resource selection can be adjusted.
