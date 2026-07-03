public static class AdRewardService
{
    public static bool TryGrantReward(AdRewardOfferData offer, out string message)
    {
        if (GameDataManager.Instance == null)
        {
            message = "데이터를 불러올 수 없습니다.";
            return false;
        }

        var data = GameDataManager.Instance;

        if (offer.rewardGold > 0)
            data.AddGold(offer.rewardGold);

        if (offer.rewardGems > 0)
            data.AddGems(offer.rewardGems);

        message = $"보상 지급 완료!\n{offer.RewardLabel}";
        return true;
    }

    public static string FormatResultMessage(AdPlaybackResult result, string detail = null)
    {
        switch (result)
        {
            case AdPlaybackResult.Success:
                return string.IsNullOrEmpty(detail) ? "광고 시청 완료!" : detail;
            case AdPlaybackResult.Failure:
                return string.IsNullOrEmpty(detail)
                    ? "광고 재생 실패\n잠시 후 다시 시도해 주세요."
                    : $"광고 재생 실패\n{detail}";
            case AdPlaybackResult.Cancel:
                return "광고 시청 취소\n광고 시청이 취소되었습니다.";
            default:
                return detail ?? string.Empty;
        }
    }
}
