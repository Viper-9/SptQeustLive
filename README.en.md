# SptQuestLive

A quest override mod for [SPT (Single Player Tarkov)](https://sp-tarkov.com/) servers.
It overwrites the original quest conditions/rewards and locale strings at server load time, using the data defined in `db/quests.json` and `db/locales/*.json`.

## Requirements

- SPT server `~4.1.2`
- .NET 10 SDK (only if building from source)
- (Optional) [WTT-ContentBackport](https://hub.sp-tarkov.com/) `2.0.0` — the mod works fine without it, but some
  quests' issued gear/attachments reference items added by ContentBackport, and those specific items may be skipped
  when it's not installed.

## Installation

1. Download `sptQuestLive.zip` for the version you want from [Releases](../../releases).
2. Extract it and overwrite the resulting `SPT_Runtime` folder into your SPT install root (e.g. `F:\SPT4.1.2`, the parent folder of the `SPT_Runtime` folder containing `SPT.Server.exe`). (`SPT_Runtime/user/mods/sptQuestLive/...` structure)
   - If your server root *is* the `SPT_Runtime` folder itself (older SPT layout), you can instead copy just the `SPT_Runtime\user\mods\sptQuestLive` folder from the zip into your server root's `user\mods\`.
3. Restart the server.

## Excluded from scope

Lightkeeper and Ref trader quests are excluded from changes. Ref's quests are mostly tied to Arena and don't need touching, and Lightkeeper's quests are end-game content that will be reviewed separately later.


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
- The Survivalist Path - Unprotected but Dangerous
- The Survivalist Path - Thrifty
- The Survivalist Path - Zhivchik
- The Survivalist Path - Wounded Beast
- The Survivalist Path - Tough Guy
- The Survivalist Path - Cold Blooded
- The Survivalist Path - Eagle-Owl
- The Survivalist Path - Combat Medic
- The Survivalist Path - Junkie
- The Cleaner
- A Shooter Born in Heaven
- Psycho Sniper

## Locales

- Korean (`db/locales/kr.json`)
- English (`db/locales/en.json`)

## License

MIT
