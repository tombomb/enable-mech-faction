# Mech Faction Enabler

[![Build](https://github.com/tombomb/enable-mech-faction/actions/workflows/build.yml/badge.svg)](https://github.com/tombomb/enable-mech-faction/actions/workflows/build.yml)
![RimWorld 1.6](https://img.shields.io/badge/RimWorld-1.6-blue)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A small RimWorld 1.6 mod that adds the **Mechanoid faction** back to a save that was created without it.

If you removed mechanoids from the faction list when you made your world, and later want to play a
**mechanitor** (Biotech) or do **Odyssey's** mechanoid content, you'll find a lot of it silently doesn't
happen. This mod fixes that without starting a new colony.

## What it does

- When you load a save that has **no** mechanoid faction, a popup asks whether to add it.
  - **Add mechanoids** — adds the vanilla Mechanoid faction, then asks you to save and reload.
  - **Don't ask again for this save** — the popup stays away for that save.
  - Press Escape to decide later; it asks again next time you load.
- On saves that already have mechanoids, it does nothing at all.
- Dev-mode tools are also available under **Debug actions → Mech Faction Enabler**:
  - *Add Mechanoid faction*
  - *Log faction status* (writes every faction in the save to the log — useful for bug reports)
  - *Re-enable load prompt*

The added faction is exactly what world generation would have created: hidden, permanently hostile to
everyone, no settlements.

## Safety checks

The mod is built to never double anything up:

| Check | Why |
|---|---|
| Refuses if **any** Mechanoid faction exists, including a temporary one | Stops a second mech faction being created. |
| Also treats a modded faction with `replacesFaction = Mechanoid` as present | Doesn't add vanilla mechs on top of a mod that swaps them out. |
| Relations are only backfilled when **both** sides are missing | The vanilla helper writes both sides but only checks one; calling it blindly could duplicate an entry. |
| Never touches existing pawns, lords, quests or other factions' relations | Changing a pawn's faction drops it from its AI group, which is worse than leaving it. |
| Only runs while a game is loaded, and catches/logs any error | A failure leaves the save as it was. |

## Install

**Back up your save first.** (Saves live in `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Saves`.)

1. Download `MechFactionEnabler.zip` from the [latest release](https://github.com/tombomb/enable-mech-faction/releases/latest)
   (under **Assets**). No GitHub account needed.
2. Find your RimWorld folder: in Steam, right-click **RimWorld** → **Manage** → **Browse local files**.
   Open the `Mods` folder inside it.
3. Right-click the zip → **Extract All…** → set the destination to that `Mods` folder. Windows adds
   `\MechFactionEnabler` to the end of the path, so delete that part first.
   You should end up with `Mods\MechFactionEnabler\About\About.xml`. If you get
   `Mods\MechFactionEnabler\MechFactionEnabler\...` instead, move the inner folder up one level.
4. Start RimWorld, enable **Mech Faction Enabler** in the Mods menu (anywhere after the DLCs) and restart when asked.
5. Load your save and click **Add mechanoids**.
6. **Save, then reload the save.**

Once the faction exists you can uninstall the mod if you like — the faction is part of your save now.
The next load shows a harmless red `Could not find class MechFactionEnabler.GameComponent_MechFactionEnabler`
error or two; save once and it's gone.

## What to expect afterwards (caveats)

These come from reading the game's 1.6 code, not guesses:

- **Mech raids and threats start happening** — that's the point, but the storyteller will now include
  mechanoid raids, mech clusters and crashed ship parts like a normal game.
- **Odyssey mechanoid quests start appearing.** The Mechhive, orbital/crashed mechanoid platforms and
  mechanoid relay quests were being skipped while the faction was missing. They're re-checked every time the
  game picks a quest, so they become possible straight away. Nothing about them is created at world generation.
- **The one-time Mechhive warning letter won't come** if your gravship questline had already started. It's
  scheduled only when that quest begins. It's just a letter; the Mechhive content itself still works.
- **Biotech mechanitor ship:** mechanoid transponders you haven't decrypted yet will work normally.
  If one was already decrypted while mechs were missing, that quest ran with factionless mechs and won't repeat.
- **Mechs already on your map stay as they are.** Any that spawned without a faction (e.g. from a gestator
  tank or a mech that lost its overseer) are left alone. New ones will belong to the faction.
- **Ancient dangers and ruins** generated while the faction was missing mostly just spawned no mechs. The mod
  doesn't go back and add them.
- **Bossgroup edge case:** summoning a bossgroup with no mech faction makes the game create a *temporary*
  one, which it deletes by itself once those mechs are gone. The mod counts that as "present" and won't add a
  duplicate — but if the game later deletes it, you're back to no mechs. Load the save again and the popup will
  offer to add a permanent one.

## Building from source

You don't need the game installed to build — the RimWorld reference assemblies come from the
[`Krafs.Rimworld.Ref`](https://www.nuget.org/packages/Krafs.Rimworld.Ref) NuGet package.

Requirements: [.NET SDK 8](https://dotnet.microsoft.com/download) or newer (the mod itself targets .NET Framework 4.7.2, which RimWorld uses).

```powershell
# Build the DLL into 1.6/Assemblies
dotnet build Source/MechFactionEnabler/MechFactionEnabler.csproj -c Release

# Build + assemble a clean mod folder in dist/MechFactionEnabler
./build.ps1

# ...and copy it into the game's Mods folder (default Steam path; override with -ModsDir)
./build.ps1 -Install
```

If Windows blocks the script: `powershell -ExecutionPolicy Bypass -File .\build.ps1 -Install`.

### Project layout

```
About/About.xml                    mod metadata (name, packageId, supported versions)
1.6/Assemblies/                    build output (gitignored)
Languages/English/Keyed/           all player-facing text
Source/MechFactionEnabler/
  MechFactionUtility.cs            the checks + the actual add (single entry point)
  GameComponent_MechFactionEnabler.cs  on-load popup, per-save "don't ask again"
  DebugActions_MechFactionEnabler.cs   dev-mode menu entries
build.ps1                          build / package / install
.github/workflows/build.yml        CI: build, upload artifact, attach zip to releases on v* tags
```

### Releasing

Bump `<Version>` in the `.csproj` and `<modVersion>` in `About.xml`, commit, then:

```powershell
git tag v1.0.1
git push origin v1.0.1
```

CI builds the mod and attaches `MechFactionEnabler.zip` to a GitHub Release. Steam Workshop steps are in
[WORKSHOP.md](WORKSHOP.md).

## Compatibility

- RimWorld **1.6** only. Works with or without the DLCs (it's most useful with Biotech and Odyssey).
- No Harmony, no patches to game code — it just calls the game's own faction generator once.
- Should work with other mods. If a mod replaces the mechanoid faction with its own, this mod will see that and
  do nothing.

## Reporting a problem

[Open an issue](https://github.com/tombomb/enable-mech-faction/issues) with your mod list and, if you can,
the output of **Debug actions → Mech Faction Enabler → Log faction status** from the dev-mode log.

## License

[MIT](LICENSE)
