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
                BlockType randType = (BlockType)Random.Range(0, System.Enum.GetValues(typeof(BlockType)).Length);

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
        Vector3 tempPos = a.transform.position;
        a.transform.position = b.transform.position;
        b.transform.position = tempPos;

        blocks[a.x, a.y] = b.gameObject;
        blocks[b.x, b.y] = a.gameObject;

        int tempX = a.x;
        int tempY = a.y;
        a.x = b.x;
        a.y = b.y;
        b.x = tempX;
        b.y = tempY;

        // 매칭 검사 → 제거 → 연쇄 처리
        StartCoroutine(HandleMatches());
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

                // 아래 빈 칸 찾기
                while (targetY > 0 && blocks[x, targetY - 1] == null)
                    targetY--;

                // 블록 이동
                blocks[x, targetY] = blocks[x, y];
                blocks[x, y] = null;

                Block block = blocks[x, targetY].GetComponent<Block>();
                block.y = targetY;
                block.transform.position = new Vector3(x * cellSize, targetY * cellSize, 0f)
                                         - new Vector3((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, 0f)
                                         + Vector3.up * (height * 0.1f);
            }
        }
    }

    void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            CollapseColumn(x);

            // 빈 칸 생성
            for (int y = 0; y < height; y++)
            {
                if (blocks[x, y] == null)
                {
                    BlockType randType = (BlockType)Random.Range(0, System.Enum.GetValues(typeof(BlockType)).Length);

                    GameObject blockObj = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity, blocksParent);
                    blockObj.name = $"Block_{x}_{y}";

                    Block block = blockObj.GetComponent<Block>();
                    block.x = x;
                    block.y = y;
                    block.SetType(randType, spriteDict[randType]);

                    blocks[x, y] = blockObj;

                    // 위치 설정
                    block.transform.position = new Vector3(x * cellSize, y * cellSize, 0f)
                                             - new Vector3((width - 1) * cellSize / 2f, (height - 1) * cellSize / 2f, 0f)
                                             + Vector3.up * (height * 0.1f);
                }
            }
        }
    }

    // 매칭 → 제거 → 채우기 → 반복
    IEnumerator HandleMatches()
    {
        yield return new WaitForSeconds(0.2f); // 약간의 연출 대기

        List<Block> matches = FindAllMatches();

        while (matches.Count > 0)
        {
            // 제거
            foreach (Block block in matches)
            {
                blocks[block.x, block.y] = null;
                Destroy(block.gameObject);
            }

            yield return new WaitForSeconds(0.2f); // 제거 후 딜레이

            // 채우기
            FillEmptySpaces();

            yield return new WaitForSeconds(0.2f); // 채우기 후 딜레이

            // 다시 검사
            matches = FindAllMatches();
        }

        // 마지막 상태로 돌아옴
        Debug.Log("All matches handled.");
    }
}
