# Release Readiness Review

**Reviewed:** 2026-07-31 · **Against:** `main` @ `e506c23` (tag `v4.1.0.0`) · **Reviewer:** Claude

A review of Genie Remix's supportability across release cycles: can you tell what a user is
running, reproduce it, and ship a fix with confidence? Findings are ordered by how much they
cost you when something goes wrong.

---

## Summary

The **code** is in good shape. The `.NET 10` port landed, the solution builds clean
(0 errors), the app is genuinely portable, and the fork has a clear, well-communicated identity.

The **release process** is the weak half. Every step from "bump the version" to "upload the ZIP"
is manual, and the v4.1.0.0 release shows the predictable consequences: a version number that
doesn't match the README, a release body linking to the wrong tag, and an asset that was
silently replaced four days after publication. Right now, **if a player reports a bug you cannot
determine what code they are running.** That is the single thing worth fixing.

| Area | State | Notes |
|---|---|---|
| Build reproducibility | ✅ Good | CI builds on a clean runner, records the SDK, stamps the commit sha into the binary |
| Version management | ✅ Fixed | Single source of truth in `Directory.Build.props`, injectable from the tag, asserted in CI |
| CI | ✅ Good | Build + package verification on push, PR, and dispatch; branch protection still optional |
| Release automation | ✅ Written | Tag-driven, with guards; not yet exercised on GitHub |
| Artifact integrity | ✅ Fixed | Version-stamped filename, `SHA256SUMS.txt`, immutability enforced |
| Changelog | ✅ Now exists | Added by this review |
| Rollback story | ⚠️ Informal | Works by accident (portable app), undocumented until now |
| Test coverage | ❌ None | No automated tests; not necessarily worth building, but know it |
| Contributor docs | ✅ Now exists | `CLAUDE.md` + `docs/` added by this review |

---

## Blockers — ✅ resolved 2026-07-31

Both were fixed in Phase 1. Kept here with their evidence, because the failure modes are subtle
and worth recognising if they recur.

### B1 — MSBuild version properties are silently ignored ✅ fixed

