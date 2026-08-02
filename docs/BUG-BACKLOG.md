# Bug backlog — Genie Remix core client

Full-codebase analysis, 2026-08-01, against `ebdabd9` (Release 4.2.0). Scope is the **Genie
client itself** — the Lich connection work landed in 4.2.0 is deliberately excluded except where
a defect sits on the same code path.

There is no test suite. Every entry below was established by reading the code; the ones marked
**Verified** were additionally confirmed by running the check described. Nothing here has been
reproduced against a live DragonRealms session yet — that is the next step for anything moved
into a release.

## Severity

| | |
|---|---|
| **Critical** | Loses player data, or leaves the client silently broken with no error |
| **High** | A documented feature does not work, or the client hangs / degrades badly |
| **Medium** | Wrong behaviour in a real scenario, with a workaround or a narrow trigger |
| **Low** | Latent, cosmetic, or only reachable through unusual configuration |

## Risk = risk of *fixing*

| | |
|---|---|
| **Low** | Localised change, obvious correct behaviour, hard to regress |
| **Medium** | Touches a hot path or a shared contract; needs a live session to verify |
| **High** | Changes a user-facing format or the wire protocol; needs a migration story |

---

## Status

This is a living document — see *Living documents* in [CLAUDE.md](../CLAUDE.md). Update entries in
the same commit as the code change.

| Marker | Meaning |
|---|---|
| **Open** | Not yet acted on. The default. |
| **✅ Fixed `<version/date>`** | Resolved. Entry stays, with a line on what the fix actually was. |
| **⚠️ Partial** | Partly addressed. Entry states exactly what remains. |
| **❌ Not a defect** | Disproved. Entry stays, with the evidence. |

IDs are never reused. Next free ID: **GRX-024**.

## Summary

