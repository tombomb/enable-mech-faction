# Releasing to the Steam Workshop

RimWorld uploads mods to the Workshop from inside the game. There's no separate tool and no API key.

## What you need

- A Steam account that owns RimWorld.
- The mod in the game's **local** `Mods` folder (not a Workshop subscription). `./build.ps1 -Install` puts it there.
- **`About/Preview.png`** — the Workshop thumbnail. This repo doesn't have one yet.
  - 16:9 works best (e.g. 640×360 or 1280×720), PNG, under 1 MB.
  - Put it in the repo's `About/` folder. `build.ps1` copies it along with the rest of `About/`.
- A finished `About/About.xml`. The `<description>` is what shows on the Workshop page; edit it there later if you
  want Steam formatting (BBCode).
- To have accepted the [Steam Workshop legal agreement](https://steamcommunity.com/sharedfiles/workshoplegalagreement).
  Until you do, the item stays hidden no matter what visibility you pick.

## First upload

1. `./build.ps1 -Install`
2. Start RimWorld → **Options** → enable **Development mode**.
3. **Mods** menu → select **Mech Faction Enabler** → open the options menu in the mod's info panel →
   **Upload to Steam Workshop**. It's only listed for local mods, with dev mode on and Steam running.
4. Confirm. The game uploads the whole mod folder and writes `About/PublishedFileId.txt` into the **Mods** copy.
5. **Copy `PublishedFileId.txt` back into this repo's `About/` folder and commit it.** It links future uploads to
   the same Workshop item. Without it, the next upload creates a second item.
6. Open the item on the Workshop (Steam → Workshop → Your Files), check the visibility setting, add a
   screenshot of the popup if you like, and check the description.

## Updating

1. Bump the version (`<Version>` in the `.csproj`, `<modVersion>` in `About.xml`), commit, tag (see the README).
2. `./build.ps1 -Install`. The script keeps `PublishedFileId.txt` in the Mods copy, and copies the repo's one if it's committed.
3. In game: Mods → select the mod → options menu → **Update on Steam Workshop**. It updates the existing item.
4. Add a change note on the Workshop page.

## Why upload from `dist/` / the Mods copy, not the repo

The uploader publishes **everything** in the mod folder. The repo also contains `Source/`, `.git/`, `.github/` and
`dist/`, which don't belong on the Workshop. `build.ps1` builds a clean copy with only `About/`, `1.6/`,
`Languages/` and `LICENSE`.

## Checklist before the first public upload

- [ ] `About/Preview.png` added
- [ ] Tested on a real save without mechanoids: popup → add → save → reload → no errors in the log
- [ ] Tested on a save that already has mechanoids: no popup, *Add Mechanoid faction* refuses
- [ ] Version numbers match in the `.csproj` and `About.xml`
- [ ] `PublishedFileId.txt` committed after the first upload
