# SptQuestLive

**Language: [한국어](README.md) | [English](README.en.md)**

[SPT (Single Player Tarkov)](https://sp-tarkov.com/) 서버용 퀘스트 오버라이드 모드입니다.
`db/quests.json`, `db/locales/*.json`에 정의한 내용으로 원본 퀘스트 조건/보상과 로케일 문자열을 서버 로딩 시점에 덮어씁니다.

## 요구사항

- SPT 서버 `~4.1.2`
- .NET 10 SDK (직접 빌드할 경우)
- (선택) [WTT-ContentBackport](https://hub.sp-tarkov.com/) `2.0.0` — 없어도 정상 작동하지만, 일부 퀘스트
  지급장비가 이 모드가 추가하는 아이템을 사용하는 경우 해당 아이템만 빠지고 지급됩니다.

## 설치

1. [Releases](../../releases)에서 원하는 버전의 `sptQuestLive.zip`을 다운로드합니다.
2. 압축을 풀어 나오는 `SPT_Runtime` 폴더를 SPT 설치 루트(예: `F:\SPT4.1.2`, `SPT.Server.exe`가 든 `SPT_Runtime` 폴더의 상위 폴더)에 그대로 덮어씁니다. (`SPT_Runtime/user/mods/sptQuestLive/...` 구조)
   - `SPT_Runtime` 폴더 자체가 서버 루트인 구조(구버전 SPT)라면, 압축 안의 `SPT_Runtime\user\mods\sptQuestLive` 폴더만 서버 루트의 `user\mods\`에 복사해도 됩니다.
3. 서버를 재시작합니다.

## 수정 범위 제외

라이트키퍼(Lightkeeper), 레프(Ref) 트레이더 퀘스트는 수정 대상에서 제외합니다. 레프 퀘스트는 대부분 아레나(Arena) 연동 퀘스트라 손댈 필요가 없고, 라이트키퍼는 엔드컨텐츠 이후 퀘스트라 추후 별도로 검토할 예정입니다.

The Tarkov Shooter - Part 5는 의도적으로 오버라이드하지 않습니다. 현재 라이브 버전 계정 덤프를 확인한 결과 이 퀘스트에 해당하는 항목 자체가 사라졌고, 이후 파트 6·7·8이 각각 한 칸씩 당겨져 있어 SPT 원본과 1:1로 대응되는 덤프 데이터가 없습니다. 그래서 SPT 원본 상태(야간 21:00~05:00, Customs 한정) 그대로 둡니다.

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
- The Tarkov Shooter - Part 1 (타르코프의 저격수 - 파트 1)
- The Tarkov Shooter - Part 2 (타르코프의 저격수 - 파트 2)
- The Tarkov Shooter - Part 3 (타르코프의 저격수 - 파트 3)
- The Tarkov Shooter - Part 4 (타르코프의 저격수 - 파트 4)
- The Tarkov Shooter - Part 6 (타르코프의 저격수 - 파트 6)
- The Tarkov Shooter - Part 7 (타르코프의 저격수 - 파트 7)
- The Tarkov Shooter - Part 8 (타르코프의 저격수 - 파트 8)
- Long Road (긴 도로)
- Grenadier (척탄병)
- The Punisher - Part 1 (퍼니셔 - 파트 1)
- The Punisher - Part 2 (퍼니셔 - 파트 2)
- The Punisher - Part 3 (퍼니셔 - 파트 3)
- Golden Swag (황금빛 스웩)
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

## 로케일

- 한국어 (`db/locales/kr.json`)
- 영어 (`db/locales/en.json`)

## 라이선스

MIT
