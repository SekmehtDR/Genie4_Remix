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

### Lich Integration

- **Lich setup is now part of the client.** A new Lich tab in the Configuration window lets you set up your Ruby and Lich paths with Browse buttons and a Test button that confirms everything is working before you connect. No more typing setup commands by hand.

- **Each character remembers whether to use Lich.** The Connect window now has a "Connect via Lich" checkbox. Check it once for a character and it sticks. You no longer need to type a special command every time you log in.

- **Lich connection issues between sessions are fixed.** If you logged out while using Lich, the next login would sometimes silently try to connect through Lich again even if it was not running, causing a stuck or failed connection. Each session now starts clean.

- **Lich works correctly even if your password is not saved.** If Genie prompted you to type your password manually, it would skip Lich entirely regardless of your preference. That is fixed.

---

### Performance

- **The client stays responsive during busy moments.** In heavy combat or crowded rooms, incoming game text, triggers, and scripts no longer pile up and block each other. Text appears promptly and the client does not fall behind or freeze up.

- **Scripts no longer cause text lag.** There was a bug where running scripts could delay incoming game text by several seconds per line. If you ever saw text arrive very slowly even though your internet was fine, this was likely why. It is fixed.

- **Connecting to the game is smoother.** The login process no longer causes a brief freeze of the main window while it works in the background.

---

### UI Enhancements

- **Dark, light, and custom color themes are fully supported.** The entire client responds to your chosen theme including menus, scrollbars, title bars, and all windows. Switch anytime from the Layout menu.

- **The cast roundtime bar now shows "Ready" when your spell is prepared.** Once the cast timer finishes counting down, the bar stays lit and shows "Ready" so you always know your spell is still up and waiting. It clears when you cast, dismiss, or lose the spell.

- **The client opens cleanly with no white flash.** Previously the window would briefly appear unstyled before your theme loaded. It now opens fully styled from the start.

- **All status bars scale correctly at any window size.** The spell cast bar was floating off to the right side of the screen at wider resolutions. It is now properly placed and stretches with the window.

- **The mana bar is handled correctly for all guilds.** Barbarians see Inner Fire correctly. Thieves and Commoners, who have no mana bar, have it hidden automatically and the other bars shift over to fill the space.

- **The AutoMapper matches your chosen theme.** The map background, toolbar, node colors, and your current position indicator all update when you switch themes. Your current room and active route are clearly highlighted on the map.

- **All your files stay in one place.** Settings, scripts, maps, logs, and sounds are stored in the same folder as the application rather than spread across your system. You can move the whole folder to a new PC or a USB drive and it works without any setup.

---

### Bug Fixes

- **Highlights set to ignore capitalization now work correctly.** The ignore case option was silently not working. It does now.

- **Highlights still work correctly when timestamps are turned on.** Certain highlight patterns would stop matching once timestamps were enabled. That is fixed.

- **Highlights and Substitutes can be turned off from the File menu.** New toggle checkboxes let you quickly disable them without deleting anything, which is handy for troubleshooting or taking clean screenshots.

- **Script activity now shows up in the Auto Log.** If you use Auto Log to save your session, script messages like loaded, aborted, and debug output were not being saved to the file. They are now.

- **The right-click menu on running scripts is visible again.** The Resume, Pause, Abort, and other options on script buttons disappeared after a theme update. They are back.

- **Menu dividers are visible again.** The thin lines separating groups of menu items were being squashed to invisible. Fixed.

- **Opening scripts, logs, or folders no longer freezes the client.** Any action that opens an external program or file browser now does so without briefly locking up the window.

- **The AutoMapper works without Lich running.** Maps would sometimes fail to load unless Lich was active. That dependency is removed.

- **Several script command fixes.** `#mapper allowdupes`, `#mapper save`, `$unixtime` in waiteval conditions, and `contains()` in compound if statements all behaved incorrectly in certain situations. All are fixed.

- **Auto-update is turned off.** The original Genie4 updater would replace this version with the upstream release on launch. It has been disabled so your client stays put.