`Genie4.csproj` and `Plugin/Plugins.vbproj` both set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`,
and the version is hardcoded in `Properties/AssemblyInfo.cs`. Passing `-p:Version=` on the
command line therefore does nothing — **with no warning and no error.**

Verified:

```
dotnet publish -c Release -r win-x64 --self-contained true -p:Version=4.1.0.1 -p:FileVersion=4.1.0.1
→ Genie.dll FileVersion: 4.1.0.0     # the injected version was discarded
```

**Impact:** any CI release workflow would have built, packaged, uploaded, and labelled a release
with a version the binary does not carry. The failure is completely silent.

**Fixed by** moving to `GenerateAssemblyInfo=true` with the version defined in
`Directory.Build.props`. Note that the *minimal* fix — keeping `GenerateAssemblyInfo=false` and
re-enabling only the per-attribute switches — was tried and **produced a `0.0.0.0` binary**;
see [VERSIONING.md § The dead end worth remembering](VERSIONING.md#the-dead-end-worth-remembering).

### B2 — The version was stored in three places ✅ fixed

| Location | Was | Now |
|---|---|---|
| `Properties/AssemblyInfo.cs` | `4.1.0.0` hardcoded | version attributes removed |
| `Genie4.csproj` `<ApplicationVersion>` | `4.1.0.0` | deleted (ClickOnce leftover) |
| `README.md` | hardcoded version + release date | live badges from the GitHub API |

Source of truth is now `Directory.Build.props`, overridden by CI from the git tag.

*Correction to the original review:* this section first claimed README said `4.0.3.2` while
`4.1.0.0` shipped. That was read from a working copy 28 commits behind `origin/main`; the README
on `main` was already correct at `4.1.0.0`. The drift risk was real, the specific drift was not.

---

## High

### H1 — No CI whatsoever ✅ fixed 2026-07-31

There was no `.github/` directory. Nothing verified that `main` compiled. Several commits in the
history (`Add files via upload`, `Delete screenshots/...`) were made through the GitHub web UI,
so they were never built locally either.

**Fixed by** [`.github/workflows/build.yml`](../.github/workflows/build.yml), which runs on every
push to `main`, every pull request, and on manual dispatch:

- **`build`** — Release build of the full solution on `windows-latest` with .NET 10. Records
  `dotnet --info` for toolchain provenance, asserts the version stamping is intact, and posts a
  warning breakdown to the run summary.
- **`package`** — self-contained `win-x64` publish, verifies the output actually contains what
  players need, and uploads it as a downloadable test build (14-day retention). Skipped on pull
  requests to keep review feedback fast.

The version assertions are the important part: they fail the build if `Genie.dll` is stamped
`0.0.0.0` (the B1 regression), if `AssemblyVersion` isn't `MAJOR.MINOR.PATCH.0`, or if the
plugin ABI version drifts off `2.0.0.1`.

**Still open:** requiring the check before merge — see M5.

### H2 — Release assets are mutable and have been mutated ✅ fixed

The v4.1.0.0 release was published `2026-04-17T00:45:45Z`. Its `Genie-Remix.zip` asset was
uploaded `2026-04-21T23:36:22Z` — **four days later.** Two different builds shipped under one
version number. The 38 recorded downloads cannot be attributed to either.

**Impact:** "what version are you on?" stops being a meaningful question. Bug reports become
unfalsifiable.

**Fixed by** the release workflow, which refuses to run if a release for the tag already exists,
and by CI's downloadable test builds — you no longer need to publish a release to hand someone a
build to try. Bad builds get a new patch version; assets are never replaced.

### H3 — Asset filename carries no version, and the release body links to the wrong tag ✅ fixed

The asset is `Genie-Remix.zip` for every release. In a user's Downloads folder it is
unidentifiable, and the download URL is not stable.

Worse, the v4.1.0.0 release body's two download links both point at
`.../download/Initial-Release/Genie-Remix.zip` — the **first** release, not the one being
announced. Hand-written release bodies with copy-pasted links do this eventually; this one
already has.

**Fixed:** assets are now `Genie-Remix-<version>.zip`, and the release body is generated from the
`CHANGELOG.md` section for that version — the download instructions reference the asset by its
real name, so they cannot point at the wrong tag.

### H4 — No checksums, no provenance ✅ fixed

No `SHA256SUMS.txt`. Nothing in the release recorded which commit or SDK produced the binary. A
corrupted download, or "was this built before or after that fix?", could not be answered.

**Fixed:** releases ship `SHA256SUMS.txt`; `AssemblyInformationalVersion` embeds the short commit
sha (`4.1.1+a3f19c2`), so a running binary identifies its own source commit; and the release body
links back to the Actions run that built it.

### H5 — ~5,100 warnings hide real ones ✅ fixed as a side-effect of Phase 1

**Corrected diagnosis.** The original review blamed the `<NoWarn>` override — `Genie4.csproj`
sets `<NoWarn>$(NoWarn);WFO1000;CA1416</NoWarn>` in the main `PropertyGroup`, and the
`Debug|AnyCPU` / `Release|AnyCPU` groups each reassign `<NoWarn>` to a legacy list, wiping it.
That override is real, but it was **not** the cause.

The actual cause was `GenerateAssemblyInfo=false`. It suppressed the SDK-generated
`[assembly: SupportedOSPlatform("Windows7.0")]`, so the platform-compatibility analyzer treated
a `net10.0-windows` app as targeting *every* platform and flagged every Windows API call —
hence the warning text "This call site is reachable on all platforms."

Measured before and after Phase 1, both full rebuilds from a cleaned tree:

| | Warnings | CA1416 |
|---|---|---|
| Before | 5,109 | 10,128 |
| After | **45** | **0** |

**What this exposed.** The surviving 45 include **12 × CS4014** (async call not awaited) and
**8 × CA2200** (`throw ex` discarding the original stack trace) — genuine bug-risk signals that
were invisible under the noise. Worth triaging; several past bugs in this repo were threading
and exception-handling issues of exactly this shape.

**Still outstanding (low):** the `<NoWarn>` override itself. It is now harmless — CA1416 no
longer fires, and `WFO1000` is suppressed via `.editorconfig` — but it is a trap for whoever
next adds a suppression to the main `PropertyGroup` and finds it silently ignored. Fix by
appending `$(NoWarn)` in the two configuration-specific groups.

---

## Medium

### M1 — The tag namespace is unusable as release history

```
Initial-Release  Latest  LichDirect  Test_Build  windows-x64  v4.1.0.0
4.0.0.3 … 4.0.2.9   (inherited from upstream)
```

Six tags, three schemes, plus 21 inherited upstream tags. A tag literally named `Latest` is
actively harmful — GitHub already has a "Latest" concept, and the two will disagree.

**Fix:** adopt `v<MAJOR>.<MINOR>.<PATCH>` going forward (decided). Leave existing tags alone —
deleting a tag with a published release breaks download URLs people have bookmarked.

### M2 — No symbols are produced, so crash reports have no line numbers

The `Release` configuration sets `<DebugType>None</DebugType>`. `Forms/DialogException.cs` shows
users a stack trace and invites them to report it — but without PDBs, that trace has no file or
line information.

**Fix:** build with `<DebugType>portable</DebugType>`, keep PDBs **out** of the user ZIP, and
attach them to the GitHub Release as a separate `Genie-Remix-<version>-symbols.zip`. Costs
nothing for users, makes bug reports actionable.

### M3 — The auto-updater is live code pointed at the wrong repository ✅ fixed 2026-08-01

`Utility/Updater.cs` hardcoded `GenieClient/Genie4` and `GenieClient/Lamp`. It was disabled by
unhooking its menu items — but the code was still there and the constants were wrong rather than
absent.

**Rewritten, not repointed.** `Utility/RemixUpdater.cs` is a new, self-contained updater targeting
this fork, with no dependency on Lamp:

- Reads `releases/latest` from `SekmehtDR/Genie4_Remix`; pre-releases are excluded by GitHub
- Offers only strictly newer versions, so it can never downgrade
- Verifies the download against `SHA256SUMS.txt` before touching anything
- Extracts to temp first — a failure at any stage leaves the install untouched
- Copies over the install folder rather than replacing it, so user data survives

The upstream client-update paths were **deleted**: `Updater.ClientIsCurrent`, `RunUpdate`,
`UpdateToTest`, `ForceUpdate`, and `FormMain.UpdateOnStartup`, along with the orphaned
`Force Update` and `Load Test Client` menu items. `Updater.cs` now serves content downloads
(maps, scripts, plugins, art) only, and says so at the top of the file.

Update checking remains **user-initiated only** — Help → Check For Updates. There is no startup
check and no auto-update.

**Remaining (low):** the orphaned `AutoUpdate` / `AutoUpdate Lamp` / `Check Updates On Startup`
menu items still exist in the Designer and toggle `Config` properties that are hardcoded
`get => false`. They are unreachable and inert, but they are misleading dead code — worth
deleting when someone is next in `FormMain.Designer.cs`.

### M4 — No smoke test on the published artifact

Nothing verifies the ZIP actually runs. A missing satellite assembly or a broken
`runtimeconfig.json` ships silently and the first person to find out is a player.

**Fix:** in CI, extract the published ZIP and launch `Genie.exe` headlessly for a few seconds,
failing if the process exits non-zero. Crude, but it catches the catastrophic packaging failures.

### M5 — Everything lands directly on `main`

No branch protection, no PR flow, no required checks. Two commits in the recent history are
`UNDO:` / `Undo:` revert pairs, which is exactly the pattern a pre-merge check catches.

CI now exists (H1), but nothing yet *requires* it. This is a repository settings change, not a
code change, so it has to be made deliberately by the repo owner:

```powershell
# Requires the "Build (Release)" check to pass before anything merges to main.
gh api -X PUT repos/SekmehtDR/Genie4_Remix/branches/main/protection `
  -H "Accept: application/vnd.github+json" `
  -f "required_status_checks[strict]=true" `
  -f "required_status_checks[contexts][]=Build (Release)" `
  -F "enforce_admins=false" `
  -F "required_pull_request_reviews=null" `
  -F "restrictions=null"
