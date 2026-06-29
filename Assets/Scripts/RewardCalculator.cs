using UnityEngine;

public struct RunReward
{
    public int gold;
    public int gems;

    public bool HasReward => gold > 0 || gems > 0;

    public string ToDisplayLine()
    {
        if (!HasReward) return string.Empty;
        if (gems > 0)
            return $"+{gold:N0} Gold   +{gems} Gems";
        return $"+{gold:N0} Gold";
    }
}

/// <summary>클리어/실패 결과에 따른 Gold·Gem 보상 계산</summary>
public static class RewardCalculator
{
    public static RunReward Calculate(bool cleared, int score, int goalScore, bool isAllClear, int roundNumber)
    {
        if (cleared)
            return CalculateClear(score, goalScore, isAllClear, roundNumber);
        return CalculateFail(score, goalScore);
    }

    static RunReward CalculateClear(int score, int goalScore, bool isAllClear, int roundNumber)
    {
        int gold = 40 + score / 80 + roundNumber * 10;
        int gems = 1;

        if (goalScore > 0 && score >= goalScore * 2)
            gems += 1;

        if (isAllClear)
        {
            gold += 150;
            gems += 4;
        }

        return new RunReward { gold = gold, gems = gems };
    }

    static RunReward CalculateFail(int score, int goalScore)
    {
        int progressPercent = goalScore > 0 ? Mathf.Clamp(score * 100 / goalScore, 0, 100) : 0;
        int gold = 10 + progressPercent / 4;
        return new RunReward { gold = gold, gems = 0 };
    }
}
