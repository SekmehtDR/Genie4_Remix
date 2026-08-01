# Changelog

All notable changes to Genie Remix are recorded here.

Written for **players**, not for the compiler — describe what changed about using the client.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning follows
[docs/VERSIONING.md](docs/VERSIONING.md).

Sections, in order, omitting any that are empty:
`Added` · `Changed` · `Fixed` · `Removed` · `Known issues`

> Entries before 4.1.0.0 were reconstructed from commit history after the fact and are
> summarised rather than exhaustive.

---

## [Unreleased]

### Fixed
- **Genie forgot it was using Lich as soon as you connected.** The connection to the login
  server is closed on purpose partway through a successful Lich login, and that closure was
  being treated as a real disconnect — clearing the "using Lich" flag for the rest of the
  session. Two things followed from it. Saving a profile wrote `UseLich=False` even though you
  were connected through Lich at that moment, quietly reversing the setting. And if you were
  dropped, auto-reconnect rebuilt the connection **straight to the game server, bypassing Lich
  entirely** — no scripts, no Lich features, and a Lich process still holding your old session.
  Reconnecting now goes back through Lich. If Lich is gone it will fail and say so, rather than
  silently connecting without it.

- **A Lich already running is now used instead of a second one being started.** Genie launched
  a brand new Lich on every single connect, without ever checking whether one was already there.
  If you run Lich as a background service, or you reopened Genie after a crash, you ended up with
  two Lich processes competing for the same port. Genie now checks the port first and connects to
  whatever is already listening. `#ls` reports which of the three states you are in: not running,
  listening and ready to be reused, or already serving a session.

- **A mistyped profile name no longer leaves a stray Lich running.** Lich was started *before*
  Genie had checked the profile or logged in, so `#lc SomeTypo` — or any failed login — left a
  Lich running with nothing connected to it, forever. `#lc <profile>` now checks the profile
  exists before starting anything.

- **Lich failing to start is now reported.** The result of launching Lich was discarded, so a bad
  Ruby path, bad Lich arguments, or Lich exiting immediately all looked identical to success —
  until the connection was refused several seconds later and appeared to be a problem with the
  game. Genie also no longer launches Lich through `cmd.exe`, which means paths containing spaces
  work, and it waits for Lich to actually open its port rather than sleeping a fixed number of
  seconds. Connecting is usually quicker as a result.

- **Login failures at the authentication step no longer vanish.** If the secure connection to the
  login server failed, Genie announced "Connected" anyway and carried on into a connection that
  had already been thrown away; the error disappeared into a background thread and the window sat
  there indefinitely with no message. Failures during login now print the reason and end the
  attempt cleanly, so auto-reconnect can take over.

- **Starting Genie from a `.sal` file or command line could drop your login key.** The startup
  parameters were split at whichever delimiter was listed first rather than whichever appears
  first, so a key containing a hyphen — `KEY=abcd-1234-efgh` — was cut at the hyphen and thrown
  away, and Genie connected with no key at all. Hyphenated hostnames broke the same way.

- **That same startup connect happened too early.** It ran before your settings, highlights,
  substitutes, gags, triggers, macros and classes had loaded, and before the game window
  existed — so the first lines of the session arrived with no triggers to fire them, and even
  the "Connected to..." message went nowhere. It now happens once everything is loaded.

- **Reconnect no longer claims to be trying when it cannot.** Reconnecting logs in again with
  your account name and password, which a session started from a `.sal` file never had. Genie
  said "Attempting to reconnect" anyway, then quietly did nothing and cancelled the retry. It
  now says plainly that it cannot reconnect automatically, and why.

- **Genie did not notice when the connection dropped abruptly.** If the other end went away
  suddenly — Lich killed or crashed, the network dropped, the server reset the connection —
  Genie carried on as though nothing had happened: no message, a title still reading
  `[Connected]`, and **auto-reconnect never firing**, because nothing had told it the connection
  was gone. The only clue was that typing did nothing and your commands came back wrapped in
  (parentheses). A polite disconnect was always handled correctly, so this only ever appeared on
  the sudden kind of loss that reconnect exists for. Genie now reports
  `Connection to <host> lost.` and reconnects.

- **Auto-reconnect only ever made one attempt.** Genie will not reconnect unless you have typed
  something since it last got you into the game, which stops an unattended client being revived
  over and over. The flag behind that check was being cleared during each reconnect attempt, so
  the second attempt aborted with "No user input since last connect" — and that abort also
  cancelled the retry schedule. The 5s / 15s / 30s backoff was unreachable: one try, then
  silence. Retries now continue as intended, and the original guard still applies.

- **Closing Genie while connected could hang instead of shutting down.** Answering Yes to
  "You are connected to the game" sent `quit` and then waited for the game to drop the
  connection before actually closing. If that never happened — DragonRealms refuses to quit in
  combat or roundtime, and Lich can be busy in a script — Genie simply sat there with the window
  still open. Closing now gives up waiting after a few seconds, says so, and shuts down anyway.