| ID | Title | Severity | Risk | Status |
|---|---|---|---|---|
| [GRX-001](#grx-001) | Server text is decoded as UTF-8; DragonRealms sends Latin-1 | Critical | Medium | Open |
| [GRX-002](#grx-002) | Case-insensitive highlights are destroyed by a save/reload cycle | Critical | Medium | Open |
| [GRX-003](#grx-003) | Closing the window and answering "No" kills every trigger for the session | Critical | Low | Open |
| [GRX-004](#grx-004) | Every config save deletes the file first, then writes | Critical | Low | Open |
| [GRX-005](#grx-005) | Script keyword regex built with `&` instead of `|` | High | Low | ✅ Fixed 2026-08-02 |
| [GRX-006](#grx-006) | `#plugin` from a script crashes when a new-ABI plugin is installed | High | Low | Open |
| [GRX-007](#grx-007) | `.Result` on an async command can deadlock the UI thread permanently | High | Low | Open |
| [GRX-008](#grx-008) | Incoming images wipe the player's clipboard | High | Medium | Open |
| [GRX-009](#grx-009) | Output buffer is never trimmed while the player is scrolled up | High | Medium | Open |
| [GRX-010](#grx-010) | Output freezes while the mouse button is held in the game window | Medium | Low | Open |
| [GRX-011](#grx-011) | A trailing `\|` in the monster ignore list zeroes `$monstercount` | Medium | Low | Open |
| [GRX-012](#grx-012) | Overlapping highlights: the shortest match wins | Medium | Low | Open |
| [GRX-013](#grx-013) | Room window silently skips updates under lock contention | Medium | Medium | Open |
| [GRX-014](#grx-014) | Autolog reopens the log file for every line, and drops lines silently | Medium | Low | Open |
| [GRX-015](#grx-015) | Connect paths catch only `SocketException` | Medium | Low | Open |
| [GRX-016](#grx-016) | Concurrent `BeginSend` gives no ordering guarantee | Medium | Medium | Open |
| [GRX-017](#grx-017) | `#config scripttimeout` does nothing; `waitfor` has no timeout | Medium | Medium | Open |
| [GRX-018](#grx-018) | Map files are written non-atomically with no error handling | Medium | Low | Open |
| [GRX-019](#grx-019) | `#config logdir` to a missing folder silently disables logging | Low | Low | Open |
| [GRX-020](#grx-020) | `#highlight clear` leaves the string-highlight regex live | Low | Low | Open |
| [GRX-021](#grx-021) | `throw ex` discards stack traces in crypto and config failures | Low | Low | Open |
| [GRX-022](#grx-022) | `HandleGenieException` is an unreachable infinite-recursion trap | Low | Low | Open |
| [GRX-023](#grx-023) | Mapper value types override `Equals` without `GetHashCode` | Low | Low | Open |

---

## Critical

### GRX-001
**Server text is decoded as UTF-8; DragonRealms sends Latin-1**
`Core/Connection.cs:775` (receive), `:678` (send), `:352`–`:539` (login handshake)

Every byte from the game goes through `Encoding.Default`. On .NET Framework — which this code was
written for — `Encoding.Default` was the system ANSI codepage, i.e. Windows-1252. On .NET Core and
later, including the .NET 10 this client now targets, `Encoding.Default` is **always UTF-8**. The
port changed the meaning of this call without changing the call.

There is a second, independent defect on the same line: decoding is done per packet with a
stateless `GetString`. A multi-byte sequence that straddles the 10,240-byte receive boundary is
mangled even when the encoding is right. Correct handling needs a `Decoder` held across callbacks.

**Verified.** `[System.Text.Encoding]::Default.EncodingName` on the .NET Core runtime family
returns `Unicode (UTF-8)`; the Latin-1 byte `0xE9` (é) decodes to `U+FFFD`.

*Why it matters to players:* any accented character, em dash, or curly quote in room descriptions,
player names, item names or LNet chatter renders as `�`. The same bug on the send side means
typing those characters produces bytes the server does not expect.

*Risk (Medium):* the encoding is on the login handshake path too, so a wrong choice breaks
connecting outright. Confirm DragonRealms' actual encoding (ISO-8859-1 vs Windows-1252) against a
live session before committing; they differ in the `0x80`–`0x9F` range.

---

### GRX-002
**Case-insensitive highlights are destroyed by a save/reload cycle**
`Lists/Globals.cs:1909` (save), `Lists/Globals.cs:2072`/`:2087` (load), `Lists/Highlights.cs:103`

`SaveHighlights` writes a case-insensitive highlight as `#highlight {string} {red} {/orc/i}` —
wrapping the text in the `/…/i` marker used everywhere else in the config format. `AddHighlight`,
which reads it back, calls `HighlightList.Add(arg3, …, bCaseSensitive: true, …)` with the flag
**hardcoded**, and `Highlights.Add` contains no `/…/i` parsing at all.

Every sibling list handles this correctly — `HighlightRegExp` (`Globals.cs:1379`),
`HighlightLineBeginsWith` (`:1258`), `SubstituteRegExp` (`:1456`), `GagRegExp` (`:1684`) and
`Triggers` (`:1019`) all strip the marker inside `Add`. `Highlights` is the one list that writes
the marker and never reads it.

This was latent before `f5f1385` ("Highlight case-insensitive flag now respected") because the
flag was ignored at match time anyway. Now that the feature works, players will use it — and it
breaks on the next restart.

*Why it matters to players:* a case-insensitive highlight survives until the client is closed.
On reload its key becomes the literal string `/orc/i`, which matches nothing. The highlight is
silently dead, and the config UI shows the mangled text. Re-saving persists the corruption, so it
does not self-heal.

*Risk (Medium):* the fix itself is small (strip `/…/i` in `Highlights.Add`, or stop writing it and
persist the flag positionally). The work is in migrating configs already corrupted in the field —
detect a leading `/` and trailing `/i` on load and repair rather than orphaning the entry.

---

### GRX-003
**Closing the window and answering "No" kills every trigger for the session**
`Forms/FormMain.cs:1602`

`FormMain_FormClosing` calls `_triggerChannel.Writer.TryComplete()` as its **first** statement —
before the "You are connected to the game" confirmation. If the player answers No, the handler
sets `e.Cancel = true` and returns, but the channel is already completed.

`Game_EventTriggerParse` (`:6561`) writes with `TryWrite`, which returns `false` on a completed
channel rather than throwing, so its `catch` never fires. The consumer loop at `:146` has already
exited.

*Why it matters to players:* clicking the X and changing your mind silently disables every
trigger — combat scripts, healing alerts, LNet responders — with no message and nothing in the log.
Nothing indicates why, and only a restart fixes it. Hitting X by accident is common.

*Risk (Low):* move the `TryComplete()` to the point where shutdown actually commits (after
`bCloseNow = true`).

---

### GRX-004
**Every config save deletes the file first, then writes**
`Lists/Config.cs:453`, `Lists/Globals.cs:790`, `:1888`, and the aliases / macros / triggers /
presets / subs / gags savers on the same pattern

The shape is identical everywhere:

```csharp
if (File.Exists(sFileName)) Utility.DeleteFile(sFileName);
var oStreamWriter = new StreamWriter(sFileName, false);
```

The delete is redundant — `StreamWriter(path, append: false)` truncates — and it converts every
write failure into total loss. `SaveHighlights` is the worst case: it has **no `try`/`catch` at
all** (the `// Try` at `Globals.cs:1890` is commented out) and no `using`, so if the constructor
throws or the loop faults, `highlights.cfg` is already gone and the handle leaks.

*Why it matters to players:* an editor holding the file open, an antivirus or OneDrive lock, a
read-only flag, or a full disk means the player's entire highlight set, variable set, or settings
file is deleted rather than left alone. These are hand-built over years and there is no backup.

*Risk (Low):* write to `<name>.tmp`, then `File.Replace`/`File.Move` over the original. Standard
change, no format impact, and it makes the failure mode "old file survives" instead of "no file".

---

## High

### GRX-005
**Script keyword regex built with `&` instead of `|`**
`Script/Eval.cs:83`

**Status: ✅ Fixed 2026-08-02.** Changed `&` to `|`. Verified by probe both ways: the old form
does not match `$health > 50 AND $mana > 20`, the fixed form does, and it still matches lowercase
`and`, still respects word boundaries (`ANDROID` does not match), and leaves `$NOTES` alone. No
change needed to quoted-string handling — `ReplaceKeyWords` was already only applying this outside
string literals, which is what made the fix safe.

```csharp
new Regex(@"\b(eq|and|or|not|true|false)\b", RegexOptions.IgnoreCase & MyRegexOptions.options)
```

`RegexOptions.IgnoreCase` is `1`, `MyRegexOptions.options` is `RegexOptions.Multiline` = `2`.
Bitwise **and** gives `0` — `RegexOptions.None`. Both intended options are silently discarded.
Every other construction site in the codebase uses `|` correctly.

**Verified.** `IgnoreCase -band Multiline` evaluates to `RegexOptions.None`.

*Why it matters to players:* `IF $health > 50 AND $mana > 20` does not translate `AND` to `&&`,
so the expression reaches the evaluator malformed. Lowercase happens to work, so this presents as
"my script works but my friend's identical one doesn't" — the difference being capitalisation.
The lost `Multiline` is harmless here (the pattern uses no anchors).

*Risk (Low):* change `&` to `|`. Worth a scan for scripts that use `AND`/`OR` as literal words
inside quoted strings, though `ReplaceKeyWordsSection` is already only applied outside strings.

---

### GRX-006
**`#plugin` from a script crashes when a new-ABI plugin is installed**
`Script/Script.cs:2883`, `:2899`

```csharp
foreach (GeniePlugin.Interfaces.IPlugin oPlugin in m_oGlobals.PluginList)
```

`PluginList` is heterogeneous by design: `FormMain.VerifyAndLoadPlugin` has two overloads, one
adding `GeniePlugin.Interfaces.IPlugin` (legacy) and one adding `GeniePlugin.Plugins.IPlugin`
(current) — `FormMain.cs:1083` and `:1102`. Every other consumer branches on the runtime type
(`Game.ParsePluginText`, `FormMain.ListPlugins`); these two loops downcast every element instead,
so a modern plugin in the list throws `InvalidCastException` on the first iteration.

Neither loop has a `try`/`catch`, so a plugin that throws also takes the script line down with it.

*Why it matters to players:* `EvalPlugin` / `EvalPluginScript` back the `#plugin` script hook.
Anyone running a current-ABI plugin gets a script abort the moment a script touches it. The
error points at the script, not the plugin, so it looks like the script is broken.

*Risk (Low):* mirror the type test already used in `Game.ParsePluginText`, and wrap the call.

---

### GRX-007
**`.Result` on an async command can deadlock the UI thread permanently**
`Core/Command.cs:2873`, `Utility/LichLauncher.cs:238`, `:287`

`ParseCommand` is `async Task<string>` (`Command.cs:219`), but `ParseAllArgs` blocks on it:

```csharp
sResult = ParseCommand(sCommand.Substring(1), false, false, "", bParseQuickSend).Result;
```

In practice `ParseCommand` completes synchronously — an async method with no reached `await` runs
inline — *except* on the `#lc` path, which awaits `LichLauncher.EnsureRunning` (`Command.cs:435`).
`LichLauncher` uses no `ConfigureAwait(false)` anywhere, so its continuations post back to the
captured `SynchronizationContext`. If `ParseAllArgs` is running on the UI thread (macros, typed
input and triggers all reach it), the UI thread is blocked in `.Result` while the continuation
waits for that same thread. Neither ever proceeds.

Related: the six `CS4014` warnings in `FormMain.cs` are all fire-and-forget `ParseCommand` calls.
The `try`/`catch` around one of them (`:4612`) cannot catch anything past the first `await`, which
is a false sense of safety rather than a live fault today.

*Why it matters to players:* a hard hang — window unresponsive, no error, Task Manager required,
and any unsaved session state lost. Narrow trigger, but nothing recovers from it.

*Risk (Low):* add `ConfigureAwait(false)` throughout `LichLauncher`, which removes the deadlock
without restructuring. Making `ParseAllArgs` properly async is the real fix and is a larger change.

---

### GRX-008
**Incoming images wipe the player's clipboard**
`Forms/Components/ComponentRichTextBox.cs:380`–`393`

`InvokeAddImage` renders a character portrait by round-tripping it through the system clipboard:
save the current contents, `Clipboard.Clear()`, set the image, paste it into the control, clear
again, restore.

`Clipboard.GetDataObject()` returns a live `IDataObject` owned by the source application. Once
`Clipboard.Clear()` has run, that object is frequently dead, so the "restore" puts back nothing.
The retry helper also blocks the UI thread up to 500 ms per operation — four operations, so up to
two seconds — under clipboard contention. And `ReadOnly` is set `false` around the paste with no
`finally`, so an exception mid-paste leaves the game output window editable.

*Why it matters to players:* copy something, then walk into a room that sends a portrait, and the
clipboard is empty. Nothing connects the two events, so it reads as a Windows fault.

*Risk (Medium):* the correct fix is to insert the image via RTF directly and stop touching the
clipboard, which means reworking image insertion. Restoring `ReadOnly` in a `finally` is a cheap
independent improvement.

---

### GRX-009
**Output buffer is never trimmed while the player is scrolled up**
`Forms/Components/ComponentRichTextBox.cs:346`, `:874`–`886`

Two problems in the hottest path in the client.

`InvokeAddRTF` only trims when `m_bIsScrolling == false`. `m_bIsScrolling` is set true whenever the
view is not near the bottom, so scrolling up to read backlog disables the `TextLength >
MaxBufferSize` trim entirely. The `RichTextBox` then grows without bound.

Separately, `AddText` evaluates `m_oRichTextBuffer.Lines.Length` on **every** call.
`RichTextBox.Lines` allocates and returns a `string[]` split of the whole buffer each time it is
read — so every line of game text costs a full split of the pending buffer.

*Why it matters to players:* scroll up during a busy hunt, get distracted, come back to a client
consuming hundreds of MB and visibly stuttering. The per-line `Lines` allocation is a constant
tax on exactly the situation — heavy combat spam — where responsiveness matters most.

*Risk (Medium):* this is the render path, and the scroll-position preservation around it is
delicate. Track a line counter instead of re-reading `Lines`, and trim on a size ceiling
independent of scroll state.

---

## Medium

### GRX-010
**Output freezes while the mouse button is held in the game window**
`Forms/Components/ComponentRichTextBox.cs:925`, `:1025`–`1080`

`InvokeEndUpdate` calls `FlushBuffer()` only `if (m_bMouseDown == false)`. `m_bMouseDown` is set on
`MouseDown` and cleared only in `MouseUp` on the same control. Text still accumulates in the
pending buffer meanwhile (see GRX-009), so holding the button also drives the `Lines.Length` cost.

Mouse capture normally delivers `MouseUp` even outside the control, so the stuck-forever case needs
capture to be lost — Alt-Tab mid-drag, a modal dialog, a session lock.

*Why it matters to players:* selecting text pauses the display, which is expected. Losing capture
mid-drag pauses it permanently, which reads as "Genie froze" — and the recovery (click and release
inside the window) is not discoverable.

*Risk (Low):* also clear `m_bMouseDown` on `LostFocus` / `MouseCaptureChanged`.

---

### GRX-011
**A trailing `|` in the monster ignore list zeroes `$monstercount`**
`Core/Game.cs:2640`

```csharp
foreach (string sIgnore in m_oGlobals.Config.sIgnoreMonsterList.Split('|'))
    if (sValue.Contains(sIgnore)) { bIgnore = true; break; }
```

`Split` on a string with a trailing separator — or on an empty string — yields an empty element,
and `string.Contains("")` is always `true`. One empty entry makes every creature ignored.

The default (`"appears dead|(dead)"`) is safe; this only bites a player who edits the setting.
Writing `#config monstercountignorelist {appears dead|(dead)|}` is an easy mistake to make.

*Why it matters to players:* `$monstercount` reads 0 with creatures in the room. Combat and
hunting scripts that gate on it stop attacking, and the setting that caused it is not obviously
related.

*Risk (Low):* skip empty entries.

---

### GRX-012
**Overlapping highlights: the shortest match wins**
`Lists/Highlights.cs:173`, `:183`, `:224`, `:234`

`RebuildStringIndex` and `RebuildLineIndex` build one alternation from all highlight keys, sorting
them **ascending** first. .NET regex alternation is first-match-wins, so `orc|orc chieftain`
matches `orc` and stops — the longer, more specific highlight never fires.

`Globals.UpdateMonsterListRegEx` (`:124`–`125`) gets this right for the same construction, sorting
then reversing so longer strings come first. The two should agree.

*Why it matters to players:* highlights that share a prefix silently lose to the shorter one — a
generic `orc` in red shadows a specific `orc chieftain` in flashing yellow, which is exactly the
case where the specific highlight is the one that matters.

*Risk (Low):* sort by descending length, then alphabetically for stability.

---

### GRX-013
**Room window silently skips updates under lock contention**
`Core/Game.cs:939`, `:964`

`SetBufferEnd` and `UpdateRoom` use `Monitor.TryEnter(m_oThreadLock)` with **no timeout** — a
single instantaneous attempt. On failure they log "Unable to aquire game thread lock" and return,
dropping the room update. Every other `TryEnter` in the codebase passes a timeout
(`Script.cs` uses `m_iDefaultTimeout` = 3500 ms; `Log.cs` uses 100 ms).

*Why it matters to players:* the room window keeps showing the previous room, or goes blank, while
the main window has clearly moved on. Since the socket receive thread and the script thread both
reach this, it is most likely under exactly the automated movement where the room window is being
relied on.

*Risk (Medium):* passing a timeout is a one-line change, but it converts a dropped update into a
blocked receive thread. Needs a live session to confirm the lock is not held long.

---

### GRX-014
**Autolog reopens the log file for every line, and drops lines silently**
`Utility/Log.cs:34`, `:14`

`LogText` opens a `StreamWriter`, writes one line, and closes it — per line of game text. It also
guards with `Monitor.TryEnter(m_oThreadLock, 100)`, and the `else` branch is an empty block with a
commented-out `throw`: on contention the line is dropped with no record.

*Why it matters to players:* an open/write/close syscall per line during combat spam is
measurable I/O churn, and it is the default configuration. Silent gaps in a log are worse than a
slow log — logs are what players use to reconstruct what killed them.

*Risk (Low):* hold the writer open with periodic flush, and close it on disconnect or log rollover.

---

### GRX-015
**Connect paths catch only `SocketException`**
`Core/Connection.cs:224`, `:290`, `:296`, `:684`, `:709`

`Connect` and `ConnectAndAuthenticate` are both invoked via `Task.Run` (`Game.cs:427`, `:1272`,
`:2751`) and catch only `SocketException`. An `IOException` from the TLS stream, an
`ObjectDisposedException` from a socket torn down concurrently, or an `ArgumentException` from a
malformed host escapes into an unobserved task.

The same holds for the two `Send` overloads, which sit on the script and UI threads.

This is the exact failure class 4.2.0 fixed for the authentication path (`Connection.cs:313`–`316`
documents it); the surrounding methods were not brought along.

*Why it matters to players:* the connect dies in complete silence — no message, no reconnect, a
window that never does anything. That is the symptom the 4.2.0 work was aimed at.

*Risk (Low):* broaden to `catch (Exception)` with an explicit reported failure, matching the
pattern already established in `Authenticate`.

---

### GRX-016
**Concurrent `BeginSend` gives no ordering guarantee**
`Core/Connection.cs:679`, `:704`

`lock (m_oSendLock)` covers only the *initiation* of `BeginSend`, not completion. Two overlapped
sends on the same socket have no defined completion order under Winsock, so two commands issued
close together can reach the server reversed.

*Why it matters to players:* a script that sends `stow left` then `get rock` can have them
arrive swapped. Rare, non-deterministic, and effectively impossible for a player to diagnose —
it presents as "my script does the wrong thing sometimes".

*Risk (Medium):* the correct fix is a serialised send queue draining on one worker. That is a
structural change to the send path and needs care around disconnect.

---

### GRX-017
**`#config scripttimeout` does nothing; `waitfor` has no timeout**
`Lists/Config.cs:27`, `Script/Script.cs:1549`

`iScriptTimeout` is declared, parsed from config, saved back to config, and echoed by
`#config` (`Command.cs:2942`) — and never read anywhere in `Script/`. Only `iScriptMatchTimeout`
is actually used (`Script.cs:1186`).

Correspondingly, `TickScript` gives `ScriptState.matchwait` a timeout branch
(`m_bMatchTimeoutState`, `:1635`) but `ScriptState.waitfor` (`:1549`) has none. A `waitfor` whose
text never arrives waits forever.

*Why it matters to players:* the setting is visible, documented and settable, and does nothing —
so a player who hits a hung script and sets `scripttimeout` to fix it gets no change and no
explanation. Meanwhile a `waitfor` that misses its line (gagged text, a disconnect, a typo) hangs
the script with no recovery but `#script abort`.

*Risk (Medium):* wiring up a timeout changes long-standing script behaviour. Some scripts likely
depend on `waitfor` blocking indefinitely. Consider honouring the setting only when non-zero, and
defaulting it to off.

---

### GRX-018
**Map files are written non-atomically with no error handling**
`Mapper/MapForm.cs:943`

`SaveXML` opens an `XmlTextWriter` directly over the live map file, with no `using` and no
`finally`. A fault partway through leaves a truncated, unparseable map — and the writer unflushed.

Same family as GRX-004, but tracked separately because maps are shared artefacts players download
and hand-edit, not per-user config.

*Why it matters to players:* a corrupted map file breaks the automapper for that zone, and hand-
built room notes and arcs are lost.

*Risk (Low):* write to a temp file and replace, as with GRX-004.

---

## Low

### GRX-019
**`#config logdir` to a missing folder silently disables logging**
`Utility/Log.cs:22`–`:34`, `Forms/FormMain.cs:2451`

`Logs/` is created at startup at the default location only. `Log.LogDirectory` follows
`Config.sLogDir`, which the player can repoint. Nothing creates the new directory, so the
`StreamWriter` throws, `GenieError` fires, and logging is off from then on.

*Why it matters to players:* logging appears enabled and produces nothing.
*Risk (Low):* create the directory on demand, and report the failure once rather than per line.

---

### GRX-020
**`#highlight clear` leaves the string-highlight regex live**
`Core/Command.cs:1918`

The `clear` branch calls `RebuildLineIndex()` but not `RebuildStringIndex()`. `Highlights.Remove`
rebuilds neither. The stale compiled alternation keeps matching every line of output forever; the
subsequent key lookup fails, so no colour is applied and the bug is masked.

*Why it matters to players:* nothing visible today — it is wasted work on the hot path and a
correctness trap for anyone who later adds an `IsActive` check to the consumers.
*Risk (Low):* rebuild both indexes wherever the list is mutated.

---

### GRX-021
**`throw ex` discards stack traces in crypto and config failures**
`Utility/Crypto.cs:463`, `:564`, `:613`, `Lists/Config.cs:1492` (4 × `CA2200`)

Re-throwing the caught variable resets the stack trace to the re-throw point, so the exception
dialog names the `catch` block rather than the code that failed.

*Why it matters to players:* only indirectly — it makes their bug reports unactionable, on the
password and settings-parsing paths where reports are hardest to reproduce.
*Risk (Low):* `throw;` instead of `throw ex;`.

---

### GRX-022
**`HandleGenieException` is an unreachable infinite-recursion trap**
`Lists/Globals.cs:96`–`100`

The handler subscribed to `GenieError.EventGenieError` calls `GenieError.Error(...)`, which raises
that same event — unbounded recursion ending in an uncatchable `StackOverflowException`. It also
sets `Config.bAutoLog = false`, silently disabling logging on any error.

It is currently dead: subscription happens only in the `Globals.Log` property setter, and nothing
ever assigns `Globals.Log`. The subscribe/unsubscribe logic in that setter is also inverted.

*Why it matters to players:* nothing today. It becomes an instant process kill the moment anyone
wires up `Globals.Log`.
*Risk (Low):* delete it, or make it report without re-raising.

---

### GRX-023
**Mapper value types override `Equals` without `GetHashCode`**
`Mapper/NodeList.cs:30` (`Point3D`), `:211` (`Label`), `:620` (`Node`) — 3 × `CS0659`

None of the three currently go into a `Dictionary` or `HashSet`, so this is latent.

*Why it matters to players:* nothing today. Any future pathfinding work that reaches for a hash
set of nodes gets silently wrong lookups — the failure mode is a route that cannot be found rather
than a crash.
*Risk (Low):* implement `GetHashCode` consistently with `Equals`.

---

## Notes on the build

`dotnet build Genie4.sln -c Release` → **0 errors, 45 warnings**, matching the CI baseline. Of the
45, the ones above account for `CA2200` × 4 and `CS0659` × 3. The 6 × `CS4014` are covered under
GRX-007. The remainder are unused fields, unused events, and one obsolete `WebClient`
(`Forms/DialogDownload.cs:14`) — noise, but the count is low enough to keep meaningful, so they
are worth clearing eventually rather than raising the baseline.
