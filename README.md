# SptQuestLive

[SPT (Single Player Tarkov)](https://sp-tarkov.com/) 서버용 퀘스트 오버라이드 모드입니다.
`db/quests.json`, `db/locales/*.json`에 정의한 내용으로 원본 퀘스트 조건/보상과 로케일 문자열을 서버 로딩 시점에 덮어씁니다.

## 요구사항

- SPT 서버 `~4.0.13`
- .NET 9 SDK (직접 빌드할 경우)

## 설치

1. [Releases](../../releases)에서 원하는 버전의 `sptQuestLive.zip`을 다운로드합니다.
2. 압축을 풀어 나오는 `user` 폴더를 SPT 서버 루트에 그대로 덮어씁니다. (`user/mods/sptQuestLive/...` 구조)
3. 서버를 재시작합니다.

## 포함된 퀘스트 오버라이드

`db/quests.json`에 정의된 퀘스트 목록입니다. 새 버전이 나올 때마다 [Releases](../../releases) 노트에서 변경 내역을 확인할 수 있습니다.

- Stirrup
- Test Drive - Part 1
- Setup
- Test Drive - Part 2
- Test Drive - Part 3
- Test Drive - Part 4
- Test Drive - Part 5

## 로케일

- 한국어 (`db/locales/kr.json`)
- 영어 (`db/locales/en.json`)

## 빌드

```powershell
.\package.ps1
```

`dotnet build -c Release`로 빌드한 뒤 `dist/user/mods/sptQuestLive` 구조로 스테이징하고, 결과를 `sptQuestLive.zip`으로 압축합니다.
이미 빌드된 결과물만 다시 압축하려면 `-SkipBuild` 옵션을 사용하세요.

```powershell
.\package.ps1 -SkipBuild
```

## 라이선스

MIT
