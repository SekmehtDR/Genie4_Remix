# Releasing Genie Remix

> **Status: implemented** (2026-07-31) in
> [`.github/workflows/release.yml`](../.github/workflows/release.yml), but **not yet exercised on
> GitHub**. Every step has been run locally against real build output; runner-specific behaviour
> (SDK resolution, `gh release create`, the smoke test on a hosted runner) is unproven until the
> first real run. Do a [dry run](#dry-run-first) before the first real tag.
>
> [Today, by hand](#the-old-manual-process) is kept for reference — it explains what the
> automation is protecting you from.

---

## What a release is

A **GitHub Release** on `SekmehtDR/Genie4_Remix` with:

- a tag `v<MAJOR>.<MINOR>.<PATCH>` (see [VERSIONING.md](VERSIONING.md))
- one asset: `Genie-Remix-<version>.zip`
- a `SHA256SUMS.txt` asset
- a release body generated from [CHANGELOG.md](../CHANGELOG.md)

### What's in the ZIP

A **self-contained `win-x64` publish**, nested one level under a `Genie-Remix/` folder so that
extracting never scatters ~390 files into the user's Downloads directory.

```
Genie-Remix-<version>.zip
└── Genie-Remix/
    ├── Genie.exe                  <- the apphost users double-click
    ├── Genie.dll                  <- the actual client
    ├── Genie.runtimeconfig.json
    ├── System.Private.CoreLib.dll  ) the bundled .NET 10 runtime
    ├── ... ~230 more runtime dlls  )
    ├── Interfaces.dll, Jint.dll, Acornima.dll, Plugins.dll
    ├── Libs/
    └── LICENSE
```

Roughly **47 MB compressed, 112 MB extracted, ~250 files.**

There are **no localised resource folders**. `SatelliteResourceLanguages` in
`Directory.Build.props` trims dependency translations to English; without it the publish grows
13 extra folders and 117 extra files that nothing in this client can reach. CI fails the build if
they come back.

**No user data ships in the ZIP.** `Config/`, `Scripts/`, `Maps/`, `Plugins/`, `Logs/`, and
`Icons/` are created on first launch by `CreateGenieFolders()` in `Forms/FormMain.cs`. This is
deliberate — it is what makes upgrading safe, because extracting a new build over an old folder
cannot clobber a player's settings.

**Symbols are not shipped.** The `Release` configuration sets `<DebugType>None</DebugType>`, so
no PDBs are produced at all. Crash reports from `Forms/DialogException.cs` therefore arrive
without line numbers. Worth changing (see RELEASE-READINESS.md), but it is the current behaviour.

---

## The old manual process

How releases were cut before automation, reconstructed from the v4.1.0.0 release:

1. Edit `Properties/AssemblyInfo.cs` — bump `AssemblyVersion` and `AssemblyFileVersion`.
2. Edit `Genie4.csproj` — bump `<ApplicationVersion>`.
3. Edit `README.md` — update the version/date prose.
4. Commit, push.
5. `dotnet publish` self-contained `win-x64` locally.
6. Zip the output into a `Genie-Remix/` folder by hand.
7. Create the tag and GitHub Release in the web UI, hand-write the body, upload the ZIP.

### Why this needs replacing

Each of these actually happened on v4.1.0.0:

- **The asset was replaced in place, four days after the release was published.** The release is
  dated `2026-04-17`; the ZIP asset is dated `2026-04-21`. Two different builds shipped under
  one version number, and anyone who downloaded early has bits nobody can now identify.
- **The release body links to the wrong tag** — both download links point at
  `.../download/Initial-Release/Genie-Remix.zip`, not `v4.1.0.0`.
- **The asset filename carries no version.** `Genie-Remix.zip` in a user's Downloads folder is
  unidentifiable, and the URL is not stable across releases.
- **No checksum**, so a corrupted or tampered download is undetectable.
- **README says `4.0.3.2`** while the shipped build is `4.1.0.0`.
- **The build is not reproducible** — it came off one machine with whatever SDK was installed,
  and there is no record of which commit it was built from.

Every one of these is a supportability problem, not a cosmetic one: when a player reports a bug,
you currently cannot determine what code they are running.

---

## Cutting a release

Two steps by hand; everything else is derived.

```powershell
# 1. Move the CHANGELOG's [Unreleased] entries under a new "## [4.1.1]" heading, commit, push.
# 2. Tag it.
git tag v4.1.1
git push origin v4.1.1
```

The workflow does the rest:

| Step | What it does |
|---|---|
| Resolve version | `v4.1.1` → prefix `4.1.1`, suffix ``, build `<run_number>`, sha `<short>` |
| Guard: on main | Fails if the tagged commit is not an ancestor of `origin/main` |
| Guard: not published | Fails if a release for that tag already exists — published versions are immutable |
| Guard: changelog | Fails if `CHANGELOG.md` has no `## [4.1.1]` section |
| Build notes | Release body = that CHANGELOG section + install, upgrade and checksum instructions |
| Publish | `dotnet publish -c Release -r win-x64 --self-contained true` with versions from the tag |
| Assert version | Fails unless the binary carries exactly the tag's version, and the plugin ABI is still `2.0.0.1` |
| Assert contents | Required files present, no user-data folders, output ≥ 100 MB |
| Smoke test | Launches the published `Genie.exe`, requires it to stay up and create its folders |
| Package | `Genie-Remix-4.1.1.zip`, nested under `Genie-Remix/` |
| Checksum | `SHA256SUMS.txt` |
| Release | Creates the GitHub Release with both assets, pre-release flagged automatically if the tag contains `-` |

### Dry run first

`workflow_dispatch` builds and packages without publishing anything. Use it before the first
real tag, and any time the workflow changes:

```powershell
gh workflow run Release -f version=4.1.1 -f dry_run=true
gh run watch
```

The ZIP, checksums and rendered release notes are uploaded as a `dry-run-<version>` artifact so
you can inspect exactly what a real release would produce.

### Pre-releases

Tag `v4.2.0-rc.1`. The workflow detects the `-` and marks the GitHub Release as a pre-release, so
it does not become "Latest" and casual users keep getting the stable build.

### If the smoke test is flaky on the runner

Launching a WinForms app on a hosted runner is the least predictable step. If it proves
unreliable, set `SMOKE_TEST: 'false'` in the workflow's `env:` block and fall back to the manual
post-release check in the checklist below. Everything else keeps working.

## How players receive an update

Installed clients update themselves from these releases via **Help → Check For Updates**
(`Utility/RemixUpdater.cs`). Nothing is automatic — no startup check, no background download.

What the client does when a user asks:

1. Reads `releases/latest` from this repo. **Pre-releases are excluded by GitHub**, so an `-rc`
   tag is never offered to someone on a stable build.
2. Compares the tag against its own `AssemblyVersion`. Only a strictly newer version is offered.
3. Downloads the `Genie-Remix*.zip` asset and checks it against `SHA256SUMS.txt`. A mismatch
   aborts with nothing changed.
4. Extracts to a temp folder — the install is still untouched at this point.
5. Exits, then a helper copies the payload over the install folder and relaunches.

Because the copy is additive and the ZIP contains no user data, `Config/`, `Scripts/`, `Maps/`,
`Plugins/`, `Logs/`, `Icons/` and `Sounds/` survive an update untouched.

### This creates a contract you must not break

The updater is running on machines you cannot reach. It depends on:

| Thing | Why it matters |
|---|---|
| Asset named `Genie-Remix*.zip` | The client matches on this prefix. Rename it and no installed client can find the download. |
| `SHA256SUMS.txt` published alongside | Without it the client warns that it cannot verify the download. |
| ZIP nests everything under one folder containing `Genie.exe` | The client tolerates a flat archive too, but this is the expected shape. |
| ZIP contains **no** user-data folders | Shipping a `Config/` would overwrite every updating player's settings. CI fails the release if one appears. |
| Tag parses as `vMAJOR.MINOR.PATCH[-suffix]` | An unparseable tag (like the old `Latest` or `Test_Build`) is ignored, and users silently stop being offered updates. |

Older clients (4.1.0.0 and earlier) have **no** working update path — the Lamp updater pointed at
upstream and was disabled. Those users must download and extract manually once; from the first
Remix build carrying `RemixUpdater`, in-client updating works from then on.

### Rules for the automated flow

- **Tags are immutable.** Never move, delete, or re-point a tag that has a published release.
  A bad build gets `v4.1.2`, not a re-cut `v4.1.1`.
- **Assets are immutable.** Never replace an asset on a published release. If the bits change,
  the version changes.
- **`main` must build clean before tagging.** A CI check on every push to `main` and every PR
  makes this true by default rather than by hope.

---

## Release checklist

CI now covers the mechanical checks — the build, the version stamping, package contents, and
that the app starts. What it **cannot** check is whether the software is actually good, so the
list below is deliberately about things only a human can do.

Before tagging:

- [ ] Launched the build, connected to DragonRealms, and exercised whatever changed
- [ ] Verified the affected feature both with and without Lich, if the change touches connection
- [ ] Loaded an existing profile with real highlights, aliases and maps — nothing broke
- [ ] `CHANGELOG.md` section for this version is written in player-facing language
- [ ] Version bump matches [VERSIONING.md](VERSIONING.md) (patch/minor/major)
- [ ] Ran a dry run if the release workflow itself changed

After releasing:

- [ ] Downloaded the published ZIP, extracted, and launched it — on a machine that is *not* the
      build machine, to catch a missing-runtime problem CI cannot see
- [ ] Release body renders correctly on GitHub
- [ ] Checksum in `SHA256SUMS.txt` matches the downloaded file

---

## Rolling back

There is no in-place downgrade path — the auto-updater is deliberately disabled
(`Utility/Updater.cs` points at upstream `GenieClient/Genie4` and would downgrade Remix installs).

To pull a bad release:

1. Mark the bad GitHub Release as a **pre-release**. It stops being "Latest" and the
   `releases/latest` link that README points at immediately serves the previous good build.
2. Do **not** delete the release or the tag — users already running it need the artefact to
   remain identifiable.
3. Fix forward with the next patch version.

Because the client is portable and stores everything beside the exe, a user's own rollback is
"extract the older ZIP over the folder again" — their config survives. Say that in the release
body when shipping anything risky.
