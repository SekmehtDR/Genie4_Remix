<h1 align="center">Genie4_Remix</h1>

  <p align="center">
    Modified version of the public release of <a href="https://github.com/GenieClient/">Genie4</a>.<br>
    This is currently an unofficial release — please do not expect the Genie team to support this version. <strong>USE AT YOUR OWN RISK!</strong>
  </p>

  <p align="center">
    <code>Latest Version: 4.0.3.1</code> / <code>Release: 3/29/2026</code> / <code>Lich Support: Yes!</code> / <code>Stable? YES!</code>
  </p>

  <p align="center">
    <kbd><a href="screenshots/geniemain-cs-31MAR2026.png"><img src="screenshots/geniemain-cs-31MAR2026.png" width="350"/></a></kbd>
    <kbd><a href="screenshots/automapper-cs-31MAR2026.png"><img src="screenshots/automapper-cs-31MAR2026.png" width="350"/></a></kbd>
  </p>


<!-- ABOUT THE PROJECT -->
## About The Project

Genie4_Remix is an unofficial fork of [Genie4](https://github.com/GenieClient/Genie4), the community-developed front-end for Simutronics Corporation's game DragonRealms. This fork adds bug fixes, UI improvements, and quality-of-life features not yet merged upstream — including full dark/light/custom theming, a portable self-contained mode, AutoMapper enhancements, and numerous scripting engine fixes.

<!-- GETTING STARTED -->
## Getting Started

- Download the latest release at
   [https://github.com/SekmehtDR/Genie4_Remix/releases/latest](https://github.com/SekmehtDR/Genie4_Remix/releases/latest)

<!-- CHANGELOG -->
## Changelog

### v4.0.3.1
#### Bug Fixes
- **Merged upstream/Dev-4-0-2-10** — <a href="https://github.com/GenieClient/Genie4/commits/Dev-4-0-2-10/"> Merged upstream/Dev-4-0-2-10</a> branch.
- **Automapper Maps Load without Lich** — Pull request code "Add support for ShowRoomID flag" was added - <a href="https://github.com/GenieClient/Genie4/pull/175"> Genie4/pull/175</a>.
- **Fix sounds not playing for regex highlights** — Pull request code "Add support for ShowRoomIFix sounds not playing for regex highlights" was added - <a href="github.com/GenieClient/Genie4/pull/177"> Genie4/pull/177</a>.
- **Running script context menu restored** — The dropdown arrow and context menu (Resume, Pause, Abort, Debug, Show Trace, Show Vars, Edit) on running script toolbar buttons were no longer visible after the custom `MenuRenderer` was introduced. The base `ToolStripRenderer` does not paint split button backgrounds, so the dropdown zone was invisible. Added `OnRenderSplitButtonBackground` to explicitly render the button area, a theme-adaptive tinted dropdown zone, a divider line, and a filled down-arrow glyph. Works correctly across Dark, Light, and Custom themes.
- **ProfileConnect "Edit Note" crash fixed** (`Forms/DialogProfileConnect.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/178">Genie4/issues/178</a>) — `EditNote_Click` threw a `NullReferenceException` when the profile list was in flat (non-tree) view mode because `_profiles.SelectedNode` was null. Added an early-return guard. Also corrected the dialog owner: `ShowDialog(Parent)` was passing the MDI client as owner, which caused `DialogProfileNote` to be positioned relative to the main window and appear behind it. Changed to `ShowDialog(this)` so the note dialog centers on the profile connect dialog as intended.
- **`$unixtime` now works inside `waiteval`** (`Core/Game.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/179">Genie4/issues/179</a>) — `$unixtime` was a lazy placeholder (`@unixtime@`) substituted only at parse time and never fired a variable-changed event, so `waiteval` conditions containing it would stall forever. Fixed by updating the `unixtime` variable list entry with the current Unix timestamp and firing `VariableChanged("$unixtime")` each time the server sends a time tick — the same mechanism that makes `$gametime` work in `waiteval`.
- **`#mapper allowdupes true/false` now sets state correctly** (`Mapper/MapForm.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/169">Genie4/issues/169</a>, <a href="https://github.com/GenieClient/Genie4/issues/154">Genie4/issues/154</a>) — `SetAllowDuplicatesToggle(bool)` ignored its parameter and always toggled the current state, making `#mapper allowdupes true` and `#mapper allowdupes false` behave identically. Fixed to assign the parameter directly so the command reliably sets the desired state regardless of what it was before.
- **Script `contains()` with multi-condition evaluation fixed** (`Script/Eval.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/145">Genie4/issues/145</a>) — `if (contains("x", "%1") && "%2" == "")` and similar compound expressions evaluated incorrectly. In `ParseSection`'s comparison pass, `iArgLeft` was not reset when a `&&` or `||` operator was encountered, causing the left operand of one sub-expression to "bleed" into the right side of an unrelated comparison across the logical operator boundary. `&&` and `||` now act as group boundaries in the comparison pass, resetting `iArgLeft` so each sub-expression is evaluated independently before being combined.
- **Map files saved as UTF-8 instead of UTF-16** (`Mapper/MapForm.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/166">Genie4/issues/166</a>) — `XmlTextWriter` was constructed with `System.Text.Encoding.Unicode` (UTF-16 LE), producing map XML files with a UTF-16 BOM and two-byte-per-character encoding. External tools, Lich, and cross-platform scripts expect standard UTF-8 XML. Changed to `System.Text.Encoding.UTF8` so saved map files are universally compatible.
- **Script and debug output now captured by Auto Log** (`Forms/FormMain.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/80">Genie4/issues/80</a>, <a href="https://github.com/GenieClient/Genie4/issues/54">Genie4/issues/54</a>) — When Auto Log is enabled, script output (status messages like `[Script loaded:]`, `[Script aborted:]`) and debug lines (e.g. `wait.cmd(2): [start:]`) were silently dropped by the logger because `Script_EventPrintText` only sent text to the game window, never to the log. Added a `LogText` call in `Script_EventPrintText` so all script output — including debug at any level — is captured in the existing `CharacterNameGame_YYYY-MM-DD.log` file alongside game text, with no new config options or separate files needed. Also fixed `[Script debuglevel set to N]` messages (generated directly in FormMain when changing debug level from the running script context menu) which bypassed `Script_EventPrintText` entirely and were also missing from the log. Additionally fixed a formatting issue where script lines were being appended directly after the game prompt (`> [Script loaded:]`) — script text now checks `LastRowWasPrompt` and inserts a newline when needed so each script line starts cleanly on its own line in the log.
- **Highlights Enabled / Substitutes Enabled toggles added to File menu; "Ignores/Gags" renamed to "Gags"** (`Lists/Config.cs`, `Core/Game.cs`, `Forms/Components/ComponentRichTextBox.cs`, `Forms/FormMain.cs`, `Forms/FormMain.Designer.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/125">Genie4/issues/125</a>) — Added `bHighlightsEnabled` and `bSubstitutesEnabled` flags to `Config`. File menu now has **Highlights Enabled** and **Substitutes Enabled** checkboxes below Gags Enabled and above Triggers Enabled. When Highlights Enabled is unchecked, all highlight processing (line-color matching in `PrintTextWithParse` and full `ParseHighlights` pass in `ComponentRichTextBox`) is bypassed. When Substitutes Enabled is unchecked, `ParseSubstitutions` is skipped for all game text including bold/creature buffers. "Ignores/Gags Enabled" renamed to "Gags Enabled" to match the tab label.
- **External editor and file browser launches no longer block the UI** (`Core/Command.cs`, `Forms/FormMain.cs`, `Forms/ScriptExplorer.cs`) — Replaced all `Interaction.Shell()` calls (25 locations) with `Process.Start()` across script editing, config file editing (`#edit` commands for aliases, triggers, highlights, gags, macros, substitutes, presets, names, variables, settings), log file opening, and all Open Directory menu items. `Interaction.Shell()` is a VB6-era method that transfers focus synchronously and can cause brief UI freezes. `Process.Start()` launches the external process non-blocking with no focus interference.
- **Reduce UI stutter when connecting** (`Core/Game.cs`, credit: <a href="https://github.com/digitalnyc1">digitalnyc1</a> via <a href="https://github.com/digitalnyc1/Genie4x/commit/5c069d6">Genie4x/5c069d6</a>) — Three synchronous socket calls (`Connect` and `ConnectAndAuthenticate`) were blocking the main thread during connection, causing the UI to freeze and stutter. Wrapped all three in `Task.Run()` so they execute on a background thread, keeping the UI responsive while connecting.
- **Linebreak added after pasted images** (`Forms/Components/ComponentRichTextBox.cs`, credit: <a href="https://github.com/digitalnyc1">digitalnyc1</a> via <a href="https://github.com/digitalnyc1/Genie4x/commit/5a6af34">Genie4x/5a6af34</a>) — After pasting an image into the RichTextBox, no newline was inserted so subsequent text would appear on the same line as the image. Added `Select(TextLength, 0)` and `SelectedText = Environment.NewLine` after the paste so images are always followed by a clean line break.
- **Timestamps no longer break regex anchors** (`Forms/Components/ComponentRichTextBox.cs`, fixes <a href="https://github.com/GenieClient/Genie4/issues/168">Genie4/issues/168</a>) — When timestamps were enabled, the `[HH:MM AM/PM] ` prefix prepended to each line caused `^`-anchored regex highlight patterns (e.g. `^You`) to never match because the line started with `[` rather than the game text. Fixed by building a timestamp-stripped copy of the buffer text and a position map before regex matching. The regex runs against the stripped text so anchors work as expected, and match indices are translated back to their original positions in the buffer for correct highlight coloring.
- **Mana/Inner Fire bar decoupled from Magic Panels toggle; guild-aware bar layout** (`Forms/FormMain.cs`, `Forms/FormMain.Designer.cs`) — The Mana bar (displayed as "Inner Fire" for Barbarians) was previously hidden whenever the Magic Panels layout option was turned off, because `SetMagicPanels()` blindly hid all magic-related UI including `ComponentBarsMana`. Fixed by separating bar visibility from panel visibility: the castbar and spell labels still respond to the Magic Panels toggle, but `ComponentBarsMana` is now controlled independently via `UpdateManaBarVisibility()`. Additionally, guilds that have no use for a mana bar (Thief, Commoner) automatically hide the bar and collapse `TableLayoutPanelBars` to 4 columns; all other guilds show all 5 bars. Visibility updates whenever the server sends the `$guild` variable, so it reacts correctly on login and character switches. Also fixed bar column assignments so each bar lands in its correct cell: Health→0, Mana/Inner Fire→1, Concentration→2, Stamina→3, Spirit→4.

#### Portable / Self-Contained Mode
- **Fully portable** — Genie now always stores all user data (Config, Scripts, Maps, Logs, Sounds, Plugins, Icons, Help) in the application's own directory instead of `%appdata%`. The entire client can be copied to any folder, moved to a thumb drive, or run from a network share with no reconfiguration.
- **Auto-bootstrap on first run** — If the local `Config` folder does not exist when Genie starts, all required directories are created automatically next to the exe. No installer step required.
- **Migration path** — Existing users can copy the contents of `%appdata%\Genie4\` into the Genie application folder once to carry over all settings, highlights, aliases, maps, and scripts.

### v4.0.3.0
#### UI & Theme
- **Dark/Light/Custom theme system** — OS-level dark mode via `uxtheme.dll` and `dwmapi.dll` APIs. Menus, scrollbars, title bars, dropdowns, and all native controls respond to the active theme. Toggle via **Layout → Color Themes** with checkmarks indicating the active selection.
- **Flat dark title bars** — Replaced bitmap-tiled window skins with a flat charcoal title bar (`#28282A`) and 1px accent line on all MDI child windows. Cleaner appearance with improved paint performance.
- **Dark scrollbars** — Applied `SetWindowTheme("DarkMode_Explorer")` to all rich text output windows.
- **Menu renderer** — Full custom `ToolStripRenderer` for menus and context menus. Flat, no gradients, theme-aware hover and checked states.
- **MDI background** — Updated to near-black (`#141416`) in dark mode.
- **Status strip** — Flat style, grip hidden, themed to match active color mode.
- **Plugins menu** — Removed stray blank separator that appeared when no plugins were loaded.
- **AutoMapper theme integration** — The AutoMapper window now fully responds to Dark/Light/Custom theme switches. The map panel background, toolbar, status bar, node colors, exit lines, path highlight, and current position indicator all update when the theme changes. Custom mode restores whatever colors were set in `presets.cfg` at startup.

#### Bug Fixes
- **Window snap logic** — Corrected bitwise `&` operators to logical `&&` in FormSkin drag/snap conditions. Previously the snap guard (`bSnappedX/Y == false`) was not short-circuiting correctly, allowing double-snap in edge cases.
- **AutoMapper GDI leaks** — All `Pen`, `SolidBrush`, and `Font` objects created in the map paint loop are now properly disposed via `using`. On large maps this eliminates hundreds of leaked GDI handles per second that could cause rendering corruption or exhaustion on long sessions.
- **ComponentBars pen leak** — Border pens are now disposed before replacement when `BorderColor` is set, preventing accumulation of leaked GDI objects over the session.
- **MenuRenderer check font** — `Font` and `StringFormat` for the checkmark glyph promoted to `static readonly` fields; previously recreated on every checked-item paint call.

#### Performance
- **Regex compilation** — Frequently-used highlight and name patterns now use `RegexOptions.Compiled`.
- **StringBuilder** — Replaced `string +=` concatenation in `RebuildStringIndex`, `RebuildLineIndex`, and `RebuildIndex` to eliminate O(n²) allocations.
- **Highlight parsing** — Buffer text and line split are now cached once per parse pass instead of re-computed per highlight entry.
- **Substitution scanning** — Removed redundant `.Match()` before `.Replace()`; uses `ReferenceEquals` to detect no-match.
- **AutoMapper regex cache** — `IsExitSet()` now caches compiled exit regexes in a `Dictionary<string, Regex>` instead of recompiling on every call.
- **Non-blocking UI text dispatch** — Switched `AddText` from synchronous `Control.Invoke` to `Control.BeginInvoke`. The network thread no longer blocks waiting for each line to render before parsing the next, eliminating the stutter and jerkiness visible during bursts of incoming game text (movement, combat, room descriptions).

#### AutoMapper
- **Description matching robustness** — Room descriptions are now normalized before comparison (whitespace collapsed, case-insensitive). Minor game text updates no longer cause the mapper to lose position or create duplicate rooms.
- **Movement queue timeout visibility** — When the movement queue is cleared due to timeout, a message is always shown in the game window so players know the mapper may need a resync. Previously this was silent unless debug mode was enabled.
- **File handle leak fixed** — `StreamReader` instances in `RoomOnDisk` and `EchoRoomsOnDisk` are now properly disposed after use, preventing file handle exhaustion when scanning large map directories.
- **`#mapper save` path bug fixed** — Saving a map by filename (e.g. `#mapper save mymap`) now correctly constructs the path. A typo (`==` instead of `\`) was silently producing invalid file paths and losing saves.
- **Thread safety** — Map UI updates (`UpdateGraph`, `UpdateMap`, `SetNodeList`, `SetDestinationNode`) now marshal to the UI thread via `BeginInvoke`. Previously these were called directly from the game network thread, racing with the paint event and causing phantom position jumps, rendering glitches, and occasional crashes.
- **`UpdateCurrentRoom` refactor** — The 640-line monolithic room-tracking method was split into 13 focused helper methods (`BuildCurrentNode`, `DequeueMove`, `LocateViaLinkedArc`, `LocateViaBlankMove`, `LocateViaUnlinkedDirection`, etc.). No logic changes — purely structural, making future bug fixes and improvements tractable.
- **Fuzzy description matching** — When normalized exact match fails, a secondary fuzzy pass strips volatile segments (NPC presence lines, "also here:", "obvious exits:", article/number prefixes) before comparing. Catches cases where Simutronics adds or removes dynamic text without changing the room itself.
- **Map directory index cache** — `#mapper find` and auto-load no longer open every `.xml` file on disk on each invocation. A lightweight in-memory index is built once on first use and invalidated when a map is loaded, making room searches near-instant on large map collections.
- **AutoMapper toolbar themed** — The mapper's toolbar and status bar now use the same flat `MenuRenderer` as the rest of Genie. Background, foreground, hover, and separator colors all match the active theme.
- **Path visualization** — When a route is active, path nodes are highlighted with a thick colored border (preserving the node's original fill color, so cyan homes and other custom colors remain visible). The connecting exit lines along the route are also drawn in the path color at increased width, making the full route easy to follow at a glance.
- **Current position indicator** — The "you are here" node is rendered with a thick colored border and an X glyph drawn in a darkened variant of the `automapper.here` color, giving strong contrast against the node fill at any zoom level. Color is amber in Dark mode, deep red in Light mode, and fully customizable via `presets.cfg`.
- **`automapper.here` preset** — New preset key controls the current-room indicator color independently of the path color. Defaults to `Maroon` so it works in Custom/default themes without any config change.
- **MDI control-box icons** — Minimize/restore/close icons injected into the MenuStrip when an MDI child is maximized are now inverted in dark mode so they appear white instead of black.

#### Status Bar
- **Spell prep bar layout fixed** — The castbar (`ComponentRoundtime`, magenta) was free-floating at a hardcoded pixel position (`x=1172`) outside the status bar's table layout, causing it to appear far to the right at all window sizes. It is now a proper column in `TableLayoutPanelFlow`, stretching correctly at any resolution.
- **Magic panels toggle fixed** — Toggling magic panels off now correctly collapses the table to 5 columns (removing spell name and castbar). The column count was off by one after the castbar was moved into the table.
- **Status bar MaximumSize removed** — `TableLayoutPanelFlow` had a hardcoded `MaximumSize` of 1167px, preventing it from filling the full window width. Removed so the bar stretches edge to edge.

#### Startup
- **No startup flicker** — Main window is hidden (`Opacity = 0`) from construction until presets are loaded and the theme is fully applied, then appears in one clean frame. Eliminates the brief flash of unstyled or partially-painted windows on launch.
- **Deferred paint pump** — Removed the premature `Application.DoEvents()` call that was forcing a repaint of MDI child windows before their content had finished drawing.

#### Shutdown & Connectivity
- **Clean exit via X button** — Clicking X while connected now prompts the user and, on confirmation, sends `quit` through the active connection (including Lich proxy) before closing. This gives Lich and all scripts the same clean shutdown signal as typing `quit` in game.
- **Plugin shutdown ordering** — Plugins receive `ParentClosing()` only when the user confirms close, not on every X click. Prevents plugins (e.g. SpellTimer) from saving/exiting prematurely when the user cancels.
- **Connection lost handling** — Added `EventGameDisconnected` on `Game` fired from both clean disconnect and connection-lost paths, ensuring Genie exits correctly even when Lich closes the socket before the `<exit/>` tag is parsed.