```

`enforce_admins=false` deliberately leaves you able to push directly when you need to — the goal
is a safety net, not a bureaucracy. Note that the workflow must run **at least once** before
`Build (Release)` is selectable as a required check.

Consider it optional while you are the only committer; it becomes important the moment anyone
else contributes.

---

## Low

| # | Finding | Fix |
|---|---|---|
| L1 | `screenshots/` is in `.gitignore` but its two files are tracked — new screenshots are silently not added | ✅ Kept deliberately (README embeds those two); `.gitignore` now documents `git add -f` for adding more |
| L2 | Dead ClickOnce config in `Genie4.csproj`: `PublishUrl`, `UpdateUrl` (`clanshroud.org`, a dead domain), `BootstrapperPackage` entries for .NET Framework 2.0/3.5, `ManifestCertificateThumbprint` referencing a `.pfx` that isn't used | Delete; `GenerateManifests=false` means none of it does anything |
| L3 | `app.config` / `Genie.dll.config` is .NET Framework-era configuration carried into .NET 10 | Audit whether anything still reads it |
| L4 | No `CONTRIBUTING.md`, issue templates, or PR template | Add if you want outside contributions; skip if not |
| L5 | Binaries are unsigned, so Windows SmartScreen warns on every download | Real fix costs money (an OV/EV certificate); the cheap mitigation is telling users what to expect in the README |
| L6 | This working copy was **28 commits behind `origin/main`** at review time, still on .NET 6 | Symptom of no single source of truth; CI + tag-driven releases make the remote authoritative |
| L7 | Three build artifacts under `Plugin/obj/Debug/` plus `Resources/Thumbs.db` were **tracked in git** despite matching ignore rules — they predated those rules, so the rules never applied | ✅ Fixed — untracked via `git rm --cached`; `.gitignore` rewritten and scoped to this project |
| L8 | 12 × `CS4014` (async call not awaited) and 8 × `CA2200` (`throw ex` discards stack trace) are now visible after the CA1416 flood cleared | Triage individually; both match bug classes this repo has shipped before |

---

## Roadmap

Sequenced so each phase is independently useful and nothing is built on a broken foundation.

### Phase 1 — Make the version real ✅ **done 2026-07-31**
- [x] Reduce `Properties/AssemblyInfo.cs` to non-generated attributes only — `Plugin/AssemblyInfo.vb`
      untouched, its `2.0.0.1` is the plugin ABI version **(B1)**
- [x] `GenerateAssemblyInfo=true` in `Genie4.csproj`, with metadata moved to MSBuild properties **(B1)**
- [x] Add `Directory.Build.props` with `VersionPrefix` / `BuildNumber` / `SourceRevisionId`,
      scoped to `Genie4` so the plugin ABI version cannot be overwritten **(B1)**
- [x] Delete `<ApplicationVersion>` from `Genie4.csproj` **(B2)**
- [x] Replace README's hardcoded version and date with live GitHub badges **(B2)**
- [x] Verified: injected versions reach `Genie.dll`, `Genie.exe`, and the running title bar;
      `Plugins.dll` held at `2.0.0.1`; published build launches **(B1)**

### Phase 2 — Know that `main` builds ✅ **done 2026-07-31**
- [x] `.github/workflows/build.yml` — build on push, PR, and manual dispatch; Windows runner, .NET 10 **(H1)**
- [x] Version-stamping assertions in CI, guarding the B1 regression and the plugin ABI version **(B1)**
- [x] Package job: self-contained publish, content verification, downloadable test build **(partially M4)**
- [x] Warning count and code breakdown reported to the run summary **(H5)**
- [x] Fix the `NoWarn` override in `Genie4.csproj` and `Plugin/Plugins.vbproj`; stop suppressing
      CA1416 now that it legitimately reports 0 **(H5)**
- [ ] Enable branch protection requiring the build check — repo settings, owner's call **(M5)**

### Phase 3 — Tag-driven releases ✅ **written 2026-07-31, not yet run on GitHub**
- [x] `.github/workflows/release.yml` — triggered on `v*` tags, plus a dry-run dispatch mode **(H1)**
- [x] Derive all versions from the tag; **assert** the binary matches before packaging **(B1)**
- [x] Package as `Genie-Remix-<version>.zip`, nested under `Genie-Remix/` **(H3)**
- [x] Emit `SHA256SUMS.txt` **(H4)**
- [x] Generate the release body from `CHANGELOG.md`, with install and upgrade instructions **(H3)**
- [x] Auto-flag pre-release when the tag contains `-` **(H3)**
- [x] Smoke-test the published build before packaging **(M4)**
- [x] Refuse to publish over an existing release **(H2)**
- [x] Refuse to release from a commit that is not on `main`
- [ ] **Run a dry run, then cut `v4.1.1` for real** — the workflow is unproven on a hosted runner

### Phase 4 — Supportability polish
- [ ] Portable PDBs, shipped as a separate symbols asset **(M2)**
- [ ] Decide the updater's fate: repoint or remove **(M3)**
- [ ] README version badge driven by the GitHub API **(B2)**
- [ ] Clean up dead ClickOnce config **(L2)**
- [ ] Untrack `screenshots/` **(L1)**

---

## What was deliberately *not* recommended

Worth stating, so these don't get re-litigated:

- **A test suite.** A 9,000-line `FormMain.cs` driving live socket I/O is not economically
  unit-testable, and retrofitting tests would consume the effort better spent on the release
  pipeline. A CI build plus a launch smoke test buys most of the confidence for a fraction of
  the cost. Revisit if the script engine (`Script/`) or expression evaluator (`Script/Eval.cs`)
  gets significant new work — those *are* testable in isolation.
- **Framework-dependent builds.** Decided: stay self-contained. The 55 MB download is cheaper
  than telling DragonRealms players to install a .NET runtime.
- **Single-file publish.** Would shrink the folder to a handful of files, but breaks the
  embedded-assembly plugin loading in `Utility/EmbeddedAssembly.cs` and complicates the portable
  data-beside-the-exe model. Not worth the risk.
- **Merging upstream `GenieClient/Genie4`.** The fork has diverged past the point where routine
  merges are cheap. Cherry-pick specific upstream fixes as needed.
