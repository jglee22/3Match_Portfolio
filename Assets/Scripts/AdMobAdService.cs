using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

/// <summary>AdMob 리워드 광고 어댑터 — Google Mobile Ads Unity Plugin 연동</summary>
public class AdMobAdService : IAdService
{
    static bool sdkInitialized;
    static bool sdkInitializing;

    readonly GameServicesBootstrap settings;
    readonly MockAdService fallback;

    public AdMobAdService(GameServicesBootstrap settings)
    {
        this.settings = settings;
        fallback = new MockAdService(
            settings.MockPlaybackSeconds,
            settings.MockSimulateFailure,
            settings.MockFailureChance);
    }

    public static void InitializeSdk()
    {
        if (sdkInitialized || sdkInitializing) return;

        sdkInitializing = true;
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(_ =>
        {
            sdkInitialized = true;
            sdkInitializing = false;
            Debug.Log("[AdMob] SDK 초기화 완료");
        });
    }

    public IEnumerator PlayRewardedAd(Action<AdPlaybackResult> onComplete, Func<bool> isCancelled)
    {
#if UNITY_EDITOR
        if (!settings.UseAdMob)
        {
            yield return fallback.PlayRewardedAd(onComplete, isCancelled);
            yield break;
        }
#endif

        yield return PlayRewardedAdInternal(onComplete, isCancelled);
    }

    IEnumerator PlayRewardedAdInternal(Action<AdPlaybackResult> onComplete, Func<bool> isCancelled)
    {
        if (!sdkInitialized)
            InitializeSdk();

        const float initTimeout = 20f;
        float initElapsed = 0f;
        while (!sdkInitialized && initElapsed < initTimeout)
        {
            if (isCancelled != null && isCancelled())
            {
                onComplete?.Invoke(AdPlaybackResult.Cancel);
                yield break;
            }

            initElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!sdkInitialized)
        {
            Debug.LogWarning("[AdMob] SDK 초기화 시간 초과");
            onComplete?.Invoke(AdPlaybackResult.Failure);
            yield break;
        }

        bool loadFinished = false;
        RewardedAd rewardedAd = null;
        LoadAdError loadError = null;

        string adUnitId = settings.RewardedAdUnitId;
        Debug.Log($"[AdMob] 리워드 광고 로드 시작: {adUnitId}");

        var request = new AdRequest();
        RewardedAd.Load(adUnitId, request, (ad, error) =>
        {
            rewardedAd = ad;
            loadError = error;
            loadFinished = true;
        });

        const float loadTimeout = 30f;
        float elapsed = 0f;
        while (!loadFinished && elapsed < loadTimeout)
        {
            if (isCancelled != null && isCancelled())
            {
                onComplete?.Invoke(AdPlaybackResult.Cancel);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!loadFinished || loadError != null || rewardedAd == null)
        {
            string reason = loadError != null ? loadError.GetMessage() : "timeout";
            Debug.LogWarning($"[AdMob] 광고 로드 실패: {reason}");
#if UNITY_EDITOR
            yield return fallback.PlayRewardedAd(onComplete, isCancelled);
#else
            onComplete?.Invoke(AdPlaybackResult.Failure);
#endif
            yield break;
        }

        if (!rewardedAd.CanShowAd())
        {
            Debug.LogWarning("[AdMob] 광고 로드됐지만 CanShowAd() == false");
            onComplete?.Invoke(AdPlaybackResult.Failure);
            yield break;
        }

        bool showFinished = false;
        bool rewardEarned = false;
        string showError = null;

        rewardedAd.OnAdFullScreenContentClosed += () => showFinished = true;
        rewardedAd.OnAdFullScreenContentFailed += error =>
        {
            showFinished = true;
            rewardEarned = false;
            showError = error != null ? error.GetMessage() : "unknown";
        };

        rewardedAd.Show(_ => rewardEarned = true);

        while (!showFinished)
        {
            if (isCancelled != null && isCancelled())
            {
                onComplete?.Invoke(AdPlaybackResult.Cancel);
                yield break;
            }

            yield return null;
        }

        if (!string.IsNullOrEmpty(showError))
            Debug.LogWarning($"[AdMob] 광고 표시 실패: {showError}");

        onComplete?.Invoke(rewardEarned ? AdPlaybackResult.Success : AdPlaybackResult.Failure);
    }
}
