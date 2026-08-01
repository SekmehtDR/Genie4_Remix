# CLAUDE.md

Guidance for Claude Code (and any other agent) working in this repository.

---

## What this project is

**Genie Remix** is an unofficial fork of [Genie4](https://github.com/GenieClient/Genie4), a
Windows front-end (MUD client) for **DragonRealms**. It is a C# **WinForms** desktop app
targeting **.NET 10 (`net10.0-windows`)**, shipped as a portable, self-contained folder.

- Repo: `SekmehtDR/Genie4_Remix` (`origin`)
- Upstream: `GenieClient/Genie4` (`upstream`) — rarely merged, diverged significantly
- License: GPL-3
- Users are **DragonRealms players, not developers.** They download a ZIP, extract, and
  double-click `Genie.exe`. Assume no .NET SDK, no runtime install, no command line.

### Non-negotiable user-facing contracts

Breaking any of these breaks real players' setups. Treat them as frozen unless the user
explicitly asks otherwise.

| Contract | Why it matters |
|---|---|
| **Portable mode** — all data lives beside `Genie.exe` (`Config/`, `Scripts/`, `Maps/`, `Plugins/`, `Logs/`, `Icons/`) | Users move the folder between PCs. See `LocalDirectory.cs`. |
| **Assembly version is 4-part numeric** (`4.1.0.0`) | Sent to the game server as `FE:GENIE /VERSION:...` (`Lists/Config.cs`) and exposed to scripts as the `$version` variable (`Lists/Globals.cs`). A SemVer pre-release suffix would break both. |
| **Updates never point at upstream** | `Utility/RemixUpdater.cs` checks **this** repo. The old Lamp path targeted `GenieClient/Genie4` and would *downgrade* Remix installs — that is why updating was switched off. Never reintroduce an upstream client-update path. |
| **Updates are user-initiated only** | Help → Check For Updates. There is no startup check and no auto-update, deliberately. |
| **Release asset naming** | `RemixUpdater` looks for an asset named `Genie-Remix*.zip` and reads `SHA256SUMS.txt`. Renaming release assets breaks every installed client's ability to update. |
| **Config/script/map file formats** | Users' existing highlights, aliases, triggers, and maps must keep loading. Drop-in replacement for Genie4 is a headline promise. |
| **Plugin ABI** (`Plugin/IPlugin.vb`, `IHost.vb`, `Libs/Interfaces.dll`) | Third-party plugins compile against this. Changing it orphans them. |

---

## Build and run

The **.NET 10 SDK** is required. Build from the repo root.

```powershell
# Restore + build everything (Genie4 + the VB plugin project)
dotnet build Genie4.sln -c Release

# Debug build
dotnet build Genie4.sln -c Debug

# Run locally (Debug output)
.\bin\Debug\net10.0-windows\Genie.exe

# Produce the shippable folder (this is what the release ZIP contains)
dotnet publish Genie4.csproj -c Release -r win-x64 --self-contained true -o publish
```

Notes verified against the current tree:

- `dotnet build Genie4.sln -c Release` → **0 errors, 45 warnings** (see *Known noise*).
- A self-contained `win-x64` publish is **~112 MB / ~250 files**, compressing to a **~47 MB ZIP**.
  `SatelliteResourceLanguages=en` in `Directory.Build.props` keeps dependency translations out;
  CI fails if localised resource folders reappear. Don't remove it — it costs 117 files and
  ~8 MB of download for strings this client never shows.
- There are **no automated tests.** Verification is manual — build, launch, connect, exercise
  the affected feature. Say so plainly rather than implying a change is "tested".

### Warnings

The build emits **45 warnings**. That number is low enough to be meaningful — **treat a new
warning as a signal, not noise.** If it climbs, something regressed.

Worth knowing:

- **The 45 are not all benign.** They include 12 × `CS4014` (async call not awaited) and
  8 × `CA2200` (`throw ex`, discarding the original stack trace). Both are the shape of bugs
  this codebase has actually shipped. Fair game to fix, one at a time, with care.
- **`CA1416` should stay at zero.** It was 10,128 until `GenerateAssemblyInfo` was turned on,
  because without the SDK-generated `[assembly: SupportedOSPlatform("Windows7.0")]` the analyzer
  thinks this Windows-only app targets every platform. If CA1416 reappears in bulk, that
  attribute has gone missing again — check `GenerateAssemblyInfo`, don't add suppressions.
- **The `<NoWarn>` in the main `PropertyGroup` is dead.** The `Debug|AnyCPU` and `Release|AnyCPU`
  groups each *reassign* `<NoWarn>` to a legacy list, wiping it. Add suppressions to those two
  groups, or fix it properly by appending `$(NoWarn)` there.
- `.editorconfig` suppresses `WFO1000` repo-wide, deliberately, from the .NET 10 port.

---

## CI

[`.github/workflows/build.yml`](.github/workflows/build.yml) runs on every push to `main`, every
pull request, and on manual dispatch (`gh workflow run Build`).

| Job | Runs on | What it does |
|---|---|---|
| `build` | push, PR, dispatch | Release build of the solution; asserts version stamping; posts a warning breakdown to the run summary |
| `package` | push, dispatch (**not** PRs) | Self-contained `win-x64` publish, verifies package contents, uploads a downloadable test build (14-day retention) |

**Grab a test build without cutting a release:** the `package` job's artifact
`Genie-Remix-ci-<run number>` on any green `main` run is a complete, runnable folder.

Things that will fail the build — all deliberate tripwires:

- `Genie.dll` stamped `0.0.0.0` → assembly version generation broke
- `AssemblyVersion` not matching `MAJOR.MINOR.PATCH.0`
- Plugin ABI version drifting off `2.0.0.1`
- Publish output under 100 MB → `--self-contained` stopped taking effect
- Any of the required runtime files missing from the publish

The warning count is **reported, not enforced** — `BASELINE_WARNINGS` in the workflow is
currently 45. Going over it annotates the run; it does not block.

There is deliberately **no `paths-ignore`** filter. A required check that gets skipped on
docs-only changes leaves pull requests blocked forever.

[`.github/workflows/release.yml`](.github/workflows/release.yml) runs on `v*` tags and builds,
verifies, packages and publishes the release. It refuses to run if the tag isn't on `main`, if a
release for that tag already exists, if `CHANGELOG.md` has no section for the version, or if the
built binary doesn't carry exactly the tag's version. Dry-run before touching it:

```powershell
gh workflow run Release -f version=4.1.1 -f dry_run=true
```

**Do not tag or push tags unless asked** — a tag publishes a release to real users.

---

## Repository layout

```
Core/          Connection, Game (XML protocol parsing), Command dispatch, plugin hosting
Forms/         All WinForms UI. FormMain.cs is the ~9k-line hub. Config panels in ConfigPanels/
Mapper/        AutoMapper — map rendering, pathfinding, node/arc editing
Script/        Genie script engine + JavaScript (Jint) and Lua backends, expression eval
Lists/         In-memory state: Config, Globals ($variables), Highlights, Aliases, Macros, Triggers
Utility/       Cross-cutting: logging, crypto, dark mode, embedded assembly loading
               RemixUpdater.cs — self-update against THIS repo (Help -> Check For Updates)
               Updater.cs      — Lamp-based, CONTENT ONLY (maps/scripts/plugins/art)
Plugin/        VB.NET class library (Plugins.vbproj) defining the plugin ABI
Libs/          Binary references (Antlr3.Runtime, Interfaces, Jint) — embedded as resources
Resources/     Skin bitmaps
Graphics/      Application icon
Properties/    AssemblyInfo.cs (VERSION LIVES HERE), app.manifest, VB-compat designer shims
docs/          Versioning, release process, release-readiness backlog
```

### Landmines

- **`Properties/MyNamespace.*.Designer.cs`** are hand-patched Microsoft.VisualBasic
  compatibility shims (`My.MyProject.Application.Info`). They look auto-generated but are not
  regenerable — edit with care, never delete.
- **`*.Designer.cs` under `Forms/` and `Mapper/`** are real WinForms designer files. Prefer
  editing them through intent-preserving hand edits; large reformatting will break the designer.
- **`Utility/EmbeddedAssembly.cs`** loads `Interfaces.dll` out of embedded resources at startup.
  Assembly-resolution changes can break plugin loading in ways that only appear at runtime.
- **`GenerateAssemblyInfo=false`** in both project files. See below — this is the single most
  important build fact in the repo.

---

## Versioning — read this before touching any version

**The version lives in `Directory.Build.props` at the repo root. Nowhere else.**

```xml
<VersionPrefix>4.1.1</VersionPrefix>   <!-- the only value a human edits -->
```

Everything else derives from it: `AssemblyVersion` = `<prefix>.0`, `FileVersion` =
`<prefix>.<BuildNumber>`, `InformationalVersion` = `<prefix>[-suffix]+<sha>`. CI overrides
`VersionPrefix`, `BuildNumber`, and `SourceRevisionId` from the release git tag.

Rules that will bite you:

- **Do not add version attributes to `Properties/AssemblyInfo.cs`.** `GenerateAssemblyInfo` is
  `true`, so the SDK generates them. Re-adding one causes `CS0579 duplicate attribute`. That
  file now holds only `AssemblyTrademark`, `ComVisible`, and `Guid`.
- **Do not set `GenerateAssemblyInfo=false` on `Genie4.csproj`.** It gates the whole attribute
  generation target — the build still succeeds but stamps the binary **`0.0.0.0`**.
- **`Plugin/AssemblyInfo.vb` is a separate thing.** Its `AssemblyVersion("2.0.0.1")` is the
  **plugin ABI version**, deliberately decoupled. Never bump it as part of a release; only when
  `IPlugin.vb` / `IHost.vb` actually change shape. The version properties in
  `Directory.Build.props` are scoped to `MSBuildProjectName == 'Genie4'` to enforce this.
- **README carries no version number** — it uses live GitHub badges. Keep it that way.

To check a version end to end:

```powershell
dotnet build Genie4.csproj -c Release -o .\verify -p:VersionPrefix=9.9.9 -p:BuildNumber=77
[System.Reflection.AssemblyName]::GetAssemblyName(".\verify\Genie.dll").Version   # 9.9.9.0
(Get-Item .\verify\Genie.dll).VersionInfo.FileVersion                            # 9.9.9.77
```

Full scheme: **[docs/VERSIONING.md](docs/VERSIONING.md)**.

---

## Release process

Releases are GitHub Releases with a single `Genie-Remix.zip` asset (self-contained `win-x64`
publish, nested under a `Genie-Remix/` folder). Full procedure:
**[docs/RELEASING.md](docs/RELEASING.md)**.

Every user-visible change should land a line in **[CHANGELOG.md](CHANGELOG.md)**.

---

## Working conventions

**Match the surrounding code.** This is a 20-year-old codebase carried through a VB.NET → C#
conversion. It is not idiomatic modern C# and does not need to become so. Follow local style
(including `Hungarian`-ish prefixes like `sScriptDir`, `bEnabled`, `iCount`) rather than
"improving" it.

- **Scope discipline.** Fix the thing asked. Drive-by refactors in `FormMain.cs` are how
  regressions get shipped.
- **Threading.** Socket receive, script execution, and the UI run on different threads.
  Anything touching a control from a non-UI thread needs `Invoke`/`BeginInvoke`. Several past
  bugs were exactly this — check before adding UI updates in `Core/Connection.cs` or
  `Script/Script.cs`.
- **Commit messages**: imperative, one line summarising user impact
  (e.g. `Fix input bar font rendering: multiline mode, dynamic height`). Longer body when the
  *why* isn't obvious.
- **Never commit** `bin/`, `obj/`, `publish/`, `*.pdb`, or `.vs/` — all gitignored, keep it that way.
- **Never commit** the signing key `Plugin/GenieStrongKey2020.pfx` or any credential.
- **Don't push or tag unless asked.** Tagging triggers a release build.

### Before claiming something works

There is no test suite. "It builds" is not "it works". State exactly what was verified:
built / launched / connected / exercised the feature — and what was not.

---

## Quick reference

| Task | Where |
|---|---|
| Bump the version | `Properties/AssemblyInfo.cs` + `Genie4.csproj` `<ApplicationVersion>` |
| Add a config setting | `Lists/Config.cs`, then a UI panel in `Forms/ConfigPanels/` |
| Add a script variable | `Lists/Globals.cs` |
| Add a client command | `Core/Command.cs` |
| Change the connect flow | `Core/Connection.cs`, `Forms/DialogConnect.cs`, `Forms/DialogProfileConnect.cs` |
| Change server XML parsing | `Core/Game.cs` |
| Change status bars / RT bars | `Forms/Components/ComponentBars.cs`, `ComponentRoundtime.cs` |
| Theme / dark mode | `Utility/DarkModeManager.cs`, `Forms/Components/MenuRenderer.cs` |
| Change how updates work | `Utility/RemixUpdater.cs` + `checkForUpdatesToolStripMenuItem_Click` in `FormMain.cs` |
