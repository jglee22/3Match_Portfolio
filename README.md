# 3Match Portfolio

Unity 6 기반 3매치 퍼즐. CSV 마스크 보드, 특수 블록, 연쇄 콤보, 타임 어택 + 목표 점수 클리어.

---

## 플레이 영상

<!-- GIF 또는 영상 링크 -->
<!-- 예: ![Gameplay](./Docs/gameplay.gif) -->
<!-- 예: [YouTube](https://...) -->

*(영상/GIF 추가 예정)*

---

## 스크린샷

<!-- 예: ![Main](./Docs/screenshot_main.png) -->

| | |
|---|---|
| 메인 플레이 | *(추가 예정)* |
| 특수 블록 전환 | *(추가 예정)* |
| 클리어 / 실패 | *(추가 예정)* |

---

## 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [핵심 기능](#핵심-기능)
3. [퍼즐 처리 흐름](#퍼즐-처리-흐름)
4. [주요 클래스 역할](#주요-클래스-역할)
5. [CSV 마스크 보드](#csv-마스크-보드)
6. [특수 블록 규칙](#특수-블록-규칙)
7. [트러블슈팅](#트러블슈팅)
8. [실행 방법](#실행-방법)

---

## 프로젝트 개요

| 항목 | 내용 |
|------|------|
| 엔진 | Unity `6000.0.40f1` |
| 메인 씬 | `Assets/Scenes/Main.unity` |
| 스크립트 | `Assets/Scripts/` (12개) |
| 라이브러리 | DOTween, TextMeshPro |

---

## 핵심 기능

- 인접 블록 스왑, 매치 없으면 롤백 (`MatchFinder.IsBlockInMatch`)
- 연속 **3개 이상** 같은 과일만 매치 (`MatchFinder` run-length 탐색)
- 연쇄: 제거 → 낙하·리필 → 재검사 (`BoardResolver` 코루틴 + `WaitForSeconds`)
- 특수 블록 생성 규칙 + 리필 후 **같은 칸에서 일반→특수 전환** 연출
- CSV 비직사각형 보드 (6종 마스크, Resources)
- 목표 점수 **즉시 클리어** / 시간 종료 시 클리어·실패 분기 (`EndGameClear` / `EndGameFail`)
- 가능한 수 없을 때 자동 셔플
- `LevelData` ScriptableObject, 테스트용 마스크 고정 옵션
- `BlockPool` 블록 재사용 (Inspector 연결 시, 미연결 시 Instantiate/Destroy)

---

## 퍼즐 처리 흐름

### 일반 스왑 → 연쇄

```
Block.OnMouseDown
  → GridManager.SelectBlock
  → 인접 스왑 (DOTween DOMove, 0.2s)
  → MatchFinder.IsBlockInMatch
       ├─ 실패: SwapBack → isProcessing = false
       └─ 성공: BoardResolver.StartCascade()
              ↓
         [CascadeRoutine — 코루틴 while 루프]
         1. FindAllMatches() — 0개면 종료 + deadlock 셔플 검사
         2. 3개 미만이면 연쇄 중단 (제거 안 함)
         3. chainCount++
         4. SpecialBlockHandler.PlanSpecialSpawn() — 조건·좌표만 결정
         5. 매치 블록 전부 ClearBlockAt
         6. WaitForSeconds(removeDelay)        — 기본 0.25s
         7. FillEmptySpaces()                  — CollapseColumn + 위에서 리필 낙하
         8. WaitForSeconds(settleDelay + refillFallDuration)
         9. (특수 조건 시) PlaySpecialSpawnTransition()
        10. WaitForSeconds(settleDelay * 0.3)
        11. 1번으로 (연쇄 계속)
```

### 특수 블록 생성 연출 (`PlaySpecialSpawnTransition`)

특수 조건이 맞으면 **다른 빈 칸과 동일하게** 먼저 일반 리필된 뒤:

```
리필된 일반 블록 유지 (specialNormalHoldTime)
  → 축소·페이드 아웃 (specialNormalVanishTime)
  → 같은 GameObject를 특수 블록으로 전환 (ApplySpecialToBlock + DOScale)
```

### 특수 블록 스왑 발동

```
GridManager.SwapAndActivateSpecialBlock
  → ActivateSpecialBlockSequential (GridManager 코루틴)
  → 범위 제거 (폭탄 3×3 / 번개 십자 / 가로·세로 줄)
  → FillEmptySpaces
  → BoardResolver.ContinueAfterSpecial()
```

---

## 주요 클래스 역할

| 클래스 | 역할 |
|--------|------|
| `GridManager` | 보드 배열, 스왑, 리필, 셔플, 특수 전환 연출, 특수 스왑 발동 |
| `MatchFinder` | 연속 3+ 매치 탐색, 스왑 유효성 검사 (과일 4종만) |
| `BoardResolver` | 연쇄 루프 코루틴 (`removeDelay`, `settleDelay`) |
| `SpecialBlockHandler` | `PlanSpecialSpawn` (규칙·좌표), `ApplySpecialToBlock` (비주얼) |
| `Block` | `OnMouseDown` 입력, 타입·스프라이트 |
| `BlockPool` | 블록 Get/Release |
| `LevelData` | 마스크 경로, 목표 점수, 제한 시간 (SO) |
| `GameManager` | `EndGameClear` / `EndGameFail`, 재시작 |
| `GameTimer` | 제한 시간, 종료 시 점수로 클리어/실패 판정 |
| `ScoreManager` | 점수 UI, 목표 달성 시 즉시 클리어 |
| `MaskLoader` | CSV → `bool[,]` |
| `SoundManager` | BGM / SFX |

> 스왑 타이밍은 `GridManager`의 `DOVirtual.DelayedCall`(0.25s). 연쇄 루프는 `BoardResolver` 코루틴의 `WaitForSeconds`로 처리.

---

## CSV 마스크 보드

- 경로: `Assets/Resources/Masks/*.csv`
- `1` = 블록 있음, `0` = 빈 칸
- `MaskLoader`가 CSV Y축을 뒤집어 Unity 좌표에 맞춤
- 마스크 로드 우선순위:
  1. `testModeLockMask == true` → `testMaskResourcePath` (기본 `Masks/Mask_Level01`)
  2. `levels[]` 할당됨 → `LevelData.maskResourcePath`
  3. 그 외 → Level01~03 중 랜덤

---

## 특수 블록 규칙

한 번의 매치당 **최대 1개**. 우선순위는 아래 순서.

| 조건 | 블록 | 범위 |
|------|------|------|
| **3연쇄 이상** (`chainCount >= 3`) | Lightning | 해당 열 + 해당 행 전체 |
| **5매치 이상** | Bomb | 3×3 |
| **4매치** | RowClear / ColClear | 가로줄 또는 세로줄 전체 |

- **생성 위치**: 마지막 스왑 블록 (`LastMovedBlock`) 우선, 없으면 매치 목록 첫 블록
- **4매치 줄 방향**: 해당 칸 기준 가로 연속 4+ → RowClear, 아니면 ColClear
- **연쇄 카운트**: 매치 처리마다 `IncrementChainCount()` 후 판정 (첫 매치 = 1)

---

## 트러블슈팅

| 이슈 | 원인 / 대응 |
|------|-------------|
| 연쇄 중 입력됨 | `isProcessing`은 `BoardResolver` 종료·`SwapBack` 완료 시에만 해제 |
| 타이밍 어긋남 | `BoardResolver`: `removeDelay`, `settleDelay` / `GridManager`: `refillFallDuration`, 특수 연출 3종 시간 Inspector 조정 |
| 2개만 사라짐 | 매치는 3개 이상만 처리; 특수는 리필 후 **전환** (2개만 제거하지 않음) |
| 블록 겹침 | `CollapseColumn` 열 압축 방식 + `SpawnBlockAt` 중복 스폰 방지 |
| 풀 미연결 | `GridManager.blockPool` 비우면 Instantiate/Destroy |
| 레벨 SO | `Assets/ScriptableObjects/Levels/README.txt` 참고 |
| 테스트 마스크 고정 | `GridManager` → `Test Mode Lock Mask` 체크 |

---

## 실행 방법

1. Unity **6000.0.40f1** 로 프로젝트 열기
2. `Assets/Scenes/Main.unity` → Play
3. **(선택)** `GridManager` → `Test Mode Lock Mask` 로 Level01 고정
4. **(선택)** `Create → 3Match → Level Data` 생성 후 `levels` 배열 할당
5. **(선택)** 빈 GameObject + `BlockPool` 컴포넌트, Block 프리팹 지정 → `GridManager.blockPool` 연결
