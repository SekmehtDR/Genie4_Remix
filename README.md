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

## What's New

**Themes and visual improvements.** Genie4_Remix adds full support for dark, light, and custom color themes across the entire client, including menus, scrollbars, title bars, the AutoMapper, and all status bars. The spell cast roundtime bar now stays lit and shows "Ready" once your cast timer finishes, so you always know your spell is up and waiting. The mana bar is also handled correctly per guild, with Barbarians seeing Inner Fire and guilds that have no mana bar seeing a cleaner layout automatically. The client also ships in a fully portable format, meaning all your settings, scripts, and maps live in the same folder as the application and move with it to any PC.

**Lich support.** Connecting through Lich is now a first-class feature rather than something you have to set up through typed commands. A new Lich tab in the Configuration window walks you through pointing the client at Ruby and Lich, and a Test button confirms your paths are correct before you connect. Each character profile also has a "Connect via Lich" checkbox that remembers your preference, so you just click Connect like normal. Several reliability issues with Lich connections, including sessions getting stuck and manual password entry bypassing Lich entirely, have been resolved.

**A lot of bugs squashed.** This release addresses a wide range of issues across the client including significant performance improvements that reduce lag during heavy combat or script use, fixes to highlights and substitutes not working correctly in certain cases, AutoMapper reliability and theme improvements, script engine corrections, logging fixes, and many smaller UI issues that accumulated in the original codebase. The auto-updater has also been disabled so this version stays put rather than being replaced by the upstream release on launch.
