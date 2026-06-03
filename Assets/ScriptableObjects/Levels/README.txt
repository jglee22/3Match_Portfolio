레벨 데이터 만들기
====================
1. Project 창에서 우클릭 → Create → 3Match → Level Data
2. 아래 3개 예시로 생성 후 GridManager의 Levels 배열에 할당

| 파일명   | maskResourcePath   | goalScore | timeLimit |
|----------|--------------------|-----------|-----------|
| Level01  | Masks/Mask_Level01 | 800       | 90        |
| Level02  | Masks/Mask_Level02 | 1200      | 75        |
| Level03  | Masks/Mask_Heart   | 1500      | 60        |

Levels 배열이 비어 있으면 Mask_Level01~03 중 랜덤 마스크로 플레이합니다.
