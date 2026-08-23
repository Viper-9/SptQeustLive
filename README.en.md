# SptQuestLive

**Language: [한국어](README.md) | [English](README.en.md)**

A quest override mod for [SPT (Single Player Tarkov)](https://sp-tarkov.com/) servers.
It overwrites the original quest conditions/rewards and locale strings at server load time, using the data defined in `db/quests.json` and `db/locales/*.json`.

## Purpose

Overrides quests to match the latest version of EFT, on top of SPT.
Since EFT has drastically changed quest structure, quest chains, and the trader reputation system, this mod does not apply EFT's EXP/roubles/reputation reward balance, as it no longer fits the current SPT version — start conditions and quest-chain requirements are kept as-is from SPT. Instead, it mainly changes kill conditions, map conditions, issued gear, and turn-in items.

## Requirements

- SPT server `~4.1.2`
- .NET 10 SDK (only if building from source)
- **(Required)** [WTT-CommonLib] `>=3.0.4` (both WTT-ServerCommonLib and WTT-ClientCommonLib) —
  Pest Control and other quests rely on this mod's custom trigger zone feature (`db/CustomQuestZones/`).
- (Recommended) [WTT-ContentBackport] `2.0.0` — the mod works fine without it, but some
  quests' issued gear/attachments reference items added by ContentBackport, and those specific items may be skipped
  when it's not installed.

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

Quests defined in `db/quests.json`:

- Stirrup
- Setup
- The Tarkov Import (formerly Test Drive - Part 1)
- Power of Persuasion (formerly Test Drive - Part 2)
- Job for a Patriot (formerly Test Drive - Part 3)
- Getting Some Air (formerly Test Drive - Part 4)
- Easy-Breezy (formerly Test Drive - Part 5)
- Unique Experience (formerly Test Drive - Part 6)
- Forgotten Oaths
- Forced Alliance
- Last Spurt - Pioneer
- Enough Drinks for That One
- Friend from Norvinsk - Part 3
- Kind of Sabotage
- Fishing Gear
- Search Mission
- Debtor
- House Arrest - Part 1
- Worst Job in the World
- Best Job in the World
- Peacekeeping Mission
- Trophies
- Long Line
- The Tarkov Shooter - Part 1~4, 6~8
- Long Road
- Grenadier
- The Punisher - Part 1~3
- Golden Swag
- Pest Control
- Rite of Passage
- The Survivalist Path - Unprotected but Dangerous
- The Survivalist Path - Thrifty
- The Survivalist Path - Zhivchik
- The Survivalist Path - Wounded Beast
- The Survivalist Path - Tough Guy
- The Survivalist Path - Cold Blooded
- The Survivalist Path - Eagle-Owl
- The Survivalist Path - Combat Medic
- The Survivalist Path - Junkie
- The Huntsman Path - Trophy
- The Huntsman Path - Forest Cleaning
- The Huntsman Path - Controller
- The Huntsman Path - Justice
- The Huntsman Path - Evil Watchman
- The Huntsman Path - Eraser - Part 1
- The Huntsman Path - Sadist
- The Huntsman Path - Relentless
- The Huntsman Path - Big Game
- The Huntsman Path - Crooked Cop
- The Cleaner
- A Shooter Born in Heaven
- Psycho Sniper
- Gunsmith - Part 1~19, 22, 24
- Gunsmith - Old Friend's Request
- Health Care Privacy - Part 1~5

## Locales

- Korean (`db/locales/kr.json`)
- English (`db/locales/en.json`)

## License

MIT
