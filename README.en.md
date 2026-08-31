# SptQuestLive

**Language: [한국어](README.md) | [English](README.en.md)**

A quest override mod for [SPT (Single Player Tarkov)](https://sp-tarkov.com/) servers.
It overwrites the original quest conditions/rewards and locale strings at server load time, using the data defined in `db/quests.json` and `db/locales/*.json`.
Some quests also touch hideout production recipes (`db/hideout/production.json`) or trader assort/shop unlocks (`db/TraderAssortAdditions.json`, `db/QuestAssortUnlocks.json`).

## Purpose

Overrides quests to match the latest version of EFT, on top of SPT.
Since EFT has drastically changed quest structure, quest chains, and the trader reputation system, this mod does not apply EFT's EXP/roubles/reputation reward balance, as it no longer fits the current SPT version — start conditions and quest-chain requirements are kept as-is from SPT. Instead, it mainly changes kill conditions, map conditions, issued gear, and turn-in items.

## Requirements

- SPT server `~4.1.2`
- .NET 10 SDK (only if building from source)
- [WTT-CommonLib] `>=3.0.4` — **(Required)**
- [WTT-ContentBackport] `2.0.0` — **(Required)**

## Installation

1. Download `sptQuestLive.zip` for the version you want from [Releases](../../releases).
2. Extract it and overwrite the resulting `SPT_Runtime` and `BepInEx` folders into your SPT install root (e.g. `C:\SPT`, the parent folder of the `SPT_Runtime` folder containing `SPT.Server.exe`). (`SPT_Runtime/user/mods/sptQuestLive/...` and `BepInEx/plugins/SptQuestLive.Client/...` structure)
   - The `BepInEx` folder is the client plugin for the [experimental feature](#experimental-feature-removing-the-trader-sales-volume-requirement)'s UI-hiding part. It's harmless to install even if you don't use that feature.
   - If your server root *is* the `SPT_Runtime` folder itself (older SPT layout), you can instead copy just the `SPT_Runtime\user\mods\sptQuestLive` folder from the zip into your server root's `user\mods\`.
3. Restart the server.

## Excluded from scope

Lightkeeper and Ref trader quests are excluded from changes. Ref's quests are mostly tied to Arena and don't need touching, and Lightkeeper's quests are end-game content that will be reviewed separately later.

## Experimental feature: removing the trader sales-volume requirement

An experimental beta feature toggled via `disableSalesVolumeRequirement` in `db/TraderLevelConfig.json`.

- `false` (default): unchanged behavior. Trader loyalty level-ups still require level, reputation, and sales volume, and the sales volume is still shown in the UI.
- `true`: removes the sales-volume (`minSalesSum`) requirement from every trader's loyalty levels. Level-ups then depend only on level and reputation, and the sales-volume display is also hidden in the trade UI. (Hiding it in the UI requires `BepInEx/plugins/SptQuestLive.Client` to be installed — already included if you followed [Installation](#installation) above.)

Set this to `true` yourself if you want to use it.


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
