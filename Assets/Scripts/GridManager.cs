using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class BlockSprite
{
    public BlockType type;
    public Sprite sprite;
}
public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public float cellSize = 1.2f;

    [Header("Prefab & Parent")]
    public GameObject blockPrefab; // 하나의 블록 프리팹만 사용
    public Transform blocksParent;

    [Header("블록 타입과 스프라이트 매핑")]
    public List<BlockSprite> blockSprites;
    private Dictionary<BlockType, Sprite> spriteDict;

    private GameObject[,] blocks;
    private Block selectedBlock = null;
    private bool isProcessing = false;
    private int chainCount = 0;

    private bool[,] gridMask;

    // 일반 블록만 배열로 관리
    BlockType[] normalTypes = new BlockType[]
    {
        BlockType.Apple,
        BlockType.Banana,
        BlockType.Grape,
        BlockType.Orange
    };
    void Awake()
    {
        spriteDict = new Dictionary<BlockType, Sprite>();
        foreach (var entry in blockSprites)
        {
            spriteDict[entry.type] = entry.sprite;
        }
    }

    void Start()
    {
        blocks = new GameObject[width, height];
        LoadRandomMask();    // 마스크 랜덤 설정
        GenerateGrid();      // 그리드 생성
    }
    void LoadRandomMask()
    {
        string[] maskNames = {
            "Masks/Mask_Level01",
            "Masks/Mask_Level02",
            "Masks/Mask_Level03",
            "Masks/Mask_Level04",
            "Masks/Mask_Heart",
            "Masks/Mask_Tree",
        };

        int randomIndex = Random.Range(0, maskNames.Length);
        string selectedName = maskNames[randomIndex];

        TextAsset maskCsv = Resources.Load<TextAsset>(selectedName);
        if (maskCsv == null)
        {
            Debug.LogError($"마스크 파일을 찾을 수 없습니다: {selectedName}");
            return;
        }

        gridMask = MaskLoader.LoadMaskFromCSV(maskCsv);

        Debug.Log($"✅ 선택된 마스크: {selectedName}");
    }
    void GenerateGrid()
    {
        int width = gridMask.GetLength(0);
        int height = gridMask.GetLength(1);
        blocks = new GameObject[width, height]; // 반드시 여기서 초기화

        Vector2 offset = new Vector2((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f);
        float yOffset = height * 0.1f;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 마스킹된 칸이면 생성 스킵
                if (gridMask != null && !gridMask[x, y])
                    continue;

                Vector3 spawnPos = new Vector3(x * cellSize, y * cellSize, 0f) - (Vector3)offset + Vector3.up * yOffset;

                GameObject blockObj = Instantiate(blockPrefab, spawnPos, Quaternion.identity, blocksParent);
                blockObj.name = $"Block_{x}_{y}";

                Block block = blockObj.GetComponent<Block>();
                block.x = x;
                block.y = y;

                BlockType selectedType = GetNonMatchingType(x, y);
                block.SetType(selectedType, spriteDict[selectedType]);

                blocks[x, y] = blockObj;
            }
        }
    }

    // 특정 위치에 블록을 배치할 때 3개 이상 연속되지 않도록 안전한 타입을 고름
    BlockType GetNonMatchingType(int x, int y)
    {
        List<BlockType> possibleTypes = new List<BlockType>(normalTypes);

        if (x >= 2)
        {
            Block left1 = GetBlock(x - 1, y);
            Block left2 = GetBlock(x - 2, y);
            if (left1 != null && left2 != null && left1.blockType == left2.blockType)
            {
                possibleTypes.Remove(left1.blockType);
            }
        }

        if (y >= 2)
        {
            Block down1 = GetBlock(x, y - 1);
            Block down2 = GetBlock(x, y - 2);
            if (down1 != null && down2 != null && down1.blockType == down2.blockType)
            {
                possibleTypes.Remove(down1.blockType);
            }
        }

        return possibleTypes[Random.Range(0, possibleTypes.Count)];
    }

    // 블록 클릭 처리
    public void SelectBlock(Block block)
    {
        if (GameManager.Instance.isGameOver || isProcessing) return;

        if (selectedBlock == null)
        {
            selectedBlock = block;

            // 시각적 강조
            if (block.blockType == BlockType.Bomb || block.blockType == BlockType.Lightning)
                block.transform.DOScale(1.25f, 0.1f).SetEase(Ease.OutQuad);
            else
                block.transform.DOScale(0.65f, 0.1f).SetEase(Ease.OutQuad);
        }
        else
        {
            if (AreAdjacent(selectedBlock, block))
            {
                isProcessing = true;

                // 이전 선택 해제
                if (selectedBlock.blockType == BlockType.Bomb || selectedBlock.blockType == BlockType.Lightning)
                    selectedBlock.transform.DOScale(1f, 0.1f);
                else
                    selectedBlock.transform.DOScale(0.5f, 0.1f);

                // 특수 블록 클릭된 경우 처리
                if (selectedBlock.isSpecial)
                {
                    SwapAndActivateSpecialBlock(selectedBlock, block);
                    selectedBlock = null;
                    return;
                }
                else if (block.isSpecial)
                {
                    SwapAndActivateSpecialBlock(block, selectedBlock);
                    selectedBlock = null;
                    return;
                }
                else
                {
                    SwapBlocks(selectedBlock, block);
                    DOVirtual.DelayedCall(0.35f, () => isProcessing = false); // 잠금 해제
                }

                selectedBlock = null;
            }
            else
            {
                // 이전 선택 블록 해제 애니메이션
                if (selectedBlock.blockType == BlockType.Bomb || selectedBlock.blockType == BlockType.Lightning)
                    selectedBlock.transform.DOScale(1f, 0.1f);
                else
                    selectedBlock.transform.DOScale(0.5f, 0.1f);

                // 새 선택 블록 강조
                selectedBlock = block;
                if (block.blockType == BlockType.Bomb || block.blockType == BlockType.Lightning)
                    block.transform.DOScale(1.25f, 0.1f);
                else
                    block.transform.DOScale(0.65f, 0.1f);
            }
        }
    }

    bool AreAdjacent(Block a, Block b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    void SwapBlocks(Block a, Block b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        a.transform.DOMove(posB, 0.2f).SetLink(a.gameObject);
        b.transform.DOMove(posA, 0.2f).SetLink(b.gameObject);

        blocks[a.x, a.y] = b.gameObject;
        blocks[b.x, b.y] = a.gameObject;

        int tempX = a.x;
        int tempY = a.y;
        a.x = b.x;
        a.y = b.y;
        b.x = tempX;
        b.y = tempY;

        // DOTween이 끝난 후 검사 실행
        DOVirtual.DelayedCall(0.25f, () =>
        {
            if (!IsBlockInMatch(a) && !IsBlockInMatch(b))
            {
                SwapBack(a, b);
            }
            else
            {
                HandleMatches();
            }
        });
    }

    // 현재 그리드에서 매칭된 블록들을 모두 찾아 반환
    public List<Block> FindAllMatches()
    {
        List<Block> matchedBlocks = new List<Block>();

        // 가로 방향 검사
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                Block b1 = GetBlock(x, y);
                Block b2 = GetBlock(x + 1, y);
                Block b3 = GetBlock(x + 2, y);

                if (b1 != null && b2 != null && b3 != null)
                {
                    if (b1.blockType == b2.blockType && b2.blockType == b3.blockType)
                    {
                        if (!matchedBlocks.Contains(b1)) matchedBlocks.Add(b1);
                        if (!matchedBlocks.Contains(b2)) matchedBlocks.Add(b2);
                        if (!matchedBlocks.Contains(b3)) matchedBlocks.Add(b3);
                    }
                }
            }
        }

        // 세로 방향 검사
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                Block b1 = GetBlock(x, y);
                Block b2 = GetBlock(x, y + 1);
                Block b3 = GetBlock(x, y + 2);

                if (b1 != null && b2 != null && b3 != null)
                {
                    if (b1.blockType == b2.blockType && b2.blockType == b3.blockType)
                    {
                        if (!matchedBlocks.Contains(b1)) matchedBlocks.Add(b1);
                        if (!matchedBlocks.Contains(b2)) matchedBlocks.Add(b2);
                        if (!matchedBlocks.Contains(b3)) matchedBlocks.Add(b3);
                    }
                }
            }
        }

        return matchedBlocks;
    }

    // 그리드에서 (x, y) 위치에 있는 Block 가져오기
    Block GetBlock(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return blocks[x, y]?.GetComponent<Block>();
    }

    public void RemoveMatches()
    {
        List<Block> matchedBlocks = FindAllMatches();

        foreach (Block block in matchedBlocks)
        {
            // 그리드에서 제거
            blocks[block.x, block.y] = null;

            // 실제 오브젝트 제거
            Destroy(block.gameObject);
        }

        Debug.Log($"Removed {matchedBlocks.Count} matched blocks.");

        // 블록 제거 후 빈 칸 채우기
        FillEmptySpaces();
    }

    // 특정 x 좌표 열에서 블록을 아래로 내림
    void CollapseColumn(int x)
    {
        for (int y = 1; y < height; y++)
        {
            if (blocks[x, y] != null && blocks[x, y - 1] == null)
            {
                int targetY = y - 1;

                // 마스크가 true인 자리만 이동 대상으로 허용
                while (targetY > 0 && (blocks[x, targetY - 1] == null || (gridMask != null && !gridMask[x, targetY])))
                    targetY--;

                // 마스킹된 위치면 이동하지 않음
                if (gridMask != null && !gridMask[x, targetY])
                    continue;

                blocks[x, targetY] = blocks[x, y];
                blocks[x, y] = null;

                Block block = blocks[x, targetY].GetComponent<Block>();
                block.y = targetY;

                Vector3 targetPos = new Vector3(x * cellSize, targetY * cellSize, 0f)
                                  - new Vector3((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, 0f)
                                  + Vector3.up * (height * 0.1f);

                block.transform.DOMove(targetPos, 0.2f).SetLink(block.gameObject);
            }
        }
    }

    public void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            CollapseColumn(x);

            for (int y = 0; y < height; y++)
            {
                if (gridMask != null && !gridMask[x, y]) continue;

                if (blocks[x, y] == null)
                {
                    BlockType randType = normalTypes[Random.Range(0, normalTypes.Length)];

                    Vector3 spawnPos = new Vector3(x * cellSize, (y + 2) * cellSize, 0f)
                                     - new Vector3((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, 0f)
                                     + Vector3.up * (height * 0.1f);

                    GameObject blockObj = Instantiate(blockPrefab, spawnPos, Quaternion.identity, blocksParent);
                    blockObj.name = $"Block_{x}_{y}";

                    Block block = blockObj.GetComponent<Block>();
                    block.x = x;
                    block.y = y;
                    block.SetType(randType, spriteDict[randType]);

                    blocks[x, y] = blockObj;

                    Vector3 targetPos = new Vector3(x * cellSize, y * cellSize, 0f)
                                      - new Vector3((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, 0f)
                                      + Vector3.up * (height * 0.1f);

                    block.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutQuad).SetLink(block.gameObject);
                }
            }
        }
    }

    // 코루틴으로 자연스럽게 이동 (애니메이션)
    IEnumerator MoveToPosition(Transform obj, Vector3 target, float time)
    {
        if (obj == null) yield break;

        Vector3 start = obj.position;
        float t = 0f;

        while (t < 1f)
        {
            if (obj == null) yield break;
            t += Time.deltaTime / time;
            obj.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        // 마지막에 다시 null 확인
        if (obj != null)
            obj.position = target;
    }

    // 매칭 → 제거 → 채우기 → 반복
    void HandleMatches()
    {
        List<Block> matches = FindAllMatches();

        if (matches.Count == 0)
        {
            chainCount = 0; // 연쇄 종료 → 초기화
            isProcessing = false;
            return;
        }

        chainCount++; // 연쇄 카운트 증가

     
        // 특수 블록 생성 시도
        CreateSpecialBlock(matches,chainCount);

        // 점수 계산 (예: 10점 × 블록 수)
        ScoreManager.Instance.AddScore(matches.Count * 10);
        // 매치 성공 SFX
        if (matches.Count > 0)
            SoundManager.Instance.PlaySFX(SoundManager.Instance.matchSFX);

        foreach (Block block in matches)
        {
            blocks[block.x, block.y] = null;
            
            Destroy(block.gameObject);
        }

        DOVirtual.DelayedCall(0.25f, () =>
        {
            FillEmptySpaces();

            // 이 부분이 핵심
            DOVirtual.DelayedCall(0.35f, () =>
            {
                // 매칭이 또 생기면 연쇄 계속
                if (FindAllMatches().Count > 0)
                {
                    HandleMatches();
                }
                else
                {
                    // 연쇄 종료 시 입력 해제
                    Debug.Log("연쇄 끝 - 입력 가능");
                    isProcessing = false;
                }
            });
        });
    }

    // 해당 블록이 포함된 매칭이 있는지 확인
    bool IsBlockInMatch(Block block)
    {
        List<Block> horizontal = new List<Block> { block };
        List<Block> vertical = new List<Block> { block };

        // 좌우 검사
        int x = block.x;
        int y = block.y;
        BlockType type = block.blockType;

        // 왼쪽
        int i = x - 1;
        while (i >= 0 && GetBlock(i, y)?.blockType == type)
        {
            horizontal.Add(GetBlock(i, y));
            i--;
        }
        // 오른쪽
        i = x + 1;
        while (i < width && GetBlock(i, y)?.blockType == type)
        {
            horizontal.Add(GetBlock(i, y));
            i++;
        }

        // 아래쪽
        int j = y - 1;
        while (j >= 0 && GetBlock(x, j)?.blockType == type)
        {
            vertical.Add(GetBlock(x, j));
            j--;
        }
        // 위쪽
        j = y + 1;
        while (j < height && GetBlock(x, j)?.blockType == type)
        {
            vertical.Add(GetBlock(x, j));
            j++;
        }

        return horizontal.Count >= 3 || vertical.Count >= 3;
    }
    void SwapBack(Block a, Block b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        a.transform.DOMove(posB, 0.2f).SetLink(a.gameObject);
        b.transform.DOMove(posA, 0.2f).SetLink(b.gameObject);

        DOVirtual.DelayedCall(0.25f, () =>
        {
            blocks[a.x, a.y] = b.gameObject;
            blocks[b.x, b.y] = a.gameObject;

            int tempX = a.x;
            int tempY = a.y;
            a.x = b.x;
            a.y = b.y;
            b.x = tempX;
            b.y = tempY;
        });
    }

    void CreateSpecialBlock(List<Block> matched, int chainCount)
    {
        if (matched.Count < 4) return;

        Block specialBlock = matched[Random.Range(0, matched.Count)];

        // 💣 폭탄 조건
        if (matched.Count >= 5)
        {
            Block bombBlock = matched[Random.Range(0, matched.Count)];
            bombBlock.blockType = BlockType.Bomb;
            bombBlock.spriteRenderer.sprite = spriteDict[bombBlock.blockType];
            bombBlock.isSpecial = true;
            bombBlock.transform.localScale = Vector3.one;
            matched.Remove(bombBlock);
            blocks[bombBlock.x, bombBlock.y] = bombBlock.gameObject;

            bombBlock.spriteRenderer.DOFade(0.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(bombBlock.gameObject);
        }

        // ⚡ 번개 조건 (남은 matched 안에서)
        if (chainCount >= 3 && matched.Count > 0)
        {
            Block lightningBlock = matched[Random.Range(0, matched.Count)];
            lightningBlock.blockType = BlockType.Lightning;
            lightningBlock.spriteRenderer.sprite = spriteDict[lightningBlock.blockType];
            lightningBlock.isSpecial = true;
            lightningBlock.transform.localScale = Vector3.one;
            matched.Remove(lightningBlock);
            blocks[lightningBlock.x, lightningBlock.y] = lightningBlock.gameObject;

            lightningBlock.spriteRenderer.DOFade(0.5f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(lightningBlock.gameObject);
        }


        // 기본 Row/Col 특수 블록 생성
        specialBlock.isSpecial = true;
        bool horizontal = Random.Range(0, 2) == 0;
        specialBlock.isRowClear = horizontal;
        specialBlock.blockType = horizontal ? BlockType.RowClear : BlockType.ColClear;
        specialBlock.spriteRenderer.sprite = spriteDict[specialBlock.blockType];

        matched.Remove(specialBlock);
        blocks[specialBlock.x, specialBlock.y] = specialBlock.gameObject;

        specialBlock.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        specialBlock.spriteRenderer.DOFade(0.5f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(specialBlock.gameObject);
    }

    // 특수 블록(가로줄 또는 세로줄 제거) 발동 시 실행되는 코루틴
    // 중심 블록을 기준으로 양옆 블록들을 순차적으로 제거하는 연출 포함
    IEnumerator ActivateSpecialBlockSequential(Block block)
    {
        int x = block.x;
        int y = block.y;

        // 특수 블록 사운드
        SoundManager.Instance.PlaySFX(SoundManager.Instance.specialMatchSFX);
        List<Block> toRemove = new List<Block>();
        // 블록 타입이 폭탄일때
        if (block.blockType == BlockType.Bomb)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = block.x + dx;
                    int ny = block.y + dy;

                    if (IsInsideGrid(nx, ny))
                    {
                        Block b = GetBlock(nx, ny);
                        if (b != null) toRemove.Add(b);
                    }
                }
            }

            // 자신도 제거 대상에 포함
            if (!toRemove.Contains(block))
                toRemove.Add(block);

            foreach (Block b in toRemove)
            {
                blocks[b.x, b.y] = null;
                Destroy(b.gameObject);
                ScoreManager.Instance.AddScore(10);
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.2f);

            FillEmptySpaces();

            DOVirtual.DelayedCall(0.35f, () =>
            {
                if (FindAllMatches().Count > 0)
                    HandleMatches();
                else
                    isProcessing = false;
            });

            yield break;
        }
        // 블록 타입이 라이트닝일때
        if (block.blockType == BlockType.Lightning)
        {
            // 세로 줄 제거 (x 고정, y 전체)
            for (int j = 0; j < height; j++)
            {
                Block b = GetBlock(block.x, j);
                if (b != null) toRemove.Add(b);
            }

            // 가로 줄 제거 (y 고정, x 전체)
            for (int i = 0; i < width; i++)
            {
                Block b = GetBlock(i, block.y);
                if (b != null && !toRemove.Contains(b)) // 중복 방지
                    toRemove.Add(b);
            }

            // 자기 자신도 포함
            if (!toRemove.Contains(block))
                toRemove.Add(block);

            foreach (Block b in toRemove)
            {
                blocks[b.x, b.y] = null;
                Destroy(b.gameObject);
                ScoreManager.Instance.AddScore(10);
                yield return new WaitForSeconds(0.03f);
            }

            yield return new WaitForSeconds(0.2f);

            FillEmptySpaces();

            DOVirtual.DelayedCall(0.35f, () =>
            {
                if (FindAllMatches().Count > 0)
                    HandleMatches();
                else
                    isProcessing = false;
            });

            yield break;
        }

        if (block.isRowClear)
        {
            for (int i = 0; i < width; i++)
            {
                Block b = GetBlock(i, y);
                if (b != null) toRemove.Add(b);
            }
            // 중심 블록 기준으로 가까운 순으로 정렬 (좌↔우 순차 제거를 위함)
            toRemove.Sort((a, b) => Mathf.Abs(a.x - x).CompareTo(Mathf.Abs(b.x - x)));
        }
        else
        {
            // 세로줄 수집
            for (int j = 0; j < height; j++)
            {
                Block b = GetBlock(x, j);
                if (b != null) toRemove.Add(b);
            }
            // 중심 블록 기준으로 가까운 순으로 정렬 (상↕하 순차 제거)
            toRemove.Sort((a, b) => Mathf.Abs(a.y - y).CompareTo(Mathf.Abs(b.y - y)));
        }

        // 특수 블록 본인도 명확하게 포함
        if (!toRemove.Contains(block))
            toRemove.Add(block);

        foreach (Block b in toRemove)
        {
            blocks[b.x, b.y] = null;
            Destroy(b.gameObject);
            ScoreManager.Instance.AddScore(10);

            yield return new WaitForSeconds(0.05f); // 간격 조절 가능
        }

        yield return new WaitForSeconds(0.2f);

        // 빈칸 채우기 후 연쇄 매칭 여부 확인
        FillEmptySpaces();
        DOVirtual.DelayedCall(0.35f, () =>
        {
            if (FindAllMatches().Count > 0)
            {
                // 연쇄 발생 → 재귀 호출
                HandleMatches();
            }
            else
            {
                // 연쇄 종료 → 입력 가능 상태로 복귀
                isProcessing = false;
            }
        });
    }

    void SwapAndActivateSpecialBlock(Block special, Block other)
    {
        Vector3 specialTarget = other.transform.position;
        Vector3 otherTarget = special.transform.position;

        // 좌표 스왑
        int tempX = special.x;
        int tempY = special.y;

        special.x = other.x;
        special.y = other.y;
        other.x = tempX;
        other.y = tempY;

        // 배열 갱신
        blocks[special.x, special.y] = special.gameObject;
        blocks[other.x, other.y] = other.gameObject;

        // DOTween 이동
        special.transform.DOMove(specialTarget, 0.2f).SetLink(special.gameObject);
        other.transform.DOMove(otherTarget, 0.2f).SetLink(other.gameObject);

        // DOTween 끝나고 발동
        DOVirtual.DelayedCall(0.25f, () =>
        {
            // ✅ 실제 파괴 전 배열에서 제거
            blocks[special.x, special.y] = null;

            StartCoroutine(ActivateSpecialBlockSequential(special));
        });
    }
    public bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
