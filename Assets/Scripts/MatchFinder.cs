using System;
using System.Collections.Generic;

/// <summary>보드 매치 탐색 — 연속 3개 이상(run)만 인정</summary>
public class MatchFinder
{
    readonly int width;
    readonly int height;
    readonly Func<int, int, Block> getBlock;

    public MatchFinder(int width, int height, Func<int, int, Block> getBlock)
    {
        this.width = width;
        this.height = height;
        this.getBlock = getBlock;
    }

    public List<Block> FindAllMatches()
    {
        var matched = new HashSet<Block>();

        for (int y = 0; y < height; y++)
            ScanLine(horizontal: true, y, matched);

        for (int x = 0; x < width; x++)
            ScanLine(horizontal: false, x, matched);

        return new List<Block>(matched);
    }

    void ScanLine(bool horizontal, int fixedCoord, HashSet<Block> matched)
    {
        int length = horizontal ? width : height;

        for (int i = 0; i < length;)
        {
            Block start = horizontal ? getBlock(i, fixedCoord) : getBlock(fixedCoord, i);
            if (start == null || !IsNormalMatchType(start.blockType))
            {
                i++;
                continue;
            }

            int runStart = i;
            BlockType type = start.blockType;
            i++;

            while (i < length)
            {
                Block next = horizontal ? getBlock(i, fixedCoord) : getBlock(fixedCoord, i);
                if (next == null || next.blockType != type)
                    break;
                i++;
            }

            int runLength = i - runStart;
            if (runLength < 3)
                continue;

            for (int j = runStart; j < i; j++)
            {
                Block b = horizontal ? getBlock(j, fixedCoord) : getBlock(fixedCoord, j);
                if (b != null)
                    matched.Add(b);
            }
        }
    }

    public bool IsBlockInMatch(Block block)
    {
        if (block == null || !IsNormalMatchType(block.blockType))
            return false;

        int horizontal = CountLine(block.x, block.y, block.blockType, horizontal: true);
        int vertical = CountLine(block.x, block.y, block.blockType, horizontal: false);
        return horizontal >= 3 || vertical >= 3;
    }

    public int CountLine(int startX, int startY, BlockType type, bool horizontal)
    {
        if (!IsNormalMatchType(type))
            return 0;

        int count = 1;

        if (horizontal)
        {
            for (int x = startX - 1; x >= 0; x--)
            {
                Block b = getBlock(x, startY);
                if (b == null || b.blockType != type) break;
                count++;
            }
            for (int x = startX + 1; x < width; x++)
            {
                Block b = getBlock(x, startY);
                if (b == null || b.blockType != type) break;
                count++;
            }
        }
        else
        {
            for (int y = startY - 1; y >= 0; y--)
            {
                Block b = getBlock(startX, y);
                if (b == null || b.blockType != type) break;
                count++;
            }
            for (int y = startY + 1; y < height; y++)
            {
                Block b = getBlock(startX, y);
                if (b == null || b.blockType != type) break;
                count++;
            }
        }

        return count;
    }

    public static bool IsNormalMatchType(BlockType type)
    {
        return type == BlockType.Apple || type == BlockType.Banana
            || type == BlockType.Orange || type == BlockType.Grape;
    }
}
