<h1 align="center">Genie4_Remix</h1>

  <p align="center">
    Modified version of the public release of <a href="https://github.com/GenieClient/">Genie4</a>.<br>
    This is currently an unofficial release — please do not expect the Genie team to support this version. <strong>USE AT YOUR OWN RISK!</strong>
  </p>

  <p align="center">
    <code>Latest Version: 4.0.3.2</code> / <code>Release: 4/10/2026</code> / <code>Lich Support: Yes!</code> / <code>Stable? YES!</code>
  </p>

  <p align="center">
    <kbd><a href="screenshots/geniemain-cs-31MAR2026.png"><img src="screenshots/geniemain-cs-31MAR2026.png" width="350"/></a></kbd>
    <kbd><a href="screenshots/automapper-cs-31MAR2026.png"><img src="screenshots/automapper-cs-31MAR2026.png" width="350"/></a></kbd>
  </p>


## About

Genie4_Remix is an unofficial fork of [Genie4](https://github.com/GenieClient/Genie4), the long-running community front-end for DragonRealms.

The original Genie4 has seen limited active development in recent years, leaving a number of bugs unaddressed and quality-of-life improvements unrealized. This fork was created to fill that gap — bringing in fixes for issues that affect players every day, improving how reliably the client handles Lich, and modernizing the look and feel without changing how it fundamentally works.

If you are already using Genie4, Genie4_Remix is a drop-in replacement. Your existing settings, scripts, maps, and highlights carry over. The goal is simply a client that works better and gets out of your way.

## Getting Started

- Download the latest release at
   [https://github.com/SekmehtDR/Genie4_Remix/releases/latest](https://github.com/SekmehtDR/Genie4_Remix/releases/latest)

---

## What's Different from Genie4

### Lich Support

- **Easy Lich setup built into the client** — Instead of having to type `#config` commands to point Genie at Ruby and Lich, there is now a dedicated "Lich" tab in the Configuration window with labeled fields, Browse buttons, and a "Test Paths" button that tells you right away if something is misconfigured.

- **Per-character Lich preference** — The Connect dialog now has a "Connect via Lich" checkbox. Check it once for a character and it remembers. No more typing `#lc CharacterName` every time you log in.

- **Lich mode no longer sticks between sessions** — Previously, if you connected via Lich and then disconnected, the next login attempt would silently try to route through Lich again — even if Lich wasn't running — causing a hung or failed connection. This is fixed; each session starts fresh.

- **Lich works even without a saved password** — If your profile didn't have a saved password and Genie asked you to type it in, it would connect without Lich regardless of your preference. Fixed.

---

### Spellcasting

- **The cast RT bar now shows "Ready" when your spell is prepared** — Once the cast roundtime counts down to zero, the bar stays lit and displays "Ready" so you always know at a glance that your spell is still prepared and waiting. The bar clears normally when you cast, dismiss, or lose the spell.

---

### Performance & Responsiveness

- **Dramatically reduced lag during busy game moments** — Incoming game text, trigger processing, and script evaluation no longer compete with each other on the same thread. In heavy combat or crowded rooms where many lines arrive at once, the client stays responsive instead of freezing or falling behind.

- **Faster login and connection** — The connection handshake no longer briefly freezes the main window while it's working in the background.

- **Scripts no longer cause the client to stall** — A subtle bug where active scripts could block incoming game data for up to 10–15 seconds per line has been fixed. If you ever noticed severe lag in text appearing even though your connection was fine, this was likely the cause.

---

### Themes & Appearance

- **Full dark, light, and custom theme support** — The entire client — menus, scrollbars, title bars, status bars, and all windows — responds to your chosen color theme. Switch anytime via **Layout → Color Themes**.

- **No more flash of bright white on startup** — The client now appears fully styled from the moment it opens instead of briefly flickering with a white or unstyled window.

- **Status bar and spell bar layout fixed** — The cast RT bar was floating off-screen to the right at larger resolutions and has been moved into the proper layout. All bars now scale correctly with the window size.

- **Mana bar handled correctly for all guilds** — Barbarians see "Inner Fire" correctly. Thieves and Commoners, who have no use for a mana bar, have it hidden automatically and the remaining bars shift over to fill the space.

---

### AutoMapper

- **The AutoMapper window fully matches your chosen theme** — Map background, toolbar, node colors, and position indicator all update when you switch between Dark, Light, and Custom themes.

- **Current position and active route are easy to see** — Your current room is marked with a clear colored indicator. When a route is active, the path through the map is highlighted so you can follow it at a glance.

- **Room tracking is more reliable** — The mapper is better at holding your position through minor game text changes (NPC arrivals, dynamic room descriptions) without losing sync or creating duplicate rooms.

- **Map searches are much faster** — `#mapper find` and auto-load no longer read every map file on disk each time. Results are near-instant even with large map collections.

- **Maps save as standard UTF-8** — Map files are now saved in a format that external tools, Lich scripts, and other applications can read without issues.

---

### Bug Fixes

- **Highlights with "ignore case" now actually ignore case** — The case-insensitive option on individual highlight entries was silently ignored. It works correctly now.

- **Timestamps no longer break regex-style highlights** — If you had highlights that used `^` to match the start of a line, they stopped working when timestamps were enabled. Fixed.

- **Highlights and Substitutes can be toggled on/off from the File menu** — New checkboxes let you quickly disable all highlights or substitutes without deleting them, useful for troubleshooting or clean screenshots.

- **Script and debug output now appears in the Auto Log** — If you use Auto Log to keep a session record, script activity (loaded, aborted, debug messages) was silently missing from the file. It's included now.

- **Config panel icons restored** — All the small toolbar icons inside configuration panels (Refresh, Add, Remove, Save, etc.) were blank due to a conversion issue. They display correctly now.

- **Running script menu restored** — The right-click menu on running script buttons (Resume, Pause, Abort, Debug, etc.) was invisible after a theming update. It's visible and functional again.

- **Menu separators are visible again** — Thin divider lines inside menus were being crushed to zero height by the theme renderer. Fixed.

- **External editors and file browsers open without freezing the UI** — Opening a script in Notepad, browsing to a log file, or using any "Open Directory" menu item no longer causes a brief freeze.

- **AutoMapper loads maps without Lich running** — Previously the mapper could fail to load maps unless Lich was active. Fixed.

- **`#mapper allowdupes`** now sets the state you ask for instead of always toggling.

- **`$unixtime` works inside `waiteval` conditions** in scripts.

- **Various script evaluation fixes** — `contains()` in compound `if` conditions, `#mapper save` path bug, map file handle leaks, and thread-safety issues in the AutoMapper have all been corrected.

---

### Portable / Self-Contained

- **Everything stays in one folder** — All settings, scripts, maps, logs, and sounds are stored next to the application instead of scattered across `%appdata%`. You can copy the entire folder to a USB drive or a new PC and it just works.

- **No installer required** — On first run, Genie creates all needed folders automatically.

- **Easy migration** — Copy the contents of your old `%appdata%\Genie4\` folder into the Genie application folder once to carry over all your existing settings and scripts.

---

### Auto-Update

- **Auto-update is disabled** — The original Genie4 auto-updater would overwrite this version with the upstream release on launch. It has been turned off so your client stays on this version.
