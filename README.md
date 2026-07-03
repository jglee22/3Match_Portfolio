# 3Match Portfolio

Unity 6 기반 3매치 퍼즐 + 모바일 서비스 흐름(로비, Mock 상점, Mock/AdMob 리워드 광고, 영구 재화).

---

## 플레이 영상

<img width="960" height="540" alt="Movie_003-ezgif com-resize" src="https://github.com/user-attachments/assets/00431110-b07f-4bac-8825-efd6cf7c08d5" />

인접 스왑, 연쇄, 특수 블록, 로비·상점·리워드 광고 흐름을 확인할 수 있습니다.

---

## 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [게임 플로우](#게임-플로우)
3. [핵심 기능](#핵심-기능)
4. [서비스 아키텍처 (Mock / AdMob)](#서비스-아키텍처-mock--admob)
5. [퍼즐 처리 흐름](#퍼즐-처리-흐름)
6. [주요 클래스](#주요-클래스)
7. [특수 블록 규칙](#특수-블록-규칙)
8. [Android 빌드](#android-빌드)
9. [트러블슈팅](#트러블슈팅)
10. [실행 방법](#실행-방법)

---

## 프로젝트 개요

| 항목 | 내용 |
|------|------|
| 엔진 | Unity `6000.0.40f1` |
| 시작 씬 | `Assets/Scenes/Lobby.unity` |
| 인게임 씬 | `Assets/Scenes/Main.unity` |
| 스크립트 | `Assets/Scripts/` |
| 외부 SDK | Google Mobile Ads Unity Plugin v11.2.0 |
| 라이브러리 | TextMeshPro (DOTween은 [별도 설치](#필수-에셋-별도-설치)) |
| UI 에셋 | Hyper Casual UI Pack (무료, `Assets/Hyper_Casual_UI/`) |

---

## 게임 플로우

```
Lobby
 ├─ Play      → Main (3매치 플레이)
 ├─ Shop      → Mock 상점 (Gold/Gem/Shuffle 구매)
 ├─ Reward    → 리워드 광고 (Mock 또는 AdMob)
 └─ Inventory → Shuffle Item 보유 수

Main
 ├─ 클리어/실패 → 보상 Gold·Gem 지급 → 저장
 ├─ 클리어     → 다음 라운드 / (테스트) 로비 복귀
 └─ 실패       → 로비 복귀
```

- 재화·Shuffle Item은 `PlayerPrefs`에 저장되어 **앱 재시작 후에도 유지**
- 인게임 우측 상단 **Shuffle (N)** 버튼으로 셔플 아이템 사용

---

## 핵심 기능

### 퍼즐

- 인접 스왑, 매치 없으면 롤백
- 연속 3개 이상 run-length 매치 (`MatchFinder`)
- 연쇄: 제거 → 낙하·리필 → 재검사 (`BoardResolver`)
- 특수 블록 생성 + 리필 후 같은 칸 전환 연출
- 특수 블록 연쇄 발동 (특수가 특수를 제거하면 해당 특수도 발동)
- CSV 비직사각형 보드, `LevelData` 다중 라운드
- 데드락·아이템 셔플 (매치 없는 배치 보장)
- `BlockPool` 블록 재사용

### 서비스

- 로비 UI (`LobbyManager`)
- Mock 상점 (`IShopService` / `MockShopService`)
- 리워드 광고 (`IAdService` / `MockAdService` / `AdMobAdService`)
- 클리어·실패 보상 (`RewardCalculator`)
- 영구 데이터 (`GameDataManager`)

---

## 서비스 아키텍처 (Mock / AdMob)

인터페이스 + Mock 구현으로 분리. 실 SDK는 구현체만 교체.

```
GameServicesBootstrap (DontDestroyOnLoad)
├── IShopService  → MockShopService
└── IAdService    → MockAdService (기본) / AdMobAdService
```

| 인터페이스 | Mock | 실연동 |
|------------|------|--------|
| `IShopService` | `MockShopService` | `UnityIapShopService` (확장 포인트) |
| `IAdService` | `MockAdService` | `AdMobAdService` |

### Reward 광고

| 모드 | 설명 |
|------|------|
| **Mock** | 2초 시뮬레이션, 성공/실패/취소. 에디터·빠른 테스트용 |
| **AdMob** | Google 테스트 광고 단위 ID. **실기기 빌드**에서 확인 |

- Gold 광고: +100 Gold
- Gem 광고: +1 Gem
- 광고 중 UI 연타·다른 패널 이동 차단

### AdMob 설정

1. **Assets → Google Mobile Ads → Settings**
   - Android App ID: `ca-app-pub-3940256099942544~3347511713` (테스트)
   - iOS App ID: `ca-app-pub-3940256099942544~1458002511` (테스트)
2. `GameServicesBootstrap` → **Ad Provider = AdMob**
3. Android/iOS **실기기 빌드** 후 Reward 테스트
4. 첫 광고 로드는 SDK 초기화 후 수 초 걸릴 수 있음 (재시도 시 정상 동작)

> App ID는 `~`(물결), 광고 단위 ID는 `/`(슬래시). 혼동하지 말 것.

---

## 퍼즐 처리 흐름

### 일반 스왑 → 연쇄

```
SelectBlock → 스왑 → MatchFinder.IsBlockInMatch
  ├─ 실패: SwapBack
  └─ 성공: BoardResolver.StartCascade()
         → FindAllMatches → 제거 → 낙하·리필 → 특수 생성 → 연쇄 반복
```

### 특수 블록 스왑 발동

```
SwapAndActivateSpecialBlock
  → ActivateSpecialChainRoutine (연쇄 특수 발동)
  → FillEmptySpaces
  → BoardResolver.ContinueAfterSpecial()
```

---

## 주요 클래스

| 클래스 | 역할 |
|--------|------|
| `GridManager` | 보드, 스왑, 리필, 셔플, 특수 발동 |
| `MatchFinder` | run-length 매치 탐색 |
| `BoardResolver` | 연쇄 코루틴 |
| `SpecialBlockHandler` | 특수 블록 규칙·비주얼 |
| `GameManager` | 클리어/실패, 라운드, 보상, 씬 전환 |
| `GameDataManager` | Gold/Gem/Shuffle `PlayerPrefs` |
| `CurrencyManager` | 재화 UI |
| `LobbyManager` | 로비 UI |
| `ShopManager` | Mock 상점 UI |
| `AdRewardManager` | 리워드 광고 UI |
| `GameServicesBootstrap` | Mock/AdMob 전환 |
| `SceneLoader` | Lobby ↔ Main |
| `ShuffleItemUI` | 인게임 셔플 버튼 |
| `RewardCalculator` | 클리어/실패 보상 계산 |
| `EndPanelView` | 결과 패널 연출 |

---

## 특수 블록 규칙

| 조건 | 블록 | 범위 |
|------|------|------|
| 3연쇄 이상 | Lightning | 해당 행 + 열 |
| 5매치 이상 | Bomb | 3×3 |
| 4매치 | RowClear / ColClear | 가로줄 또는 세로줄 |

- 셔플·연쇄 시작 시 `chainCount` 초기화
- 셔플 후 매치 없는 배치만 적용

---

## Android 빌드

### 사전 준비 (Unity Hub)

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

### Player Settings (Android)

| 항목 | 권장값 |
|------|--------|
| Scripting Backend | **IL2CPP** |
| Target Architectures | **ARM64** |
| Internet Access | Require |

> Mono 사용 시 ARM64가 비활성화되어 `Target architecture not specified` 오류 발생.

### 빌드 팁

- **Build APK**만 먼저 시도 (Build And Run 지양)
- 첫 IL2CPP 빌드는 20~40분 소요 가능
- 빌드 멈춤/취소 안 될 때: Unity 강제 종료 후 `Tools/KillStuckAndroidBuild.ps1` 실행
- AdMob 테스트: `Ad Provider = AdMob`으로 빌드

---

## 트러블슈팅

| 이슈 | 대응 |
|------|------|
| `Target architecture not specified` | IL2CPP + ARM64 체크 |
| 빌드 취소 안 됨 | `KillStuckAndroidBuild.ps1`로 java/gradle 종료 |
| AdMob 첫 재생 실패 | Wi-Fi 확인, 2~3초 후 재시도. SDK 초기화 대기 로직 포함 |
| App ID 오류 | Settings에 `~` 형식 App ID 입력 (광고 단위 ID 아님) |
| 셔플 후 3매치 붙음 | `ShuffleBoardRoutine` 검증·폴백 로직 적용됨 |
| 셔플 후 특수 블록 오발동 | `chainCount` 셔플·연쇄 시작 시 리셋 |
| `GameServicesBootstrap` 안 보임 | Lobby 씬에 배치됨. Play 중 DontDestroyOnLoad 섹션 확인 |

---

## 필수 에셋 (별도 설치)

이 레포에는 **유료 에셋 파일이 포함되어 있지 않습니다.** 아래를 설치하지 않으면 빌드·실행이 되지 않습니다.

| 에셋 | 용도 |
|------|------|
| [DOTween (Hotween v2)](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) | 블록 스왑·낙하·UI 연출 (`DG.Tweening`) |

1. Unity Asset Store에서 DOTween 구매·다운로드
2. 프로젝트에 `.unitypackage` Import → `Assets/Plugins/Demigiant/` 생성
3. **Tools → Demigiant → DOTween Utility Panel → Setup DOTween** 실행
4. `DOTweenSettings`는 설치 시 자동 생성됨 (`Assets/Resources/`)

---

## 실행 방법

### 에디터

1. Unity `6000.0.40f1`로 프로젝트 열기
2. 위 [필수 에셋](#필수-에셋-별도-설치) DOTween 설치
3. `Assets/Scenes/Lobby.unity` → Play
4. Play / Shop / Reward / Inventory 테스트

### 인게임만 빠르게

- `Assets/Scenes/Main.unity` 직접 실행 가능

### 옵션

- `GridManager` → `Test Mode Lock Mask` — Level01 마스크 고정
- `GridManager.levels` — `LevelData` SO 할당 시 다중 라운드
- `GameServicesBootstrap` → Ad Provider — Mock / AdMob
- `GameManager` → `testReturnToLobbyOnClear` — 클리어 시 로비 복귀 (테스트용)
