using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public struct SpecialSpawnPlan
{
    public bool hasSpecial;
    public int spawnX;
    public int spawnY;
    public BlockType blockType;
    public bool isRowClear;
}

/// <summary>
/// 특수 블록 규칙 — 우선순위: 번개(3연쇄+) &gt; 폭탄(5매치+) &gt; 줄삭제(4매치+)
/// 매치 블록은 전부 제거 후, 지정 칸에 특수 블록을 새로 생성
/// </summary>
public class SpecialBlockHandler
{
    readonly Dictionary<BlockType, Sprite> spriteDict;

    public SpecialBlockHandler(Dictionary<BlockType, Sprite> spriteDict)
    {
        this.spriteDict = spriteDict;
    }

    public SpecialSpawnPlan PlanSpecialSpawn(
        List<Block> matched,
        int chainCount,
        Block lastMovedBlock,
        MatchFinder matchFinder)
    {
        if (matched.Count < 4 && chainCount < 3)
            return default;

        Block anchor = PickSpawnBlock(matched, lastMovedBlock);
        BlockType fruitType = anchor.blockType;

        SpecialSpawnPlan plan = new SpecialSpawnPlan
        {
            hasSpecial = true,
            spawnX = anchor.x,
            spawnY = anchor.y,
        };

        if (chainCount >= 3)
        {
            plan.blockType = BlockType.Lightning;
            plan.isRowClear = false;
        }
        else if (matched.Count >= 5)
        {
            plan.blockType = BlockType.Bomb;
            plan.isRowClear = false;
        }
        else
        {
            plan.isRowClear = matchFinder.CountLine(anchor.x, anchor.y, fruitType, horizontal: true) >= 4;
            plan.blockType = plan.isRowClear ? BlockType.RowClear : BlockType.ColClear;
        }

        return plan;
    }

    static Block PickSpawnBlock(List<Block> matched, Block lastMovedBlock)
    {
        if (lastMovedBlock != null && matched.Contains(lastMovedBlock))
            return lastMovedBlock;
        return matched[0];
    }

    public void ApplySpecialToBlock(Block block, BlockType finalType, bool isRowClear)
    {
        block.isSpecial = true;
        block.isRowClear = isRowClear;
        block.blockType = finalType;
        block.spriteRenderer.sprite = spriteDict[finalType];

        block.transform.localScale = (finalType == BlockType.Bomb || finalType == BlockType.Lightning)
            ? Vector3.one
            : new Vector3(0.5f, 0.5f, 0.5f);

        var c = block.spriteRenderer.color;
        c.a = 1f;
        block.spriteRenderer.color = c;

        block.spriteRenderer.DOKill();
        block.spriteRenderer.DOFade(0.5f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(block.gameObject);
    }
}
