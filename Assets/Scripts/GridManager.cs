using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Level")]
    public LevelData[] levels;
    public int startLevelIndex;

    [Header("Test (Inspector)")]
    [Tooltip("켜면 항상 testMaskResourcePath 마스크만 사용 (levels·랜덤 무시)")]
    public bool testModeLockMask = false;
    [Tooltip("Resources 경로, 확장자 제외. 기본: Level01")]
    public string testMaskResourcePath = "Masks/Mask_Level01";

    [Header("Prefab & Parent")]
    public GameObject blockPrefab;
    public Transform blocksParent;
    public BlockPool blockPool;

    [Header("블록 타입과 스프라이트 매핑")]
    public List<BlockSprite> blockSprites;

    GameObject[,] blocks;
    bool[,] gridMask;
    Dictionary<BlockType, Sprite> spriteDict;

    [Header("특수 블록 생성 연출")]
    [SerializeField] float refillFallDuration = 0.3f;
    [SerializeField] float specialNormalHoldTime = 0.12f;
    [SerializeField] float specialNormalVanishTime = 0.18f;
    [SerializeField] float specialTransformTime = 0.22f;

    Block selectedBlock;
    bool isProcessing;
    int chainCount;
    Block lastMovedBlock;

    MatchFinder matchFinder;
    SpecialBlockHandler specialHandler;
    BoardResolver boardResolver;

    readonly BlockType[] normalTypes =
    {
        BlockType.Apple,
        BlockType.Banana,
        BlockType.Grape,
        BlockType.Orange
    };

    public int ChainCount => chainCount;
    public Block LastMovedBlock => lastMovedBlock;
    public float RefillFallDuration => refillFallDuration;

    void Awake()
    {
        spriteDict = new Dictionary<BlockType, Sprite>();
        foreach (var entry in blockSprites)
            spriteDict[entry.type] = entry.sprite;

        specialHandler = new SpecialBlockHandler(spriteDict);
        boardResolver = GetComponent<BoardResolver>();
        if (boardResolver == null)
            boardResolver = gameObject.AddComponent<BoardResolver>();
    }

    void Start()
    {
        if (testModeLockMask)
            LoadTestMask();
        else if (levels != null && levels.Length > 0)
            LoadLevel(levels[Mathf.Clamp(startLevelIndex, 0, levels.Length - 1)]);
        else
            LoadRandomMask();

        matchFinder = new MatchFinder(width, height, GetBlock);
        boardResolver.Initialize(this, matchFinder, specialHandler);
        GenerateGrid();

        ScoreManager.Instance?.RefreshUI();
        GameTimer.Instance?.StartTimer();
    }

    public void LoadLevel(LevelData level)
    {
        if (level == null) return;

        if (!LoadMaskFromResources(level.maskResourcePath))
            return;

        GameManager.Instance.goalScore = level.goalScore;
        if (GameTimer.Instance != null)
            GameTimer.Instance.SetTotalTime(level.timeLimit);

        Debug.Log($"[Level] {level.levelName} — {level.maskResourcePath}");
    }

    void LoadTestMask()
    {
        if (!LoadMaskFromResources(testMaskResourcePath))
            return;

        Debug.Log($"[Test] 고정 마스크: {testMaskResourcePath} (goal/time은 GameManager·GameTimer Inspector 값)");
    }

    void LoadRandomMask()
    {
        string[] maskNames = {
            "Masks/Mask_Level01",
            "Masks/Mask_Level02",
            "Masks/Mask_Level03",
        };

        string selectedName = maskNames[Random.Range(0, maskNames.Length)];
        if (!LoadMaskFromResources(selectedName))
            return;

        Debug.Log($"[Mask] 랜덤 선택: {selectedName}");
    }

    bool LoadMaskFromResources(string resourcePath)
    {
        TextAsset maskCsv = Resources.Load<TextAsset>(resourcePath);
        if (maskCsv == null)
        {
            Debug.LogError($"마스크 파일 없음: {resourcePath}");
            return false;
        }

        gridMask = MaskLoader.LoadMaskFromCSV(maskCsv);
        width = gridMask.GetLength(0);
        height = gridMask.GetLength(1);
        return true;
    }

    void GenerateGrid()
    {
        blocks = new GameObject[width, height];
        Vector2 offset = new Vector2((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f);
        float yOffset = height * 0.1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridMask != null && !gridMask[x, y]) continue;

                Vector3 spawnPos = new Vector3(x * cellSize, y * cellSize, 0f) - (Vector3)offset + Vector3.up * yOffset;
                SpawnBlockAt(x, y, spawnPos, GetNonMatchingType(x, y));
            }
        }
    }

    Vector3 CellToWorld(int x, int y)
    {
        Vector3 gridOffset = new Vector3((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, 0f);
        float yOffset = height * 0.1f;
        return new Vector3(x * cellSize, y * cellSize, 0f) - gridOffset + Vector3.up * yOffset;
    }

    public void RegisterBlock(Block block)
    {
        if (block == null) return;
        blocks[block.x, block.y] = block.gameObject;
    }

    /// <summary>
    /// 일반 리필이 끝난 뒤, 특수 생성 좌표의 일반 블록만 제거 연출 후 같은 칸에서 특수 블록으로 전환
    /// </summary>
    public IEnumerator PlaySpecialSpawnTransition(SpecialSpawnPlan plan)
    {
        int x = plan.spawnX;
        int y = plan.spawnY;

        if (gridMask != null && !gridMask[x, y])
            yield break;

        Block normal = GetBlock(x, y);
        if (normal == null)
        {
            Debug.LogWarning($"[GridManager] 특수 전환 칸 ({x},{y})에 리필 블록 없음");
            yield break;
        }

        SnapBlockToCell(normal);
        yield return new WaitForSeconds(specialNormalHoldTime);

        PlayBlockVanishAnimation(normal);
        yield return new WaitForSeconds(specialNormalVanishTime);

        TransformBlockToSpecial(normal, plan.blockType, plan.isRowClear);
        yield return new WaitForSeconds(specialTransformTime);
    }

    void TransformBlockToSpecial(Block block, BlockType type, bool isRowClear)
    {
        block.transform.DOKill();
        if (block.spriteRenderer != null)
            block.spriteRenderer.DOKill();

        specialHandler.ApplySpecialToBlock(block, type, isRowClear);

        float targetScale = (type == BlockType.Bomb || type == BlockType.Lightning)
            ? 1f
            : 0.5f;

        block.transform.localScale = Vector3.zero;
        block.transform.DOScale(targetScale, specialTransformTime)
            .SetEase(Ease.OutBack)
            .SetLink(block.gameObject);
    }

    Block SpawnBlockWithRefillFall(int x, int y, BlockType type)
    {
        Vector3 spawnPos = CellToWorld(x, y + 2);
        GameObject blockObj = SpawnBlockAt(x, y, spawnPos, type);
        if (blockObj == null) return null;

        blockObj.GetComponent<Block>().transform
            .DOMove(CellToWorld(x, y), refillFallDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(blockObj);

        return blockObj.GetComponent<Block>();
    }

    void PlayBlockVanishAnimation(Block block)
    {
        if (block == null) return;

        block.transform.DOKill();
        block.transform.DOScale(0f, specialNormalVanishTime)
            .SetEase(Ease.InBack)
            .SetLink(block.gameObject);

        if (block.spriteRenderer != null)
        {
            block.spriteRenderer.DOKill();
            block.spriteRenderer.DOFade(0f, specialNormalVanishTime)
                .SetLink(block.gameObject);
        }
    }

    /// <summary>그리드에는 없지만 같은 칸에 남은 고아 오브젝트 제거</summary>
    void PurgeStrayBlockAtCell(int x, int y)
    {
        if (blocksParent == null) return;

        Vector3 world = CellToWorld(x, y);
        float radius = cellSize * 0.35f;

        for (int i = blocksParent.childCount - 1; i >= 0; i--)
        {
            Transform child = blocksParent.GetChild(i);
            if (Vector3.Distance(child.position, world) > radius) continue;

            Block b = child.GetComponent<Block>();
            if (b == null) continue;
            if (blocks[x, y] != null && blocks[x, y] == child.gameObject) continue;

            child.transform.DOKill();
            if (blockPool != null)
                blockPool.Release(child.gameObject);
            else
                Destroy(child.gameObject);
        }
    }

    void SnapBlockToCell(Block block)
    {
        if (block == null) return;
        block.transform.DOKill();
        block.transform.position = CellToWorld(block.x, block.y);
        blocks[block.x, block.y] = block.gameObject;
    }

    GameObject SpawnBlockAt(int x, int y, Vector3 position, BlockType type)
    {
        if (blocks[x, y] != null)
        {
            Debug.LogWarning($"[GridManager] ({x},{y}) 칸이 이미 점유됨 — 스폰 스킵");
            return blocks[x, y];
        }

        GameObject blockObj = blockPool != null
            ? blockPool.Get(position, blocksParent)
            : Instantiate(blockPrefab, position, Quaternion.identity, blocksParent);

        blockObj.name = $"Block_{x}_{y}";
        Block block = blockObj.GetComponent<Block>();
        block.x = x;
        block.y = y;
        block.isSpecial = false;
        block.isRowClear = false;
        block.SetType(type, spriteDict[type]);
        blocks[x, y] = blockObj;
        return blockObj;
    }

    BlockType GetNonMatchingType(int x, int y)
    {
        List<BlockType> possibleTypes = new List<BlockType>(normalTypes);

        if (x >= 2)
        {
            Block left1 = GetBlock(x - 1, y);
            Block left2 = GetBlock(x - 2, y);
            if (left1 != null && left2 != null && left1.blockType == left2.blockType)
                possibleTypes.Remove(left1.blockType);
        }

        if (y >= 2)
        {
            Block down1 = GetBlock(x, y - 1);
            Block down2 = GetBlock(x, y - 2);
            if (down1 != null && down2 != null && down1.blockType == down2.blockType)
                possibleTypes.Remove(down1.blockType);
        }

        return possibleTypes[Random.Range(0, possibleTypes.Count)];
    }

    public void SelectBlock(Block block)
    {
        if (GameManager.Instance.isGameOver || isProcessing) return;

        if (selectedBlock == null)
        {
            selectedBlock = block;
            HighlightBlock(block, selected: true);
        }
        else if (AreAdjacent(selectedBlock, block))
        {
            isProcessing = true;
            HighlightBlock(selectedBlock, selected: false);

            if (selectedBlock.isSpecial)
            {
                lastMovedBlock = selectedBlock;
                SwapAndActivateSpecialBlock(selectedBlock, block);
            }
            else if (block.isSpecial)
            {
                lastMovedBlock = block;
                SwapAndActivateSpecialBlock(block, selectedBlock);
            }
            else
            {
                lastMovedBlock = block;
                SwapBlocks(selectedBlock, block);
            }

            selectedBlock = null;
        }
        else
        {
            HighlightBlock(selectedBlock, selected: false);
            selectedBlock = block;
            HighlightBlock(block, selected: true);
        }
    }

    void HighlightBlock(Block block, bool selected)
    {
        bool big = block.blockType == BlockType.Bomb || block.blockType == BlockType.Lightning;
        float scale = selected ? (big ? 1.25f : 0.65f) : (big ? 1f : 0.5f);
        block.transform.DOScale(scale, 0.1f).SetEase(Ease.OutQuad).SetLink(block.gameObject);
    }

    static bool AreAdjacent(Block a, Block b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }

    void SwapBlocks(Block a, Block b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        a.transform.DOMove(posB, 0.2f).SetLink(a.gameObject);
        b.transform.DOMove(posA, 0.2f).SetLink(b.gameObject);

        SwapGridRefs(a, b);

        DOVirtual.DelayedCall(0.25f, () =>
        {
            if (!matchFinder.IsBlockInMatch(a) && !matchFinder.IsBlockInMatch(b))
                SwapBack(a, b);
            else
                boardResolver.StartCascade();
        });
    }

    void SwapGridRefs(Block a, Block b)
    {
        blocks[a.x, a.y] = b.gameObject;
        blocks[b.x, b.y] = a.gameObject;

        int tx = a.x, ty = a.y;
        a.x = b.x; a.y = b.y;
        b.x = tx; b.y = ty;
    }

    void SwapBack(Block a, Block b)
    {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        a.transform.DOMove(posB, 0.2f).SetLink(a.gameObject);
        b.transform.DOMove(posA, 0.2f).SetLink(b.gameObject);

        DOVirtual.DelayedCall(0.25f, () =>
        {
            SwapGridRefs(a, b);
            isProcessing = false;
        });
    }

    public Block GetBlock(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return blocks[x, y]?.GetComponent<Block>();
    }

    public void ClearBlockAt(int x, int y)
    {
        if (blocks[x, y] == null) return;
        GameObject obj = blocks[x, y];
        blocks[x, y] = null;

        if (blockPool != null)
            blockPool.Release(obj);
        else
            Destroy(obj);
    }

    public void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
            CollapseColumn(x);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridMask != null && !gridMask[x, y]) continue;
                if (blocks[x, y] != null) continue;

                BlockType randType = normalTypes[Random.Range(0, normalTypes.Length)];
                SpawnBlockWithRefillFall(x, y, randType);
            }
        }
    }

    /// <summary>열 전체를 아래(y=0)로 압축. 기존 로직은 점유 칸을 덮어써 고아 블록이 생길 수 있었음.</summary>
    void CollapseColumn(int x)
    {
        var columnObjects = new List<GameObject>();

        for (int y = 0; y < height; y++)
        {
            if (gridMask != null && !gridMask[x, y]) continue;
            if (blocks[x, y] == null) continue;

            columnObjects.Add(blocks[x, y]);
            blocks[x, y] = null;
        }

        int index = 0;
        for (int y = 0; y < height; y++)
        {
            if (gridMask != null && !gridMask[x, y]) continue;
            if (index >= columnObjects.Count) break;

            GameObject obj = columnObjects[index++];
            blocks[x, y] = obj;

            Block block = obj.GetComponent<Block>();
            if (block.x != x || block.y != y)
            {
                block.x = x;
                block.y = y;
                block.transform.DOMove(CellToWorld(x, y), 0.2f).SetLink(obj);
            }
        }
    }

    public void IncrementChainCount() => chainCount++;
    public void ResetChainCount() => chainCount = 0;
    public void SetProcessing(bool value) => isProcessing = value;

    public bool IsInsideGrid(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    public void CheckDeadlockAndShuffle()
    {
        if (HasPossibleMove()) return;
        Debug.Log("[GridManager] 가능한 수 없음 — 셔플");
        StartCoroutine(ShuffleBoardRoutine());
    }

    bool HasPossibleMove()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridMask != null && !gridMask[x, y]) continue;
                Block a = GetBlock(x, y);
                if (a == null || a.isSpecial) continue;

                if (TrySwapWouldMatch(a, GetBlock(x + 1, y))) return true;
                if (TrySwapWouldMatch(a, GetBlock(x, y + 1))) return true;
            }
        }
        return false;
    }

    bool TrySwapWouldMatch(Block a, Block b)
    {
        if (a == null || b == null) return false;
        SwapTypesInPlace(a, b);
        bool match = matchFinder.IsBlockInMatch(a) || matchFinder.IsBlockInMatch(b);
        SwapTypesInPlace(a, b);
        return match;
    }

    static void SwapTypesInPlace(Block a, Block b)
    {
        BlockType t = a.blockType;
        a.blockType = b.blockType;
        b.blockType = t;
    }

    IEnumerator ShuffleBoardRoutine()
    {
        isProcessing = true;
        var cells = new List<Block>();
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            if (gridMask != null && !gridMask[x, y]) continue;
            Block b = GetBlock(x, y);
            if (b != null && !b.isSpecial) cells.Add(b);
        }

        for (int attempt = 0; attempt < 20; attempt++)
        {
            for (int i = cells.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                SwapTypesInPlace(cells[i], cells[j]);
                var tmpSprite = cells[i].spriteRenderer.sprite;
                cells[i].spriteRenderer.sprite = cells[j].spriteRenderer.sprite;
                cells[j].spriteRenderer.sprite = tmpSprite;
            }

            if (matchFinder.FindAllMatches().Count == 0 && HasPossibleMove())
            {
                foreach (Block b in cells)
                    b.SetType(b.blockType, spriteDict[b.blockType]);
                break;
            }
        }

        yield return new WaitForSeconds(0.2f);
        isProcessing = false;
    }

    void SwapAndActivateSpecialBlock(Block special, Block other)
    {
        Vector3 specialTarget = other.transform.position;
        Vector3 otherTarget = special.transform.position;

        int tx = special.x, ty = special.y;
        special.x = other.x; special.y = other.y;
        other.x = tx; other.y = ty;

        blocks[special.x, special.y] = special.gameObject;
        blocks[other.x, other.y] = other.gameObject;

        special.transform.DOMove(specialTarget, 0.2f).SetLink(special.gameObject);
        other.transform.DOMove(otherTarget, 0.2f).SetLink(other.gameObject);

        DOVirtual.DelayedCall(0.25f, () =>
        {
            SnapBlockToCell(special);
            SnapBlockToCell(other);
            StartCoroutine(ActivateSpecialBlockSequential(special));
        });
    }

    IEnumerator ActivateSpecialBlockSequential(Block block)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.specialMatchSFX);
        var toRemove = new List<Block>();

        if (block.blockType == BlockType.Bomb)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (!IsInsideGrid(block.x + dx, block.y + dy)) continue;
                Block b = GetBlock(block.x + dx, block.y + dy);
                if (b != null) toRemove.Add(b);
            }
        }
        else if (block.blockType == BlockType.Lightning)
        {
            for (int j = 0; j < height; j++)
            {
                Block b = GetBlock(block.x, j);
                if (b != null) toRemove.Add(b);
            }
            for (int i = 0; i < width; i++)
            {
                Block b = GetBlock(i, block.y);
                if (b != null && !toRemove.Contains(b)) toRemove.Add(b);
            }
        }
        else if (block.isRowClear)
        {
            for (int i = 0; i < width; i++)
            {
                Block b = GetBlock(i, block.y);
                if (b != null) toRemove.Add(b);
            }
            toRemove.Sort((a, b) => Mathf.Abs(a.x - block.x).CompareTo(Mathf.Abs(b.x - block.x)));
        }
        else
        {
            for (int j = 0; j < height; j++)
            {
                Block b = GetBlock(block.x, j);
                if (b != null) toRemove.Add(b);
            }
            toRemove.Sort((a, b) => Mathf.Abs(a.y - block.y).CompareTo(Mathf.Abs(b.y - block.y)));
        }

        foreach (Block b in toRemove)
        {
            ClearBlockAt(b.x, b.y);
            ScoreManager.Instance.AddScore(10);
            yield return new WaitForSeconds(0.05f);
        }

        FillEmptySpaces();
        boardResolver.ContinueAfterSpecial();
    }
}
