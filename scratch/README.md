# scratch/

Local working area. **Nothing in here is committed** — see the `/scratch/` block in
[`.gitignore`](../.gitignore). This README is the single tracked file, so the folder exists in a
fresh clone instead of showing up as a mystery empty directory.

## What belongs here

Throwaway things that support development but are not part of the product:

- One-off probe programs — e.g. a tiny console app to confirm what `Encoding.Default` resolves
  to on this runtime, or how a regex option combination evaluates
- Diagnostic and comparison scripts
- Captured output: build logs, `#config` dumps, before/after text captures from a running client
- Copies of config files pulled from the test install for inspection
- Notes in progress that are not yet worth a `docs/` page

## What does not belong here

- **Anything that should be reviewable.** If a script is worth running twice, it is worth a real
  home and a commit message.
- **Secrets.** `.gitignore` will stop them reaching the repo, but this folder still sits inside
  it — the config files under the test install carry saved (weakly protected) passwords. Don't
  accumulate copies here.
- **Test builds.** Those go in `C:\GenieRemix-4Realz\`, not in the repository. Building a
  ~112 MB self-contained tree inside the working copy is slow and easy to commit by accident.

## Promoting something out

Move it to where it belongs — `docs/`, `.github/`, a tools folder — and commit it there. Do not
`git add -f` from inside `scratch/`; the whole value of the folder is that its contents are
never a candidate for commit.

## Housekeeping

Safe to empty at any time. Nothing here is depended on by the build, CI, or the app. If deleting
it would lose something, it was in the wrong place.
