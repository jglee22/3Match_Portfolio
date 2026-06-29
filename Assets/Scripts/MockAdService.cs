using System;
using System.Collections;
using UnityEngine;

/// <summary>Mock 광고 재생 — 2초 대기 후 성공/실패/취소 결과 반환</summary>
public class MockAdService : IAdService
{
    readonly float playbackSeconds;
    readonly bool simulateFailure;
    readonly float failureChance;

    public MockAdService(float playbackSeconds = 2f, bool simulateFailure = false, float failureChance = 0.1f)
    {
        this.playbackSeconds = playbackSeconds;
        this.simulateFailure = simulateFailure;
        this.failureChance = Mathf.Clamp01(failureChance);
    }

    public IEnumerator PlayRewardedAd(Action<AdPlaybackResult> onComplete, Func<bool> isCancelled)
    {
        float elapsed = 0f;

        while (elapsed < playbackSeconds)
        {
            if (isCancelled != null && isCancelled())
            {
                onComplete?.Invoke(AdPlaybackResult.Cancel);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (isCancelled != null && isCancelled())
        {
            onComplete?.Invoke(AdPlaybackResult.Cancel);
            yield break;
        }

        if (simulateFailure && UnityEngine.Random.value < failureChance)
        {
            onComplete?.Invoke(AdPlaybackResult.Failure);
            yield break;
        }

        onComplete?.Invoke(AdPlaybackResult.Success);
    }
}
