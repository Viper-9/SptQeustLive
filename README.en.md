# SptQuestLive

A quest override mod for [SPT (Single Player Tarkov)](https://sp-tarkov.com/) servers.
It overwrites the original quest conditions/rewards and locale strings at server load time, using the data defined in `db/quests.json` and `db/locales/*.json`.

## Requirements

- SPT server `~4.1.2`
- .NET 10 SDK (only if building from source)
- (Optional) [WTT-ContentBackport](https://hub.sp-tarkov.com/) `2.0.0` — the mod works fine without it, but if a quest's
  issued gear references an item added by ContentBackport, that specific item will simply be skipped when it's not installed.

## Installation

1. Download `sptQuestLive.zip` for the version you want from [Releases](../../releases).
2. Extract it and overwrite the resulting `SPT_Runtime` folder into your SPT install root (e.g. `F:\SPT4.1.2`, the parent folder of the `SPT_Runtime` folder containing `SPT.Server.exe`). (`SPT_Runtime/user/mods/sptQuestLive/...` structure)
   - If your server root *is* the `SPT_Runtime` folder itself (older SPT layout), you can instead copy just the `SPT_Runtime\user\mods\sptQuestLive` folder from the zip into your server root's `user\mods\`.
3. Restart the server.

## Excluded from scope

Lightkeeper and Ref trader quests are excluded from changes. Ref's quests are mostly tied to Arena and don't need touching, and Lightkeeper's quests are end-game content that will be reviewed separately later.

The Tarkov Shooter - Part 5 is intentionally left un-overridden. A live-account data dump shows this quest no longer exists in the current game version — Parts 6/7/8 have each shifted down by one — so there's no 1:1 live data to override it with. It's left as vanilla SPT (night 21:00–05:00, Customs only).

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
- The Tarkov Shooter - Part 1
- The Tarkov Shooter - Part 2
- The Tarkov Shooter - Part 3
- The Tarkov Shooter - Part 4
- The Tarkov Shooter - Part 6
- The Tarkov Shooter - Part 7
- The Tarkov Shooter - Part 8
- Long Road
- Grenadier
- The Punisher - Part 1
- The Punisher - Part 2
- The Punisher - Part 3
- Golden Swag
- Rite of Passage

## Locales

- Korean (`db/locales/kr.json`)
- English (`db/locales/en.json`)

## License

MIT
