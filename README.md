# SptQuestLive

**Language: [한국어](README.md) | [English](README.en.md)**

[SPT (Single Player Tarkov)](https://sp-tarkov.com/) 서버용 퀘스트 오버라이드 모드입니다.
`db/quests.json`, `db/locales/*.json`에 정의한 내용으로 원본 퀘스트 조건/보상과 로케일 문자열을 서버 로딩 시점에 덮어씁니다.

## 모드 취지

EFT 최신 버전의 퀘스트를 SPT 기준으로 오버라이드합니다.
EFT는 퀘스트 구조나 연계, 상인 평판구조까지 대대적으로 바뀌었기 때문에 현재 SPT버전에 어울리지 않는 보상 밸런스인 EXP, 루블, 평판 보상은 적용하지 않고, 시작 조건·연계 퀘스트 같은 체인 조건은 SPT 기준을 그대로 유지합니다. 대신 킬 조건, 맵 조건, 지급 장비, 제출 아이템 등을 주로 수정합니다.

## 요구사항

- SPT 서버 `~4.1.2`
- .NET 10 SDK (직접 빌드할 경우)
- **(필수)** [WTT-CommonLib] `>=3.0.4` (서버용 WTT-ServerCommonLib + 클라이언트용 WTT-ClientCommonLib 둘 다) —
  Pest Control 등 일부 퀘스트가 이 모드의 커스텀 트리거 존 기능(`db/CustomQuestZones/`)을 사용합니다.
- (권장) [WTT-ContentBackport] `2.0.0` — 없어도 정상 작동하지만, 일부 퀘스트의
  지급장비·부착물이 이 모드가 추가하는 아이템을 사용하는 경우 해당 아이템만 빠지고 지급될 수 있습니다.

## 설치

1. [Releases](../../releases)에서 원하는 버전의 `sptQuestLive.zip`을 다운로드합니다.
2. 압축을 풀어 나오는 `SPT_Runtime`, `BepInEx` 폴더를 SPT 설치 루트(예: `C:\SPT`, `SPT.Server.exe`가 든 `SPT_Runtime` 폴더의 상위 폴더)에 그대로 덮어씁니다. (`SPT_Runtime/user/mods/sptQuestLive/...`, `BepInEx/plugins/SptQuestLive.Client/...` 구조)
   - `BepInEx` 폴더에 들어있는 건 [실험적 기능](#실험적-기능-상인-거래량-조건-제거)의 UI 숨김 기능을 위한 클라이언트 플러그인입니다. 실험적 기능을 안 쓰더라도 그냥 같이 깔아두면 되고, 나머지 기능에는 영향이 없습니다.
   - `SPT_Runtime` 폴더 자체가 서버 루트인 구조(구버전 SPT)라면, 압축 안의 `SPT_Runtime\user\mods\sptQuestLive` 폴더만 서버 루트의 `user\mods\`에 복사해도 됩니다.
3. 서버를 재시작합니다.

## 수정 범위 제외

라이트키퍼(Lightkeeper), 레프(Ref) 트레이더 퀘스트는 수정 대상에서 제외합니다. 레프 퀘스트는 대부분 아레나(Arena) 연동 퀘스트라 손댈 필요가 없고, 라이트키퍼는 엔드컨텐츠 이후 퀘스트라 추후 별도로 검토할 예정입니다.

## 실험적 기능: 상인 거래량 조건 제거

`db/TraderLevelConfig.json`의 `disableSalesVolumeRequirement` 값으로 켜고 끌 수 있는 실험적 베타 기능입니다.
- `false` (기본값): 원본 그대로. 상인 로열티 레벨업에 레벨/평판/거래량 조건이 모두 그대로 적용되고, UI에도 거래량이 표시됩니다.
- `true`: 모든 상인의 거래량 조건을 제거합니다. 레벨업에는 레벨·평판 조건만 남고, 거래 UI에서도 거래량 표시가 사라집니다.

필요하면 직접 이 값을 `true`로 바꿔서 사용하시면 됩니다.


## 포함된 퀘스트 오버라이드

`db/quests.json`에 정의된 퀘스트 목록입니다.

- Stirrup (소동)
- Setup (준비작업)
- The Tarkov Import (구 Test Drive - Part 1) (타르코프 수입품)
- Power of Persuasion (구 Test Drive - Part 2) (설득의 힘)
- Job for a Patriot (구 Test Drive - Part 3) (애국자를 위한 일)
- Getting Some Air (구 Test Drive - Part 4) (잠깐 바람 쐬기)
- Easy-Breezy (구 Test Drive - Part 5) (식은 죽 먹기)
- Unique Experience (구 Test Drive - Part 6) (독특한 경험)
- Forgotten Oaths (잊혀진 맹세)
- Forced Alliance (강제된 동맹)
- Last Spurt - Pioneer (마지막 질주 - 개척자)
- Enough Drinks for That One (술은 그만)
- Friend from Norvinsk - Part 3 (노르빈스크에서 온 친구 - 파트 3)
- Kind of Sabotage (일종의 방해 공작)
- Fishing Gear (낚시 장비)
- Search Mission (수색 임무)
- Debtor (채무자)
- House Arrest - Part 1 (가택 연금 - 파트 1)
- Worst Job in the World (세계 최악의 직업)
- Best Job in the World (세계 최고의 직업)
- Peacekeeping Mission (평화 유지 임무)
- Trophies (전리품)
- Long Line (대기줄)
- The Tarkov Shooter - Part 1~4, 6~8 (타르코프의 저격수 - 파트 1~4, 6~8)
- Long Road (긴 도로)
- Grenadier (척탄병)
- The Punisher - Part 1~3 (퍼니셔 - 파트 1~3)
- Golden Swag (황금빛 스웩)
- Pest Control (해충구제)
- Rite of Passage (통과의례)
- The Survivalist Path - Unprotected but Dangerous (생존가의 길 - 무방비하지만 위험한)
- The Survivalist Path - Thrifty (생존가의 길 - 비축)
- The Survivalist Path - Zhivchik (생존가의 길 - 지브치크)
- The Survivalist Path - Wounded Beast (생존가의 길 - 상처 입은 짐승)
- The Survivalist Path - Tough Guy (생존가의 길 - 상남자)
- The Survivalist Path - Cold Blooded (생존가의 길 - 냉혈한)
- The Survivalist Path - Eagle-Owl (생존가의 길 - 수리부엉이)
- The Survivalist Path - Combat Medic (생존가의 길 - 의무병)
- The Survivalist Path - Junkie (생존가의 길 - 약쟁이)
- The Huntsman Path - Trophy (사냥꾼의 길 - 트로피)
- The Huntsman Path - Forest Cleaning (사냥꾼의 길 - 삼림 청소)
- The Huntsman Path - Controller (사냥꾼의 길 - 상황 통제)
- The Huntsman Path - Justice (사냥꾼의 길 - 정의)
- The Huntsman Path - Evil Watchman (사냥꾼의 길 - 사악한 경비원)
- The Huntsman Path - Eraser - Part 1 (사냥꾼의 길 - 말살자 - 파트 1)
- The Huntsman Path - Sadist (사냥꾼의 길 - 사디스트)
- The Huntsman Path - Relentless (사냥꾼의 길 - 가차없는)
- The Huntsman Path - Big Game (사냥꾼의 길 - 빅 게임)
- The Huntsman Path - Crooked Cop (사냥꾼의 길 - 비리 경찰)
- The Cleaner (청소부)
- A Shooter Born in Heaven (천국에서 태어난 저격수)
- Psycho Sniper (사이코 저격수)
- Gunsmith - Part 1~19, 22, 24 (건스미스 - 파트 1~19, 22, 24)
- Gunsmith - Old Friend's Request (건스미스 - 옛 친구의 부탁)
- Health Care Privacy - Part 1~5 (의료 개인 정보 보호 - 파트 1~5)

## 로케일

- 한국어 (`db/locales/kr.json`)
- 영어 (`db/locales/en.json`)

## 라이선스

MIT
