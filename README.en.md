# SptQuestLive

**Language: [한국어](README.md) | [English](README.en.md)**

## Purpose

Overrides quests to match the latest version of EFT, on top of SPT.
Since EFT has drastically changed quest structure, quest chains, and the trader reputation system, this mod does not apply EFT's EXP/roubles/reputation reward balance, as it no longer fits SPT v4.1.x — start conditions and quest-chain requirements are mostly kept as-is from SPT. Instead, it mainly changes kill conditions, map conditions, issued gear, turn-in items, and some rewards.

## Requirements

- SPT server `~4.1.3`
- .NET 10 SDK (only if building from source)
- [WTT-CommonLib] `>=3.0.6` — **(Required)**
- [WTT-ContentBackport] `2.0.1` — **(Required)**

## Installation

1. Download `sptQuestLive.zip` for the version you want from [Releases](../../releases).
2. Extract it and overwrite the resulting `SPT_Runtime` and `BepInEx` folders into your SPT install root (e.g. `C:\SPT`, the parent folder of the `SPT_Runtime` folder containing `SPT.Server.exe`). (`SPT_Runtime/user/mods/sptQuestLive/...` and `BepInEx/plugins/SptQuestLive.Client/...` structure)
   - The `BepInEx` folder is the client plugin that handles the UI side of the [config flags](#config-flags-dbconfigjson) below. Just install it alongside the rest — it doesn't affect anything else.
   - If your server root *is* the `SPT_Runtime` folder itself (older SPT layout), you can instead copy just the `SPT_Runtime\user\mods\sptQuestLive` folder from the zip into your server root's `user\mods\`.
3. Restart the server.

## Excluded from scope

Lightkeeper, Ref, and BTR trader quests are excluded from changes. Ref's quests are mostly tied to Arena and don't need touching, and Lightkeeper's quests are end-game content that will be reviewed separately later.

## Config flags (db/Config.json)

Mod behavior can be adjusted through the `db/Config.json` file.

- `questContentEnabled` (default `true`): toggles all of this mod's quest/reward content (quest overrides, quest removals, alternative conditions, trader assort unlocks/removals, bot/static loot additions, hideout fixes, locale changes, etc.) as a whole. Set to `false` to disable all of it and fall back to vanilla SPT behavior.
- `disableSalesVolumeRequirement` (default `true`, experimental): removes the sales-volume requirement from trader loyalty level-ups. Set to `true` and level-ups depend only on level and reputation, with the sales-volume display also hidden in the trade UI. This always works independently of `questContentEnabled`.


## Included quest overrides

Quests defined in `db/quests.json`, split into per-trader documents:

- [Prapor](readme/quests/prapor.en.md)
- [Therapist](readme/quests/therapist.en.md)
- [Skier](readme/quests/skier.en.md)
- [Peacekeeper](readme/quests/peacekeeper.en.md)
- [Mechanic](readme/quests/mechanic.en.md)
- [Ragman](readme/quests/ragman.en.md)
- [Jaeger](readme/quests/jaeger.en.md)
- [Fence](readme/quests/fence.en.md)

## Locales

- Korean (`db/locales/kr.json`)
- English (`db/locales/en.json`)

## License

MIT
