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

    [Header("Prefab & Parent")]
    public GameObject blockPrefab; // 하나의 블록 프리팹만 사용
    public Transform blocksParent;

    [Header("블록 타입과 스프라이트 매핑")]
    public List<BlockSprite> blockSprites;

    private Dictionary<BlockType, Sprite> spriteDict;
    private GameObject[,] blocks;
    private Block selectedBlock = null;

    void Awake()
    {
        
    }

    void Start()
    {
        // 타입별 스프라이트 딕셔너리 초기화
        spriteDict = new Dictionary<BlockType, Sprite>();
        foreach (var b in blockSprites)
        {
            spriteDict[b.type] = b.sprite;
        }

        blocks = new GameObject[width, height];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Vector2 offset = new Vector2((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f);
        float yOffset = height * 0.1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 spawnPos = new Vector3(x * cellSize, y * cellSize, 0f) - (Vector3)offset + Vector3.up * yOffset;

                // 랜덤 타입 선택
                BlockType randType = GetNonMatchingType(x, y);

                // 프리팹 생성
                GameObject blockObj = Instantiate(blockPrefab, spawnPos, Quaternion.identity, blocksParent);
                blockObj.name = $"Block_{x}_{y}";

                // 정보 전달
                Block block = blockObj.GetComponent<Block>();
                block.x = x;
                block.y = y;
                block.SetType(randType, spriteDict[randType]);

                blocks[x, y] = blockObj;
            }
        }
    }
    // 특정 위치에 블록을 배치할 때 3개 이상 연속되지 않도록 안전한 타입을 고름
    BlockType GetNonMatchingType(int x, int y)
    {
        List<BlockType> possibleTypes = new List<BlockType>((BlockType[])System.Enum.GetValues(typeof(BlockType)));

        // 왼쪽 2칸 검사
        if (x >= 2)
        {
            Block left1 = GetBlock(x - 1, y);
            Block left2 = GetBlock(x - 2, y);

            if (left1 != null && left2 != null && left1.blockType == left2.blockType)
            {
                possibleTypes.Remove(left1.blockType); // 같은 타입 제거
            }
        }

        // 아래쪽 2칸 검사
        if (y >= 2)
        {
            Block down1 = GetBlock(x, y - 1);
            Block down2 = GetBlock(x, y - 2);

            if (down1 != null && down2 != null && down1.blockType == down2.blockType)
            {
                possibleTypes.Remove(down1.blockType); // 같은 타입 제거
            }
        }

        // 남아있는 타입 중에서 랜덤으로 선택
        return possibleTypes[Random.Range(0, possibleTypes.Count)];
    }
    // 블록 클릭 처리
    public void SelectBlock(Block block)
    {
        if (selectedBlock == null)
        {
            selectedBlock = block;
        }
        else
        {
            if (AreAdjacent(selectedBlock, block))
            {
                SwapBlocks(selectedBlock, block);
            }

            selectedBlock = null;
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
                while (targetY > 0 && blocks[x, targetY - 1] == null)
                    targetY--;

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

    void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            CollapseColumn(x);

            for (int y = 0; y < height; y++)
            {
                if (blocks[x, y] == null)
                {
                    BlockType randType = (BlockType)Random.Range(0, System.Enum.GetValues(typeof(BlockType)).Length);

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

        if (matches.Count == 0) return;

        foreach (Block block in matches)
        {
            blocks[block.x, block.y] = null;
            Destroy(block.gameObject);
        }

        DOVirtual.DelayedCall(0.25f, () =>
        {
            FillEmptySpaces();

            // 다시 연쇄 검사
            DOVirtual.DelayedCall(0.35f, () =>
            {
                HandleMatches();
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
}
