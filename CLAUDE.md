# CLAUDE.md

Notes for Claude Code (and anyone else) working in this repo.

## What this is

**Mech Faction Enabler** — a RimWorld **1.6** mod (C#, .NET Framework 4.7.2) that adds the vanilla Mechanoid
faction to a save generated without it. Written for Tom's wife, who removed mechs at world creation and then
wanted Biotech mechanitor / Odyssey content. Keep it **mechanoid-only** and small; it was deliberately scoped
down from a general "re-add any faction" tool.

Personal project (not THUMBWAR work). Public repo: https://github.com/tombomb/enable-mech-faction

## Git / GitHub — personal account setup

This machine has two `gh` accounts: **TW-Tom** (work, the active/default one) and **tombomb** (personal).
This repo must use tombomb, same pattern as `../DOW-Alerter`:

- Local git identity is set: `user.name=tombomb`, `user.email=tombomb318@gmail.com`. Don't commit as TW-Tom.
- Remote URL includes the username so the gh credential helper picks tombomb:
  `https://tombomb@github.com/tombomb/enable-mech-faction.git`
- For `gh` commands, don't switch the global account. Set a per-command token instead:
  ```powershell
  $env:GH_TOKEN = gh auth token --user tombomb; gh run list -R tombomb/enable-mech-faction
  ```
- PowerShell 5.1 mangles quotes in `git commit -m` here-strings; write the message to a file and use `git commit -F`.
- Don't edit files with PowerShell `Get-Content`/`Set-Content` round-trips — 5.1 reads UTF-8 as ANSI and garbles
  `→ — ×`. Use the Edit/Write tools.

## Build

- `.NET 8 SDK` is installed (winget). Build: `dotnet build Source/MechFactionEnabler/MechFactionEnabler.csproj -c Release`
  → outputs `1.6/Assemblies/MechFactionEnabler.dll` (gitignored).
- `./build.ps1` → clean mod folder in `dist/MechFactionEnabler`. `./build.ps1 -Install` also copies it to
  `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\MechFactionEnabler` (writable without admin).
- References come from NuGet `Krafs.Rimworld.Ref` (not publicized — private members aren't accessible) with
  `ExcludeAssets=runtime`. **Keep its version in sync with the game build** in
  `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Version.txt` (currently 1.6.4871).
- CI: `.github/workflows/build.yml` on ubuntu. Builds on push/PR, uploads the mod folder as an artifact, and on
  `v*` tags attaches a zip to a GitHub Release.
- No Harmony. Don't add it unless something truly can't be done otherwise.

## Verifying game APIs

Don't trust memory or old wiki pages for RimWorld internals — decompile the game:
```powershell
# ilspycmd 9.1.0.7988 is installed as a global dotnet tool (newer versions need .NET 10)
ilspycmd -t RimWorld.FactionGenerator -r "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll"
```
Use `-p -o <dir>` for the whole assembly (≈9k files; put it in a scratch dir, never in the repo — it's copyrighted).
Game XML defs: `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Data\<Core|Biotech|Odyssey|...>\Defs`.

## Code map

- `MechFactionUtility.cs` — **all** checks and the add live here; UI calls `AddWithFeedback()`.
  `TryAddMechanoidFaction` must stay idempotent.
- `GameComponent_MechFactionEnabler.cs` — popup on `LoadedGame` (not `StartedNewGame`), deferred with
  `LongEventHandler.ExecuteWhenFinished`. Saves one bool, `dontAskAgain`.
- `DebugActions_MechFactionEnabler.cs` — `[LudeonTK.DebugAction]` entries (namespace is LudeonTK since 1.5).
- `Languages/English/Keyed/MechFactionEnabler.xml` — all player-facing strings, `MFE_` prefix. Use `.Translate()`,
  no hard-coded UI text.

## Verified facts about the 1.6 game code (build 1.6.4871)

- `FactionGenerator.NewGeneratedFaction(parms)` creates relations with every existing faction, on both sides.
  It only adds a settlement for non-hidden factions. Mechanoid is hidden, `permanentEnemy`, not humanlike (no ideology),
  and has no `layerWhitelist`, so generating it on the Surface layer is correct.
- `FactionManager.Add` only dedupes by instance, then calls `RecacheFactions()`, so `Faction.OfMechanoids`
  updates immediately. `RecacheFactions` uses `FirstFactionOfDef` → the **first** Mechanoid faction wins.
- `Faction.TryMakeInitialRelationsWith(other)` only checks *this* side before writing both, so only call it when both
  sides are missing.
- Nothing in vanilla regenerates a missing Mechanoid faction. `BackCompatibility.FactionManagerPostLoadInit`
  only does Empire, HoraxCult, Entities, TradersGuild and Salvagers.
- `QuestNode_Root_Bossgroup` creates a **temporary** Mechanoid faction when none exists. It's auto-removed later
  via `FactionManager.FactionCanBeRemoved`. We treat it as present (no duplicate).
- Odyssey gravcore/Mechhive quests check `TestRunInt` → skipped while missing, re-checked on every roll → fine
  once added. The Mechhive warning letter in `QuestNode_Root_GravShip` is a one-time loss.
- Mechanitor ship quest comes from decrypting a `MechanoidTransponder`; no one-shot global flag.
- Existing factionless mechs are left alone on purpose: `SetFaction` would pull them out of their Lord.
- Removing the mod from a save: the unknown GameComponent class logs an error, deserializes to null, and
  `Game.FillComponents` drops it. Harmless.

## Open ideas / TODO

- If the only Mechanoid faction is a temporary bossgroup one, set `temporary = false` on it instead of treating
  it as present (it's a public field on `Faction`). Not done yet.
- `About/Preview.png` still needed for the Workshop (see WORKSHOP.md).
- Not yet tested in-game on a real no-mechs save.
