# 3Match Portfolio

Unity 6 기반 3매치 퍼즐. CSV 마스크 보드, 특수 블록, 연쇄 콤보, 타임 어택 목표 점수 클리어.

---

## 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [핵심 기능](#핵심-기능)
3. [퍼즐 처리 흐름](#퍼즐-처리-흐름)
4. [주요 클래스 역할](#주요-클래스-역할)
5. [CSV 마스크 보드](#csv-마스크-보드)
6. [특수 블록 규칙](#특수-블록-규칙)
7. [트러블슈팅](#트러블슈팅)
8. [개선 로드맵](#개선-로드맵)
9. [실행 방법](#실행-방법)
10. [플레이 영상 / GIF](#플레이-영상--gif)

---

## 프로젝트 개요

| 항목 | 내용 |
|------|------|
| 엔진 | Unity `6000.0.40f1` |
| 메인 씬 | `Assets/Scenes/Main.unity` |
| 라이브러리 | DOTween, TextMeshPro |

---

## 핵심 기능

- 인접 스왑, 매치 없으면 롤백
- 연쇄: 제거 → 낙하 → 리필 → 재검사 (`BoardResolver` 코루틴)
- 특수 블록 (규칙표 참고)
- CSV 비직사각형 보드
- 목표 점수 즉시 클리어 / 시간 종료 시 클리어·실패 분기
- 가능한 수 없을 때 자동 셔플
- `BlockPool`으로 블록 재사용 (Inspector 연결 시)

---

## 퍼즐 처리 흐름

```
[입력] BlockInput(OnMouseDown) → GridManager.SelectBlock
         ↓
[스왑] 인접 확인 → DOMove → MatchFinder.IsBlockInMatch
         ├─ 실패 → SwapBack → isProcessing = false
         └─ 성공 → BoardResolver.StartCascade()
                    ↓
              매치 검사 → 특수 블록 1개 생성(규칙) → 제거
                    ↓ Wait
              FillEmptySpaces (낙하·리필)
                    ↓ Wait
              매치 있으면 반복 / 없으면 입력 해제 + deadlock 셔플 검사
```

특수 블록 스왑 발동 시 `ActivateSpecialBlockSequential` → `BoardResolver.ContinueAfterSpecial()`.

---

## 주요 클래스 역할

| 클래스 | 역할 |
|--------|------|
| `GridManager` | 보드 배열, 좌표, 스폰, 스왑, 마스크, 셔플 |
| `MatchFinder` | 매치 탐색, 스왑 유효성 검사 |
| `BoardResolver` | 연쇄 루프 코루틴 (DelayedCall 대신 Wait) |
| `SpecialBlockHandler` | 특수 블록 생성 규칙·비주얼 |
| `BlockPool` | 블록 Get/Release |
| `LevelData` | 마스크 경로, 목표 점수, 제한 시간 |
| `GameManager` | 클리어/실패 종료 (중복 방지) |
| `GameTimer` / `ScoreManager` | 시간·점수 UI |
| `MaskLoader` | CSV → `bool[,]` |

---

## CSV 마스크 보드

- 경로: `Assets/Resources/Masks/*.csv`
- `1` = 블록 있음, `0` = 빈 칸
- `MaskLoader`가 Y축을 뒤집어 Unity 좌표에 맞춤

---

## 특수 블록 규칙

| 조건 | 블록 | 비고 |
|------|------|------|
| **3연쇄 이상** | Lightning (십자 전체) | 최우선 |
| **5매치 이상** | Bomb (3×3) | |
| **4매치** | RowClear / ColClear | 가로 4개 이상이면 가로줄, 아니면 세로줄 |
| 생성 위치 | 마지막 스왑 블록 | 없으면 매치 목록 첫 블록 |

한 번의 매치당 **특수 블록 1개만** 생성.

---

## 트러블슈팅

| 이슈 | 대응 |
|------|------|
| 연쇄 중 입력 가능 | `isProcessing`은 `BoardResolver` 종료 시에만 해제 |
| 애니 길이 변경 시 타이밍 어긋남 | `BoardResolver`의 `removeDelay` / `settleDelay` Inspector 조정 |
| 풀 미연결 | `GridManager.blockPool` 비우면 기존처럼 Instantiate/Destroy |
| 레벨 미할당 | `Assets/ScriptableObjects/Levels/README.txt` 참고 |

---

## 개선 로드맵

| 단계 | 내용 | 상태 |
|------|------|------|
| 0 | README 골격 | ✅ |
| 1 | 클리어/실패·즉시 클리어·빌드 이슈 | ✅ |
| 2 | 특수 블록 규칙 + `SpecialBlockHandler` | ✅ |
| 3 | `MatchFinder` / `BoardResolver` 분리 | ✅ |
| 4 | `LevelData` + deadlock 셔플 | ✅ (SO는 Inspector에서 생성) |
| 5 | `BlockPool` | ✅ (Inspector 연결 필요) |
| 6 | 플레이 영상/GIF | ⏳ |

---

## 실행 방법

1. Unity **6000.0.40f1** 로 프로젝트 열기
2. `Main.unity` 실행
3. (선택) `GridManager`에 `LevelData` 3개, `BlockPool` 할당
4. (선택) 빈 GameObject + `BlockPool` — `Block Prefab`에 기존 Block 프리팹 지정

---

## 플레이 영상 / GIF

고도화 완료 후 30~60초 영상 추가 예정.
