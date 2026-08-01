# Versioning

> **Status: implemented** (2026-07-31). The version is derived from `Directory.Build.props` and
> overridable from the command line, verified end to end. What remains is the CI that passes
> those values from the git tag — see [RELEASE-READINESS.md](RELEASE-READINESS.md) Phase 3.

---

## The scheme

A single git tag is the source of truth for a release:

```
v<MAJOR>.<MINOR>.<PATCH>          e.g.  v4.1.1
```

Everything else is **derived** from it. Nothing is hand-edited.

| Field | Value | Example | Purpose |
|---|---|---|---|
| Git tag | `v4.1.1` | `v4.1.1` | The one thing a human chooses |
| `AssemblyVersion` | `4.1.1.0` | `4.1.1.0` | Binding identity. **Fourth component always `0`.** |
| `AssemblyFileVersion` | `4.1.1.<build>` | `4.1.1.47` | Identifies the exact CI build |
| `AssemblyInformationalVersion` | `4.1.1+<sha>` | `4.1.1+a3f19c2` | Traceability back to a commit |
| Release title | `Genie Remix 4.1.1` | | What users see |
| Release asset | `Genie-Remix-4.1.1.zip` | | Immutable, version-stamped filename |

`<build>` is the GitHub Actions run number. You never pick it, and it only ever moves forward.

### Why the fourth component is pinned to `0` in `AssemblyVersion`

Plugins bind against the client assembly. If `AssemblyVersion` changed on every CI run, every
rebuild would be a new binding identity and plugin loading would become fragile. `FileVersion`
carries the build counter instead — it is informational and safe to churn.

### Why not plain SemVer strings

The client sends its version to the DragonRealms game server:

```csharp
// Lists/Config.cs
sConnectString = "FE:GENIE /VERSION:" + My.MyProject.Application.Info.Version.ToString() + " /P:WIN_XP /XML";
```

and exposes it to user scripts:

```csharp
// Lists/Globals.cs
Add("version", My.MyProject.Application.Info.Version.ToString(), VariableType.Reserved);
```

`Application.Info.Version` is a `System.Version` — **four numeric components, no suffixes.**
A tag like `v4.2.0-beta.1` cannot be represented there. Pre-release information belongs in
`AssemblyInformationalVersion` and in the GitHub Release "pre-release" flag, never in
`AssemblyVersion`.

---

## When to bump what

This is a fork of a mature client with an installed user base. Read the increments in terms of
**what a player has to do**, not internal code churn.

| Bump | When | Player impact |
|---|---|---|
| **PATCH** (`4.1.0` → `4.1.1`) | Bug fixes, visual polish, performance. Config, scripts, maps, and plugins all keep working untouched. | Extract over the old folder, done. |
| **MINOR** (`4.1.x` → `4.2.0`) | New features, new config options, new script commands. Still backward compatible. | Same, plus something new to discover. |
| **MAJOR** (`4.x` → `5.0.0`) | A user-visible contract breaks: config format migration, plugin ABI change, dropped Genie4 compatibility, or a .NET version bump that changes install requirements. | Needs migration notes and a loud release body. |

**Default to PATCH.** Most of this fork's history is bug fixes. Reserve MINOR for genuinely new
capability (the Lich configuration tab, themes, the AutoMapper work) and MAJOR for the things
that would make a Genie4 user's existing setup stop working.

### Pre-releases

Tag `v4.2.0-rc.1`, mark the GitHub Release as a pre-release. CI produces:

- `AssemblyVersion` `4.2.0.0` — the suffix is dropped, as it must be
- `AssemblyInformationalVersion` `4.2.0-rc.1+a3f19c2` — the suffix survives here
- Asset `Genie-Remix-4.2.0-rc.1.zip`

Pre-releases do not become "Latest" on the releases page, so casual users keep getting the
stable build.

---

## The plugin assembly versions separately — leave it alone

`Plugin/AssemblyInfo.vb` declares:

```vb
<Assembly: AssemblyVersion("2.0.0.1")>
```

This is the **plugin ABI version**, not a client version, and it is intentionally decoupled.
Third-party plugins bind against `GeniePlugin` 2.0.0.1; bumping it in step with client releases
would break every existing plugin for no benefit.

**Rule:** the release version applies to `Genie4.csproj` only. `Plugins.vbproj` keeps its
hand-managed version, and it changes **only** when `IPlugin.vb` / `IHost.vb` actually change
shape — which is itself a MAJOR-bump event for the client.

---

## How it works now

**Implemented 2026-07-31.** Three files carry the whole mechanism:

### 1. `Directory.Build.props` — the source of truth

```xml
<VersionPrefix>4.1.1</VersionPrefix>   <!-- the only value a human edits -->
<VersionSuffix></VersionSuffix>        <!-- "rc.1" for pre-releases -->
<BuildNumber>0</BuildNumber>           <!-- CI run number -->
<SourceRevisionId>local</SourceRevisionId>

<!-- derived, scoped to the client project only -->
<AssemblyVersion>$(VersionPrefix).0</AssemblyVersion>
<FileVersion>$(VersionPrefix).$(BuildNumber)</FileVersion>
<InformationalVersion>$(VersionPrefix)$(_VersionSuffixPart)+$(SourceRevisionId)</InformationalVersion>
```

