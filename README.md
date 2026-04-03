<div id="top"></div>
<!--
*** Genie Client is a Community focused development of Conny's Open Source Version of GenieClient.
*** we want to take a moment and thank Conny for his hard work on GenieClient over the years and 
*** for allowing the community to take a part in the future development of the Client.
*** 
*** Thanks again! Now team go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** This Readme is using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![GPU License][license-shield]][license-url]




<!-- PROJECT LOGO -->
<br />
<div align="center">


<h1 align="center">Genie 4</h1>

  <p align="center">
    Genie is an alternative front-end for use with the Simutronics Corporation’s game DragonRealms.
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#features">Features</a></li>
    <li><a href="#changelog">Changelog</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>


### Built With

* [C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
* [Visual Basic](https://docs.microsoft.com/en-us/dotnet/visual-basic/)


<p align="right">(<a href="#top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started
1. Download the Latest Release at
	[https://github.com/GenieClient/Genie4/releases/latest] 	

2. Upgrade and First Time Installation Instructions can be found at
 	[https://github.com/GenieClient/Genie4/wiki]

3. Get Maps Updates from the Team at
	[https://github.com/GenieClient/Maps]


<p align="right">(<a href="#top">back to top</a>)</p>


<!-- CHANGELOG -->
## Changelog

### v4.0.3.1
#### Bug Fixes
- **Automapper Maps Load without Lich** — Pull request code "Add support for ShowRoomID flag" was added - <a href="https://github.com/GenieClient/Genie4/pull/175"> Genie4/pull/175</a>.
- **Fix sounds not playing for regex highlights** — Pull request code "Add support for ShowRoomIFix sounds not playing for regex highlights" was added - <a href="github.com/GenieClient/Genie4/pull/177"> Genie4/pull/177</a>.
- **Running script context menu restored** — The dropdown arrow and context menu (Resume, Pause, Abort, Debug, Show Trace, Show Vars, Edit) on running script toolbar buttons were no longer visible after the custom `MenuRenderer` was introduced. The base `ToolStripRenderer` does not paint split button backgrounds, so the dropdown zone was invisible. Added `OnRenderSplitButtonBackground` to explicitly render the button area, a theme-adaptive tinted dropdown zone, a divider line, and a filled down-arrow glyph. Works correctly across Dark, Light, and Custom themes.

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

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- ROADMAP -->
## Roadmap

- [x] .NET 6 Upgrade
- [ ] Refactor Core Logic away from GUI
- [ ] Convert GUI to Cross-Platform
- [ ] Upgrade Plugin Interface
- [ ] Get Latest Version (OneButton) <AInstallLogo>
    <img src="https://cdn.advancedinstaller.com/svg/pressinfo/AiLogoColor.svg" width="70" height="40"></AInstallLogo>


See the [open issues](https://github.com/GenieClient/Genie4/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make is **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the GPL 3.0 License. See `LICENSE` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTACT -->
## Contact


Project Link: [https://github.com/GenieClient/Genie4](https://github.com/GenieClient/Genie4)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [https://github.com/walcon] Conny - Origional Developer)


<p align="right">(<a href="#top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/GenieClient/Genie4.svg?style=for-the-badge
[contributors-url]: https://github.com/GenieClient/Genie4/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/GenieClient/Genie4.svg?style=for-the-badge
[forks-url]: https://github.com/GenieClient/Genie4/network/members
[stars-shield]: https://img.shields.io/github/stars/GenieClient/Genie4.svg?style=for-the-badge
[stars-url]: https://github.com/GenieClient/Genie4/stargazers
[issues-shield]: https://img.shields.io/github/issues/GenieClient/Genie4.svg?style=for-the-badge
[issues-url]: https://github.com/GenieClient/Genie4/issues
[license-shield]: https://img.shields.io/github/license/GenieClient/Genie4.svg?style=for-the-badge
[license-url]: https://github.com/GenieClient/Genie4/blob/master/LICENSE.txt
[product-screenshot]: images/screenshot.png