- **Closing with the "are you connected" prompt turned off skipped the clean shutdown.** With
  `#config {ignoreclosealert} {True}`, closing while connected dropped the window without
  telling plugins they were closing and without quitting the game, so plugins that save state on
  exit lost it. Every way of closing now notifies plugins exactly once.

- **Occasional garbled text in the first moments after connecting.** The closing login-server
  connection and the newly opened game connection shared the same text buffers with no
  coordination, so the first lines of a session could be split or interleaved — an intermittent
  cause of logon triggers not firing.

- **"Connection closed." during login no longer looks like being dropped.** Logging in involves
  two connections: Genie authenticates with `eaccess.play.net`, collects your login key, closes
  that connection, and then opens a second one to Lich or the game and hands the key over.
  Closing the first is a normal part of a successful login — but it printed the identical line
  Genie prints when the *game* connection drops, so a routine handoff was indistinguishable from
  a disconnect. The message now names the host: `Connection to eaccess.play.net closed.`

---

## [4.1.2]

### Changed
- **Smaller download.** The ZIP drops from 55 MB to 47 MB, and the extracted folder from 393
  files to 252. Most of that was 13 folders of translated text (`cs`, `de`, `es`, `fr`, `it`,
  `ja`, `ko`, `pl`, `pt-BR`, `ru`, `tr`, `zh-Hans`, `zh-Hant`) belonging to Microsoft libraries.
  Genie Remix is English-only and never displayed any of it.

  The rest was a .NET Framework compatibility package that pulled in about fifteen unused
  assemblies — the whole WCF networking stack, legacy web services, and ODBC database drivers —
  none of which the client uses. It was there to satisfy a single leftover `using` statement for
  text-to-speech that no code called.

  Nothing was removed that the client actually loads, and the plugin interface is unchanged.

---

## [4.1.1]

### Added
- **Help → Check For Updates.** Genie Remix can now update itself. It checks this repository —
  never the original Genie4 — tells you what version you are on and what is available, and asks
  before doing anything. If you accept, it downloads the release, verifies the download against
  the published checksum, then closes, installs over your existing folder and reopens.

  Your `Config`, `Scripts`, `Maps`, `Plugins`, `Logs`, `Icons` and `Sounds` folders are left
  completely alone, so settings, highlights, scripts and maps carry over. If anything fails —
  no connection, a corrupt download — nothing is changed and you keep running what you have.

  There is **no automatic checking**. It only ever runs when you choose it from the menu.

  *This is the first build with a working updater. Updating to it has to be done by hand, but
  from here on the client can update itself.*

Behind the scenes, this release also replaces the whole build and release pipeline. None of the
rest of this section changes anything you will notice while playing.

- `CLAUDE.md`, `docs/VERSIONING.md`, `docs/RELEASING.md`, `docs/RELEASE-READINESS.md`, and this
  changelog — release-cycle and contributor documentation.
- `Directory.Build.props` — single source of truth for the client version, overridable by CI
  from the release git tag.
- `.github/workflows/build.yml` — continuous integration. Every push and pull request now gets a
  Release build on a clean Windows runner, with assertions that the version stamping and plugin
  ABI version are intact, plus a self-contained publish whose contents are verified and uploaded
  as a downloadable test build.
- `.github/workflows/release.yml` — tag-driven releases. Pushing a `v*` tag builds, verifies,
  packages, checksums and publishes the release with no manual steps. Downloads are now
  version-stamped (`Genie-Remix-<version>.zip`) and ship with `SHA256SUMS.txt`.

### Changed
- The client version is now generated by MSBuild instead of being hardcoded in
  `Properties/AssemblyInfo.cs`. Assembly metadata (title, product, company, copyright) moved to
  `Genie4.csproj`. The plugin ABI version in `Plugin/AssemblyInfo.vb` is unchanged at `2.0.0.1`
  and is deliberately not tied to client releases.
- `README.md` now shows the current version and release date via live GitHub badges rather than
  hardcoded text.
- Release build warnings dropped from 5,109 to 45. Enabling assembly info generation restored
  the `SupportedOSPlatform` attribute, eliminating 10,128 spurious `CA1416` warnings that were
  masking real ones. `CA1416` is no longer suppressed, so it is free to report genuine platform
  problems, and the per-configuration `NoWarn` lists no longer silently discard the real
  suppression list.
- `.gitignore` rewritten and scoped to this project: 355 lines down to ~110. Adds rules for the
  portable runtime folders the client creates beside the executable (`Config/`, `Scripts/`,
  `Maps/`, `Logs/`, `Icons/`, `Plugins/`, `Sounds/`) so a player's own data can never be
  committed by accident.

