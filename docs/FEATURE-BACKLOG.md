# Feature backlog — settings, discoverability and defaults

Companion to [BUG-BACKLOG.md](BUG-BACKLOG.md). That document covers things that are *broken*;
this one covers things that *work as written* but are confusing, undiscoverable, unsafe by
default, or missing a capability players reasonably expect.

Analysis date 2026-08-01, against `ebdabd9` (Release 4.2.0).

The through-line: **Genie Remix has roughly 110 client commands and roughly 65 settings, and
almost none of them are discoverable from inside the client.** The settings GUI covers Lich and
the list editors (highlights, triggers, macros, aliases, subs, gags, variables, windows) and
nothing else. Everything else is typed blind into `#config`, which silently accepts wrong input,
silently discards unknown input, and silently mutates a setting when you try to read it.

The good news is that the model to copy already exists in this fork. The **Lich tab** in
`Forms/FormConfig` is a proper settings panel — labelled fields, Browse buttons, a *Test Paths*
button that tells you whether what you typed actually works. Extending that pattern is most of
this document.

## Effort

| | |
|---|---|
| **S** | Contained change, hours |
| **M** | One feature area, needs a live session to verify |
| **L** | New UI surface or a format/behaviour change with a migration story |

---

## Status

This is a living document — see *Living documents* in [CLAUDE.md](../CLAUDE.md). Update entries in
the same commit as the code change.

| Marker | Meaning |
|---|---|
| **Open** | Not yet acted on. The default. |
| **✅ Done `<version/date>`** | Shipped. Entry stays, with a line on what was actually built. |
| **⚠️ Partial** | Partly built. Entry states exactly what remains. |
| **❌ Declined** | Decided against. Entry stays, with the reasoning — this is the useful part. |

IDs are never reused. Next free ID: **FEAT-021**.

## Summary

