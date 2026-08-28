# SptQuestLive

**Language: [한국어](README.md) | [English](README.en.md)**

[SPT (Single Player Tarkov)](https://sp-tarkov.com/) 서버용 퀘스트 오버라이드 모드입니다.
`db/quests.json`, `db/locales/*.json`에 정의한 내용으로 원본 퀘스트 조건/보상과 로케일 문자열을 서버 로딩 시점에 덮어씁니다.
일부 퀘스트는 하이드아웃 제작법(`db/hideout/production.json`)이나 상인 판매 목록(`db/TraderAssortAdditions.json`, `db/QuestAssortUnlocks.json`)도 함께 건드립니다.

## 모드 취지

EFT 최신 버전의 퀘스트를 SPT 기준으로 오버라이드합니다.
EFT는 퀘스트 구조나 연계, 상인 평판구조까지 대대적으로 바뀌었기 때문에 현재 SPT버전에 어울리지 않는 보상 밸런스인 EXP, 루블, 평판 보상은 적용하지 않고, 시작 조건·연계 퀘스트 같은 체인 조건은 SPT 기준을 그대로 유지합니다. 대신 킬 조건, 맵 조건, 지급 장비, 제출 아이템 등을 주로 수정합니다.

## 요구사항

- SPT 서버 `~4.1.2`
- .NET 10 SDK (직접 빌드할 경우)
- [WTT-CommonLib] `>=3.0.4` — **(필수)**
- [WTT-ContentBackport] `2.0.0` — **(필수)**

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

`db/quests.json`에 정의된 퀘스트 목록입니다. 트레이더별로 문서를 분리했습니다.

- [Prapor](readme/quests/prapor.md)
- [Therapist](readme/quests/therapist.md)
- [Skier](readme/quests/skier.md)
- [Peacekeeper](readme/quests/peacekeeper.md)
- [Mechanic](readme/quests/mechanic.md)
- [Ragman](readme/quests/ragman.md)
- [Jaeger](readme/quests/jaeger.md)
- [Fence](readme/quests/fence.md)

## 포함된 상점/제작법 오버라이드

퀘스트 자체가 아닌 다른 데이터를 건드리는 항목입니다. 목록은 [readme/shop-production.md](readme/shop-production.md)를 참고하세요.

## 로케일

- 한국어 (`db/locales/kr.json`)
- 영어 (`db/locales/en.json`)

## 라이선스

MIT