### Removed
- `<ApplicationVersion>` from `Genie4.csproj` — an unused ClickOnce leftover that only served to
  drift out of sync with the real version.
- Untracked four committed artifacts: three stale `Plugin/obj/Debug/` build files and
  `Resources/Thumbs.db`. All predated the ignore rules that should have covered them.
- The leftover update paths that pointed at the original Genie4 project. These were the reason
  updating had to be switched off — they would quietly replace Genie Remix with upstream Genie4.
  "Force Update" and "Load Test Client" are gone, and the startup update check has been removed
  entirely.

---

## [4.1.0.0] — 2026-04-21

The .NET 10 release. Genie4 Remix becomes **Genie Remix**.

### Added
- **Connect via Lich** checkbox on the Game Connect dialog, alongside the existing per-profile
  preference.
- **Shift+select context menu** on output text — send a selected phrase straight to a config
  panel (highlight, substitute, ignore, alias, trigger) without retyping it.

### Changed
- **Migrated to .NET 10.** The client now bundles the .NET 10 runtime; no separate runtime
  install is required.
- **Rebranded to "Genie Remix"** in the title bar and assembly metadata.
- **Per-monitor DPI awareness (PerMonitorV2)** — the client stays sharp when moved between
  displays with different scaling.
- **System fonts by default**, with a Consolas fallback for monospace output.
- Reduced RichTextBox trim frequency, cutting stutter in heavy output.

### Fixed
- **Input bar font rendering** — multiline mode, dynamic height, and underscore/caret visibility.
- **"Character not found"** connection failure.
- **Font labels in Window Settings** were wrong; the default input font is now size 10 everywhere.
- **Clipboard contention** when adding images, now retried instead of failing.
- Duplicate `GenieError` event subscription.
- Assorted stability work: authentication crash, socket disposal, `async void` handlers, the
  receive loop, and script race conditions.

---

## [4.0.3.2] — 2026-04-10

### Added
- **Lich configuration tab** with Ruby/Lich path settings and a **Test** button that verifies
  your paths before you connect.
- **Per-profile "Connect via Lich"** preference that persists across sessions.
- Config option controlling script trigger-parse behaviour.
- **"Ready"** state on the cast roundtime bar — it stays lit once your spell is prepared and the
  cast timer has expired.
- **CT label** on the casting roundtime bar; 3D bubble styling on status bars.

### Changed
- **The auto-updater is disabled.** It pointed at the upstream Genie4 repository and would
  silently downgrade Remix installs back to upstream on launch.
- Trigger processing decoupled from the network thread; `SetBufferEnd` no longer blocks the
  socket receive thread.
- External editor and file-browser launches no longer block the UI.
- Mana / Inner Fire bar decoupled from the Magic Panels toggle, with guild-aware bar layout
  (Barbarians get Inner Fire; guilds without mana get a cleaner layout).

### Fixed
- **Lich reliability** — Lich mode sticks after a disconnect, manually entered credentials no
  longer bypass Lich, and concurrent socket writes to Lich are serialized.
- **Highlights** — the case-insensitive flag is now respected, and highlights survive fast
  scrolling output.
- Connection state not updating on disconnect; early-connect crashes; stuck reconnect after an
  authentication failure; slow quit (the server's `<exit/>` signal is now handled).
- **GDI resource leaks** in the roundtime and health/mana bar components, and a pen leak in
  `ComponentRoundtime`.
- Script bar, input box, and input panel no longer flash light on startup.
- Menu separators are now visible in all menus; config panel icons restored; status strip flat
  borders removed.
- Debug log formatting.

---

## [Initial Release] — 2026-04-03

First public Genie Remix build, forked from [GenieClient/Genie4](https://github.com/GenieClient/Genie4)
and merged with upstream's `Dev-4-0-2-10` branch.

### Added
- Full **dark, light, and custom theme** support across the client, including menus, scrollbars,
  title bars, the AutoMapper, and all status bars.
- **Portable operation** — config, scripts, maps, and logs live beside `Genie.exe` and move with
  the folder to any PC.

### Fixed
- Upstream issues #54, #80, #125, #145, #154, #166, #168, #169, #178, #179.

---

[Unreleased]: https://github.com/SekmehtDR/Genie4_Remix/compare/v4.1.2...HEAD
[4.1.2]: https://github.com/SekmehtDR/Genie4_Remix/releases/tag/v4.1.2
[4.1.1]: https://github.com/SekmehtDR/Genie4_Remix/releases/tag/v4.1.1
[4.1.0.0]: https://github.com/SekmehtDR/Genie4_Remix/releases/tag/v4.1.0.0
[4.0.3.2]: https://github.com/SekmehtDR/Genie4_Remix/releases/tag/Latest
[Initial Release]: https://github.com/SekmehtDR/Genie4_Remix/releases/tag/Initial-Release