| ID | Feature | Effort | Status |
|---|---|---|---|
| **Settings safety** | | | |
| [FEAT-001](#feat-001) | `#config <name>` shows the value instead of blanking it | S | Open |
| [FEAT-002](#feat-002) | Reject invalid values instead of silently coercing to `false` | S | Open |
| [FEAT-003](#feat-003) | Validate and range-check numeric settings | S | Open |
| [FEAT-004](#feat-004) | Report settings that fail to load instead of discarding them | S | Open |
| [FEAT-005](#feat-005) | Offer to persist a setting rather than "Don't forget to #save config" | S | Open |
| **Settings coverage** | | | |
| [FEAT-006](#feat-006) | Build the general Settings tab (`UCSettings` is an empty stub) | L | Open |
| [FEAT-007](#feat-007) | Generate `#config`'s list from one table so it cannot drift | M | Open |
| [FEAT-008](#feat-008) | Remove or implement the dead settings | S | Open |
| [FEAT-009](#feat-009) | Rename `maxrowbuffer` and expose the real scrollback limit | M | Open |
| **Defaults that surprise** | | | |
| [FEAT-010](#feat-010) | Make the idle auto-`quit` opt-in, and stop it repeating | S | Open |
| [FEAT-011](#feat-011) | Surface the keepalive command that is sent invisibly | S | Open |
| **Discoverability** | | | |
| [FEAT-012](#feat-012) | Ship help content, and make `#help` say something when it fails | M | Open |
| [FEAT-013](#feat-013) | Command discovery: `#commands`, and tab-completion in the input bar | M | Open |
| **Multi-character play** | | | |
| [FEAT-014](#feat-014) | Let profiles scope highlights, triggers, subs and gags | M | Open |
| [FEAT-015](#feat-015) | Protect saved passwords with DPAPI instead of a derivable key | M | Open |
| **Paths and scripts** | | | |
| [FEAT-016](#feat-016) | Fix absolute-path detection so UNC and network paths work | S | Open |
| [FEAT-017](#feat-017) | Support a list of script extensions rather than one plus hardcoded `.js` | S | Open |
| [FEAT-018](#feat-018) | Resolve the Lua question — it is advertised but absent | S | Open |
| **Automapper** *(requested by Tirost)* | | | |
| [FEAT-019](#feat-019) | Button to centre the map on the room you are in | S | ✅ Done 4.2.2 |
| [FEAT-020](#feat-020) | Automapper window remembers its position and size | S–M | ✅ Done 4.2.2 |

---

## Settings safety

### FEAT-001
**`#config <name>` shows the value instead of blanking it**
`Core/Command.cs:1495`, `Lists/Config.cs:573`

`#config autolog` — the obvious way to ask "what is autolog set to?" — calls
`SetSetting("autolog")` with `sValue` defaulting to `""`. Every boolean case tests for
`on`/`true`/`1` and falls through to `default:` — so **asking turns the setting off**.

It is worse for `scriptextension` (`Config.cs:665`), where `ScriptExtension = sValue ?? "cmd"`
takes the empty string (not null), leaving the extension blank. Script name resolution then
appends a bare `.` and stops finding any script.

*What it does:* make the no-value form a read, printing `autolog = True`. Mutating a setting
requires a value.

*Why it matters to players:* right now the single most natural thing to type silently breaks the
thing you were asking about, and nothing tells you. `#config scriptextension` alone stops every
script from loading by name until you notice and set it back.

---

### FEAT-002
**Reject invalid values instead of silently coercing to `false`**
`Lists/Config.cs:600`–`1460` (every boolean case)

Every boolean setting is parsed the same way: `on`, `true` or `1` means true, and **everything
else** — including `off`, `yes`, `enabled`, `TRUE ` with a trailing space, or a typo like `ture`
— means false. There is no error and no echo of what was actually stored.

*What it does:* accept an explicit off-set (`off`/`false`/`0`/`no`), reject anything else with
`Not a valid value for 'autolog'. Use on or off.`, and echo the stored value back on success.

*Why it matters to players:* `#config autolog yes` reads as "turn logging on" and turns it off.
The player finds out days later when they go looking for a log that was never written. The same
pattern applies to `highlightsenabled`, `gagsenabled`, `reconnect` and `triggeroninput` — settings
where being silently off is exactly the failure you would not think to check.

---

### FEAT-003
**Validate and range-check numeric settings**
`Lists/Config.cs:656`, `:751`, `:771`, `:781`, `:1462`, `Utility/Utility.cs:627`

Numeric settings run through three different parsers with three different failure behaviours:

- `Utility.StringToInteger` returns **-1** for garbage *and* for zero *and* for negatives
- `Conversions.ToInteger(Utility.StringToDouble(…))` is used for the timeouts
- `Conversions.ToInteger(sValue)` is used for `historysize` and throws outright on garbage

Nothing range-checks. `AutoMapperAlpha` (`Config.cs:87`) is the only setting in the file that
clamps its input, and it shows how little code it takes.

The dangerous cases are the ones where a bad value silently *disables a safety feature*:
`#config servertimeout abc` yields 0, and `CheckServerIdleTime` returns immediately when the value
is 0 — the connection watchdog is off. Same for `usertimeout`.

*What it does:* one numeric parser with a declared range per setting, an error on anything
outside it, and `0` documented as "disabled" where that is the intent.

*Why it matters to players:* a typo in a timeout silently turns off the thing that reconnects you
when the game drops. You find out by logging back in to a corpse.

---

### FEAT-004
**Report settings that fail to load instead of discarding them**
`Lists/Config.cs:551`–`557`

```csharp
try { SetSetting(oArgs[1].ToString(), oArgs[2].ToString(), false); }
catch { /* Settings! We got bad settings here! See? No one cares. */ }
```

Every failure while reading `settings.cfg` is swallowed with no record. An unrecognised key, a
malformed value, a line from a different Genie version — all silently revert to the built-in
default. The line count check is also exact (`oArgs.Count == 3`), so a line whose value did not
parse into three arguments is skipped before it ever reaches the `try`.

*What it does:* collect failures during load and print a summary — `3 settings in settings.cfg
could not be applied: <name> (line 12), …` — after startup completes.

*Why it matters to players:* a hand-edited `settings.cfg` with one bad line looks like it worked.
The setting just isn't in effect, and there is no way to tell which one or that anything happened
at all. This is also the mechanism that makes FEAT-008's phantom `pluginrepo` invisible.

---

### FEAT-005
**Offer to persist a setting rather than "Don't forget to #save config"**
`Lists/Config.cs:1486`

Every successful `#config` echoes `Don't forget to #save config`. Settings are in-memory until
the player runs a separate command, and nothing warns on exit that unsaved changes exist.

*What it does:* either write through on change, or track a dirty flag and prompt on exit. If
write-through is too aggressive for a client where people experiment mid-session, a
`#config autosave on` switch plus an exit prompt covers both preferences.

*Why it matters to players:* tuning a setting during a hunt, getting it right, closing the client,
and finding it reverted next session. The reminder text is printed *after* the change, in a
scrolling combat window, where it is missed exactly when it matters.

---

## Settings coverage

### FEAT-006
**Build the general Settings tab**
`Forms/ConfigPanels/UCSettings.cs`, `Forms/FormConfig.designer.cs:163`–`171`

`UCSettings` exists as a class with a toolbar, Refresh/Load/Save buttons and an Apply button —
and `RefreshSettings()` and `ApplyChanges()` have **empty bodies**. It is also not referenced
anywhere in `Forms/`; the tab was never added to `FormConfig`. The config dialog has tabs for
Windows, Highlights, Triggers, Subs, Ignores, Aliases, Macros, Vars and Lich, and no general
settings tab at all.

So roughly 55 settings — script character, separator, prompt, timeouts, directories, buffer
sizes, reconnect behaviour, sound, links, images, history — have **no GUI whatsoever**.

*What it does:* fill in the panel, grouped (Connection / Scripts / Display / Directories /
Logging), with the right control per setting: checkboxes for booleans, numeric fields with the
range enforced, Browse buttons for directories, and a description line per setting.

*Why it matters to players:* this is a client for DragonRealms players, not developers. Right now
a player who wants to change the prompt or move their log directory has to know that `#config`
exists, know the exact key name, know the accepted value format, and know to run `#save config`
after. The Lich tab already proves the fork can build this well — it has Browse buttons and a
*Test Paths* button that verifies the values before you commit them.

*Effort (L):* the panel is the work. Doing FEAT-007 first makes it mostly generated.

---

### FEAT-007
**Generate `#config`'s list from one table so it cannot drift**
`Core/Command.cs:2900`–`2958`, `Lists/Config.cs:444`–`533`, `:573`–`1490`

Three hand-maintained lists have to agree and do not: the `case` labels in `SetSetting` (what can
be set), the `WriteLine` calls in `Save` (what gets written), and the `EchoText` calls in
`ListSettings` (what `#config` shows). Diffing them:

**Settable but never listed — 9 settings a player cannot discover or inspect:**
`artrepo`, `cmdpath`, `licharguments`, `lichpath`, `lichport`, `lichserver`, `lichstartpause`,
`rubypath`, `scriptmatchtimeout`.

**Listed and saved but not settable — 1 phantom setting:** `pluginrepo`. `#config` displays it
(`Command.cs:2940`) and `Save` writes it to `settings.cfg` (`Config.cs:490`), but `SetSetting` has
no case for it. Setting it throws `Config pluginrepo was not recognized`; loading it hits the same
throw, which `Load` swallows (FEAT-004). It is written on every save and discarded on every
startup — a setting that can never be set and never persists.

*What it does:* one declarative table — key, type, default, range, description, whether it
persists — with `SetSetting`, `Save`, `ListSettings` and the FEAT-006 panel all driven from it.

*Why it matters to players:* what `#config` prints is currently the only documentation of what
`#config` accepts, and it is wrong in both directions. A player following it will try to set
something that does not exist, and will never learn about the nine that do.

---

### FEAT-008
**Remove or implement the dead settings**
`Lists/Config.cs:68`–`70`, `:27`, `Forms/FormMain.cs:8925`–`8932`, `:9174`

```csharp
public bool CheckForUpdates { get => false; set { } }
public bool AutoUpdate      { get => false; set { } }
public bool AutoUpdateLamp  { get => false; set { } }
```

All three are hardwired off with a discarding setter — and all three are still **live menu items**.
`autoUpdateToolStripMenuItem_Click` does `Config.AutoUpdate = !Config.AutoUpdate;` then
`Checked = Config.AutoUpdate`, so the checkbox never ticks no matter how many times it is clicked.
`AutoUpdateLamp` is still passed into `Updater.UpdateMaps` / `UpdatePlugins` / `UpdateScripts` /
`UpdateArt`, always as `false`.

`iScriptTimeout` (`#config scripttimeout`) is a fourth: parsed, saved, echoed by `#config`, and
never read by anything in `Script/`. See BUG-BACKLOG GRX-017.

*What it does:* delete the menu items and settings that do nothing. Where the intent survives —
these relate to the deliberate decision that client updates are user-initiated only — say so in
the menu rather than leaving a toggle that fights back.

*Why it matters to players:* clicking a checkbox that refuses to tick reads as a broken client.
It also actively misleads: a player who wants automatic content updates sees a setting for it,
turns it "on", and gets nothing.

---

### FEAT-009
**Rename `maxrowbuffer` and expose the real scrollback limit**
`Lists/Config.cs:20`, `Forms/Components/ComponentRichTextBox.cs:346`, `:148`

`maxrowbuffer` maps to `iBufferLineSize`, default **5**. It is not a buffer size — it is the
number of pending lines that triggers a repaint:

```csharp
if (bNoCache || m_oRichTextBuffer.Lines.Length >= Config.iBufferLineSize) InvokeEndUpdate();
```

The actual scrollback ceiling is `m_iMaxBufferSize = 500000` characters, hardcoded in the control
and set again in `FormSkin.Designer.cs:76`, with **no `#config` key and no GUI**.

So the setting named "max row buffer" controls paint batching, and the thing a player means by
"buffer" cannot be configured at all. A player who reads the name and sets
`#config maxrowbuffer 5000` expecting more scrollback gets a client that only repaints every 5000
lines — indistinguishable from a freeze.

*What it does:* rename to something honest (`repaintbatchlines`), keep `maxrowbuffer` as a
deprecated alias that warns, and add a real `scrollbackchars` (or `scrollbacklines`) setting wired
to `MaxBufferSize`.

*Why it matters to players:* scrollback depth is one of the few display settings people genuinely
want to change — it is how you scroll back to find what killed you. Today it is unreachable, and
the setting that looks like it does the job makes the client appear to hang.

---

## Defaults that surprise

### FEAT-010
**Make the idle auto-`quit` opt-in, and stop it repeating**
`Lists/Config.cs:45`–`46`, `Forms/FormMain.cs:7563`–`7605`

Default configuration, active on every install:

```csharp
public int    iUserActivityTimeout = 300;      // seconds
public string sUserActivityCommand = "quit";
```

After 300 s with nothing sent to the game, the client beeps, flashes the window and prints
`GENIE HAS FLAGGED YOU AS IDLE. PLEASE RESPOND!`. 60 s later it sends **`quit`**.

Two things make this sharper than it looks:

1. `m_bCheckUserResponse` is never cleared after the `quit` fires (`FormMain.cs:7580`), and
   `TimerReconnect` ticks every second. Once `iDiff` passes the threshold it stays past it, so
   `quit` is re-sent **once per second** for as long as the session lasts.
2. Activity is measured on text *sent* (`Game.cs:547`), so a running script counts. But a player
   reading LNet, watching a room, or waiting out a long roundtime does not.

*What it does:* default `usertimeoutcommand` to empty (warn only), require the player to opt into
an auto-command, clear the response flag after firing so it happens once, and name the setting in
the warning text so the player can find and change it.

*Why it matters to players:* being logged out of DragonRealms unexpectedly is not free — you can
lose a hunting spot, drop out of a group, or leave a character somewhere unsafe. A default that
disconnects you for reading quietly for six minutes is not a default anyone would choose, and
nothing in the client tells you it exists or where it lives.

---

### FEAT-011
**Surface the keepalive command that is sent invisibly**
`Lists/Config.cs:43`–`44`, `Forms/FormMain.cs:7555`–`7560`

Every 180 s without server traffic, the client sends `fatigue` via `SendRaw` — which does not echo
it. The player has no indication that their client is issuing commands on their behalf.

*What it does:* echo the keepalive to the main window (or at minimum make it echo-able via a
setting), and expose both `servertimeout` and `servertimeoutcommand` in the FEAT-006 panel.

*Why it matters to players:* an unexplained `fatigue` result appearing in the log looks like input
lag or a stuck script. Players also share logs when reporting problems, and an invisible command
in the transcript sends them chasing the wrong thing.

---

## Discoverability

### FEAT-012
**Ship help content, and make `#help` say something when it fails**
`Core/Command.cs:2779`–`2824`, `Forms/FormMain.cs:2449`

`ShowHelp` resolves a topic to `<install>\Help\<topic>.txt` and prints it. `Help/` is created
empty at startup and **no help content ships in the repo or the release package**.

The failure path prints nothing at all: the read is guarded by `if (fi.Exists && fi.Length > 0)`,
so a missing topic falls straight through. The `catch (FileNotFoundException)` that would have
printed `Topic does not exist.` is unreachable behind that guard.

So on a fresh install, `#help` produces **silence**.

*What it does:* ship a `Help/` tree covering the command set, and print an explicit
`No help topic '<name>'. Try #help for the index.` when a topic is missing.

*Why it matters to players:* `#help` is the first thing anyone types in an unfamiliar client. Getting
nothing back — not an error, nothing — reads as a broken install, and it sends new players to
outside documentation for a fork whose behaviour has diverged from it.

---

### FEAT-013
**Command discovery: `#commands`, and tab-completion in the input bar**
`Core/Command.cs`

There are roughly 110 command keywords in the `ParseCommand` switch. There is no command that
lists them. The individual list commands (`#highlight`, `#alias`, `#var` with no arguments) do
print their contents, so the convention exists — it just has no top level.

*What it does:* add `#commands` printing the verbs grouped by area, make `#help <verb>` fall back
to a one-line usage string when no help file exists, and add tab-completion for `#`-prefixed
commands in the input bar.

*Why it matters to players:* today the only way to learn what the client can do is to read
someone else's scripts or the upstream Genie documentation, which no longer matches this fork.
Tab-completion in particular converts "I have to remember the exact word" into "I can find it",
which is the difference between a feature existing and a feature being used.

---

## Multi-character play

### FEAT-014
**Let profiles scope highlights, triggers, subs and gags**
`Forms/FormMain.cs:8490`–`8575`

`LoadProfileSettings` loads a default set from `Config\` and then a per-profile override from
`Config\Profiles\<name>\` — but only for **variables, macros, aliases and classes**. Highlights,
triggers, substitutes and gags load from the shared `Config\` directory only.

*What it does:* extend the same two-stage load (defaults, then per-profile overlay) to the
remaining four list types.

*Why it matters to players:* most DragonRealms players run several characters, and the lists that
most need to differ per character are exactly the four that cannot. A Bard's spell triggers firing
on a Barbarian are noise at best and a scripted misfire at worst — so people work around it by
keeping separate install folders and copying config between them by hand.

---

### FEAT-015
**Protect saved passwords with DPAPI instead of a derivable key**
`Forms/FormMain.cs:8436`–`8440`, `Utility/Utility.cs:212`–`249`

Saved passwords are Rijndael-encrypted with the key `"G3" + <account name in caps>` — and the
account name is stored in plaintext in the same profile XML. Anyone with the file can derive the
key from the file. It is obfuscation presented as encryption.

*What it does:* re-wrap with `System.Security.Cryptography.ProtectedData`
(`DataProtectionScope.CurrentUser`), which ties the secret to the Windows account and requires no
key management. Read the old format, write the new one, so existing profiles migrate on first load.

*Why it matters to players:* portable mode means the whole folder gets moved between machines,
onto USB sticks, into cloud-synced folders, and sometimes into a zip that gets shared for
troubleshooting. Players reasonably assume "save password" means the password is protected. It is
not, and a shared config folder currently means a shared account.

*Caveat:* DPAPI is per-Windows-account by design, so a saved password will not follow the folder
to another machine. That is the correct trade, but it changes portable-mode behaviour and needs to
be called out in the changelog and in the checkbox's tooltip.

---

## Paths and scripts

### FEAT-016
**Fix absolute-path detection so UNC and network paths work**
`Lists/Config.cs:106`, `:142`, `:177`, and the other directory properties

Every directory property decides whether a path is absolute with `sScriptDir.Contains(":")`. A
UNC path — `\\nas\genie\Scripts` — has no colon, so it is treated as relative and appended to the
install directory, producing `C:\Genie\\\nas\genie\Scripts`.

`LocalDirectory.ValidateDirectory` (`LocalDirectory.cs:38`) already does this correctly with
`Path.IsPathRooted`. The two disagree, so the setting validates successfully and then resolves to
somewhere else.

*What it does:* use `Path.IsPathRooted` everywhere, matching the validator.

*Why it matters to players:* shared scripts and maps on a home NAS is a normal setup for someone
running several machines. Today it silently resolves to a garbage path — and because the validator
uses the correct rule, the client reports the directory as found while the client reads from
somewhere else entirely.

---

### FEAT-017
**Support a list of script extensions rather than one plus hardcoded `.js`**
`Lists/Config.cs:82`, `Core/Command.cs:2100`, `Script/Script.cs:1986`

`ScriptExtension` is a single string, default `cmd`. Name resolution appends it, with `.js`
special-cased alongside:

```csharp
if (!sFile.ToLower().EndsWith($".{Config.ScriptExtension}") && !sFile.ToLower().EndsWith(".js"))
    sFile += $".{Config.ScriptExtension}";
```

*What it does:* make it an ordered list (`cmd;gsl;js`), tried in order, with `.js` participating
as a normal entry instead of a hardcoded exception.

*Why it matters to players:* script collections downloaded from different authors use different
extensions. Today running a mixed collection means renaming files or flipping the setting between
runs — and flipping it changes `$scriptname` for every other script at the same time.

---

### FEAT-018
**Resolve the Lua question — it is advertised but absent**
`Script/LUAScript.cs`, `Script/JavaScript.cs`

Both files are **entirely commented out** — 214 and 218 lines with one live line each. Real
JavaScript support lives in `Script.cs` via Jint (`InitJintEngine`, `Script.cs:694`). There is no
Lua implementation anywhere in the tree, and no `lua` case in the script-function dispatch.

`CLAUDE.md` currently describes the repo as having "JavaScript (Jint) and Lua backends", which is
not accurate and will send the next person looking for something that is not there.

*What it does:* delete both dead files, correct the `CLAUDE.md` description, and decide
explicitly whether Lua is wanted. If it is, it is a real feature with a real dependency, not a
file to uncomment — the commented code targets `LuaInterface`, which is long unmaintained.

*Why it matters to players:* only indirectly — but a player who reads that Lua is supported and
writes a Lua script gets a silent failure. Being honest about the supported script languages
costs nothing and prevents wasted effort.

---

## Automapper

Both requested by **Tirost**, 2 Aug 2026. Feasibility checked against the current tree; both are
straightforward, and neither needs new plumbing invented.

### FEAT-019
**Button to centre the map on the room you are in**
`Mapper/MapForm.cs:1973` (`CheckScrollTo`), `Mapper/MapForm.Designer.cs` (toolbar)

> *"Button in automapper window to center the room my character occupies in the window, when
> there are scroll bars on the automapper window."*

**Feasible, small.** The centring maths already exists and is already correct —
`CheckScrollTo(NodeX, NodeY)` converts node coordinates through the current scale and offset,
accounts for scrollbar width, and sets `PanelBase.AutoScrollPosition`.

The one thing it deliberately does *not* do is what this request asks for:

```csharp
if (iScrollX == 0 && iScrollY == 0)
    return;                     // already visible -- do nothing
```

It is "scroll into view if off-screen", not "centre". A button needs an unconditional variant that
centres whether or not the room is currently visible — the same formula with the early-out and the
inside-the-viewport branches skipped.

The current room is tracked in `m_CurrentNode` (`MapForm.cs:52`), and there is a working call site
to copy from at `MapForm.cs:2190`. The toolbar in `MapForm.Designer.cs` already carries ~18
buttons, so adding one is routine.

*Why it matters to a player:* on a big zone the map is far larger than the window, and after
panning around — or after the window is resized smaller — finding yourself again means dragging
until you spot the "HERE" marker. One click to recentre is the difference between the map being a
reference and being a puzzle.

*Watch out for:* `m_CurrentNode` can be `null` (`MapForm.cs:422`, `:683` both clear it), and a node
can have no `Position` (unplaced rooms are skipped on save at `:953`). The button should be
disabled or a no-op in both cases rather than throwing.

**Status: ✅ Done 4.2.2.** Added a text-labelled `Center` button to the right-aligned view group
next to the zoom controls, and `CenterOnCurrentRoom()` in `MapForm`. The shared maths was pulled
out into `NodeToPanel` and `CenterScrollFor` so the new path and `CheckScrollTo` cannot drift
apart. `CheckScrollTo` keeps its early-out — it exists to follow movement without yanking the view
around, which is the opposite of what an explicit button should do. Null and unplaced nodes no-op
as the entry required.

Text label rather than an icon: every other button on that toolbar pulls its image from the
`.resx`, and adding one means editing that resource for no functional gain.

**Verified on a live session** (Agan, zone Dirge, 91 maps loaded). Located the button through UI
Automation and invoked it after right-drag panning the map away in each direction. The toolbar
region was pixel-identical across the before/after captures — proving the same window was
sampled — while the map area changed, proving it scrolled. On the screenshot the "HERE" marker
(gold border, X glyph) lands at ≈(245, 218) in a viewport centred on ≈(254, 230): centred to
within about ten pixels.

---

### FEAT-020
**Automapper window remembers its position and size**
`Mapper/AutoMapper.cs:168`–`178`, `Forms/FormMain.cs:2467` / `:2733`

> *"Automapper window remembers its layout position when loaded like other windows."*

**Feasible, and the diagnosis is more specific than "persistence is missing".** Position is not
merely unsaved — it is **actively overwritten every time the window is shown**:

```csharp
if (!m_Form.Visible)
{
    if (!Information.IsNothing(parent))
    {
        m_Form.Top = 0;
        m_Form.Height = parent.ClientHeight - SystemInformation.Border3DSize.Height * 2;
        m_Form.Left = clientSize.Width / 2 - SystemInformation.Border3DSize.Width;
        m_Form.Width = clientSize.Width - ... - m_Form.Left;
    }
    m_Form.Show();
}
```

That pins it to the right-hand half of the client area at full height, unconditionally, on every
open. It compounds with `MapForm_FormClosing` (`MapForm.cs:200`), which cancels the close and just
sets `Visible = false` — so the form instance survives, keeps whatever geometry you gave it, and
then has it thrown away the moment you reopen it.

So the work is two parts, and the second is the one that actually makes it stick:

1. Persist geometry alongside the existing windows. `FormMain.SaveXMLConfig` / `LoadXMLConfig`
   already store `Left`/`Top`/`Width`/`Height` per window under keys like `Genie/Windows/Main`,
   `Genie/Windows/Game` and `Genie/Windows/Window<n>` — a `Genie/Windows/Mapper` section follows
   the established pattern exactly. The mapper currently appears nowhere in either method.
2. **Make the hardcoded placement a first-run default**, applied only when no saved geometry
   exists. Without this, step 1 has no visible effect.

*Why it matters to a player:* the mapper is a window you arrange once to suit your screen. Having
it jump back to half-width-right-hand-side every time you reopen it is a small irritation repeated
every session, and it is the one window in the client that behaves this way.

*Watch out for:* `MapForm` is an **MDI child** (`m_Form.MdiParent = parent`), so coordinates are
relative to the MDI client area, not the desktop. Restored geometry still needs clamping so a
window saved against a larger main window cannot end up entirely off the visible client area. The
`Dock` toggle (`MapForm.cs:2414`) is separate state and should probably be saved with it, or
explicitly left out and noted.

**Status: ✅ Done 4.2.2.** Both halves, as the entry predicted were needed:

1. `FormMain.SaveXMLConfig` / `LoadXMLConfig` now carry a `Genie/Windows/Mapper` section with
   `Left`/`Top`/`Width`/`Height`, alongside the existing per-window entries.
2. `AutoMapper.Show` places the window **once per session** instead of on every show. Saved bounds
   win when present; the original right-half placement remains as the first-run default. Without
   this, step 1 would have had no visible effect at all.

Geometry is handed to `AutoMapper.SetSavedBounds` rather than applied directly, because the window
may not exist yet when the layout loads, and `AutoMapper` is what decides between saved bounds and
the default. `TryGetBounds` refuses to report geometry before the window has been placed, so a
session that never opened the mapper cannot write zeros into the layout and strand it in the corner.
Restored bounds are clamped against the current client size with a minimum of 200×150.

**`Dock` was deliberately left out** — it is a different kind of state (a display mode, not
geometry) and folding it in would mean a docked mapper silently reopening docked with no obvious
way back. Worth its own decision rather than a side effect of this one.

**Verified on a live session.** Moved the mapper to 40,30 at 520×420, ran `#save layout`, and
confirmed `default.layout` held `Left=40 Top=30 Width=520 Height=420`. Closed the client fully,
relaunched, reopened the mapper: it came back at 520×420 rather than the right-half default. It
also held that geometry across connecting a character and through the window being recreated on
map load.

---

## Suggested order

**First — cheap, high-visibility, low risk.** FEAT-001, FEAT-002, FEAT-003 and FEAT-005 together
turn `#config` from a trap into something that answers questions and refuses bad input. FEAT-008
and FEAT-018 delete things that actively mislead. FEAT-016 is a one-line correctness fix. None of
these change a file format.

**Second — the structural one.** FEAT-007 (single settings table) is the keystone: it fixes the
drift, and it makes FEAT-006 (the Settings tab) mostly generated rather than hand-built. Doing
FEAT-006 without FEAT-007 means maintaining a fourth hand-written list.

**Third — the defaults and the gaps.** FEAT-010 needs a decision about changing a shipped default,
which is a judgement call rather than a technical one. FEAT-012 and FEAT-013 need content written,
not just code. FEAT-014 and FEAT-015 are the two that meaningfully change what the client can do
for someone running more than one character.
