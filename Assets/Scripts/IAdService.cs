using System;
using System.Collections;

/// <summary>리워드 광고 재생 추상화 — 실제 SDK 연동 시 구현체만 교체</summary>
public interface IAdService
{
    IEnumerator PlayRewardedAd(Action<AdPlaybackResult> onComplete, Func<bool> isCancelled);
}