The derived properties are guarded by `Condition="'$(MSBuildProjectName)' == 'Genie4'"` so they
cannot leak into the plugin project.

### 2. `Genie4.csproj` — `GenerateAssemblyInfo` is now `true`

Assembly metadata moved into MSBuild properties (`AssemblyTitle`, `Product`, `Company`,
`Copyright`, `Description`). `<ApplicationVersion>` was deleted as a ClickOnce leftover.

### 3. `Properties/AssemblyInfo.cs` — reduced to what the SDK does *not* generate

Only `AssemblyTrademark`, `ComVisible`, and `Guid` remain. Re-adding any generated attribute
causes a `CS0579 duplicate attribute` build failure — which is the desired behaviour, because
the collision is loud rather than silent.

### What CI passes

```powershell
dotnet publish Genie4.csproj -c Release -r win-x64 --self-contained true `
  -p:VersionPrefix=4.1.1 `
  -p:BuildNumber=$env:GITHUB_RUN_NUMBER `
  -p:SourceRevisionId=$shortSha
```

Command-line `-p:` values are MSBuild *global* properties, so they override the defaults and the
derived properties recompute from them automatically. Verified end to end:

| Invocation | AssemblyVersion | FileVersion | InformationalVersion |
|---|---|---|---|
| `dotnet build` (no args) | `4.1.1.0` | `4.1.1.0` | `4.1.1+local` |
| `-p:VersionPrefix=9.9.9 -p:BuildNumber=77 -p:SourceRevisionId=deadbee` | `9.9.9.0` | `9.9.9.77` | `9.9.9+deadbee` |
| `-p:VersionPrefix=4.2.0 -p:VersionSuffix=rc.1 -p:BuildNumber=12 -p:SourceRevisionId=a3f19c2` | `4.2.0.0` | `4.2.0.12` | `4.2.0-rc.1+a3f19c2` |

`Plugins.dll` stayed at `2.0.0.1` in every case, and a published build reported
`[Not connected] - Genie Remix 4.1.1.0` in its title bar — confirming the version reaches
`Application.Info.Version`, and therefore the game-server handshake and `$version`.

---

## Historical: the blocker this replaced

> Resolved. Kept because the failure mode is subtle and worth recognising if it recurs.

Both `Genie4.csproj` and `Plugin/Plugins.vbproj` used to set:

```xml
<GenerateAssemblyInfo>false</GenerateAssemblyInfo>
```

with the version hardcoded in `Properties/AssemblyInfo.cs`:

```csharp
[assembly: AssemblyVersion("4.1.0.0")]
[assembly: AssemblyFileVersion("4.1.0.0")]
```

**Consequence: MSBuild version properties were silently ignored.** A publish run with
`-p:Version=4.1.0.1 -p:AssemblyVersion=4.1.0.1 -p:FileVersion=4.1.0.1` still produced a binary
stamped `4.1.0.0`, with no warning and no error. Any CI pipeline built on that would have
happily published a mislabelled release.

The version was duplicated in three places that could drift independently — and had:
`AssemblyInfo.cs` (`4.1.0.0`), `Genie4.csproj` `<ApplicationVersion>` (`4.1.0.0`), and `README.md`
prose (`4.0.3.2`, stale).

---

### The dead end worth remembering

The obvious minimal fix — keep `GenerateAssemblyInfo=false` and re-enable only the three
version attributes:

```xml
<GenerateAssemblyInfo>false</GenerateAssemblyInfo>
<GenerateAssemblyVersionAttribute>true</GenerateAssemblyVersionAttribute>
<GenerateAssemblyFileVersionAttribute>true</GenerateAssemblyFileVersionAttribute>
```

**does not work, and fails in a worse way than doing nothing.** `GenerateAssemblyInfo=false`
gates the entire `GetAssemblyAttributes` target in the SDK, so the per-attribute switches never
run. The build succeeded and produced a binary stamped **`0.0.0.0`** — no version at all.

If a future change reintroduces `GenerateAssemblyInfo=false` on `Genie4.csproj`, this is the
symptom to expect.

### Verifying it took

After implementing, the check that actually matters — a version injected on the command line
must appear in the binary:

```powershell
dotnet publish Genie4.csproj -c Release -r win-x64 --self-contained true `
  -p:Version=9.9.9 -p:FileVersion=9.9.9.9 -o .\verify
(Get-Item .\verify\Genie.dll).VersionInfo | Select-Object FileVersion, ProductVersion
# Must print 9.9.9.9 — if it prints 4.1.x.x, step 1 or 2 was not applied.
```

Run this once by hand after the change, and keep it as a CI assertion so the failure mode can
never come back silently.
