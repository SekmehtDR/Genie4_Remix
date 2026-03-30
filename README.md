# Genie4_Remix

Modified version of the public release of Genie4 (https://github.com/GenieClient/). <br>This is currently an unofficial release, please do not expect the Genie team to support this version. USE AT YOUR OWN RISK!</br>
<br>`Latest Version: 4.0.3.0` / `Release: 3/29/2026` / `Lich Support: Yes!` / `Stable? YES!`</br>

<kbd><a href="screenshots/sekmeht-main.png"><img src="screenshots/sekmeht-main.png" width="350"/></a></kbd>
- Download the latest <a href="Genie-4030-b1.zip">ZIP</a> file.
- Extract this folder somewhere on your PC, open the folder and double-click Genie.exe.
- In this version of Genie, click File > Open Directory
- Copy over your `Config`, `Icons`, `Logs`, `Maps`, `Plugins` and `Scripts` folder from your working version of Genie.
- Close and relaunch this new version of Genie.

Additionally:
- Ignore any prompts stating `An Update is Available`. This is prompting you to update BACK to the public released version of Genie!
- If you find yourself updating maps frequently, don't forget to copy over your lamp.exe to your new Genie folder!

<!-- CHANGELOG -->
## Changelog

### v4.0.3.0
#### UI & Theme
- **Dark/Light/Custom theme system** — OS-level dark mode via `uxtheme.dll` and `dwmapi.dll` APIs. Menus, scrollbars, title bars, dropdowns, and all native controls respond to the active theme. Toggle via **Layout → Color Themes** with checkmarks indicating the active selection.
- **Flat dark title bars** — Replaced bitmap-tiled window skins with a flat charcoal title bar (`#28282A`) and 1px accent line on all MDI child windows. Cleaner appearance with improved paint performance.
- **Dark scrollbars** — Applied `SetWindowTheme("DarkMode_Explorer")` to all rich text output windows.
- **Menu renderer** — Full custom `ToolStripRenderer` for menus and context menus. Flat, no gradients, theme-aware hover and checked states.
- **MDI background** — Updated to near-black (`#141416`) in dark mode.
- **Status strip** — Flat style, grip hidden, themed to match active color mode.
- **Plugins menu** — Removed stray blank separator that appeared when no plugins were loaded.

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

#### Shutdown & Connectivity
- **Clean exit via X button** — Clicking X while connected now prompts the user and, on confirmation, sends `quit` through the active connection (including Lich proxy) before closing. This gives Lich and all scripts the same clean shutdown signal as typing `quit` in game.
- **Plugin shutdown ordering** — Plugins receive `ParentClosing()` only when the user confirms close, not on every X click. Prevents plugins (e.g. SpellTimer) from saving/exiting prematurely when the user cancels.
- **Connection lost handling** — Added `EventGameDisconnected` on `Game` fired from both clean disconnect and connection-lost paths, ensuring Genie exits correctly even when Lich closes the socket before the `<exit/>` tag is parsed.
