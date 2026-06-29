using UnityEngine;

/// <summary>광고·상점 서비스 생성 및 모드 전환 (Mock ↔ AdMob)</summary>
public class GameServicesBootstrap : MonoBehaviour
{
    public enum AdProvider
    {
        Mock,
        AdMob,
    }

    public static GameServicesBootstrap Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoEnsure()
    {
        Ensure();
    }

    /// <summary>씬에 없으면 생성. Lobby/Main 어디서 시작해도 보장.</summary>
    public static GameServicesBootstrap Ensure()
    {
        if (Instance != null) return Instance;

        var existing = FindAnyObjectByType<GameServicesBootstrap>();
        if (existing != null) return existing;

        var go = new GameObject("GameServicesBootstrap");
        return go.AddComponent<GameServicesBootstrap>();
    }

    [Header("광고")]
    [SerializeField] AdProvider adProvider = AdProvider.Mock;
    [SerializeField] bool useAdMobInEditor;
    [Tooltip("Google 테스트 ID (Android)")]
    [SerializeField] string androidRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    [Tooltip("Google 테스트 ID (iOS)")]
    [SerializeField] string iosRewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";

    [Header("Mock 광고 옵션")]
    [SerializeField] float mockPlaybackSeconds = 2f;
    [SerializeField] bool mockSimulateFailure;
    [SerializeField] [Range(0f, 1f)] float mockFailureChance = 0.1f;

    public IShopService ShopService { get; private set; }
    public IAdService AdService { get; private set; }

    public float MockPlaybackSeconds => mockPlaybackSeconds;
    public bool MockSimulateFailure => mockSimulateFailure;
    public float MockFailureChance => mockFailureChance;

    public string RewardedAdUnitId
    {
        get
        {
#if UNITY_IOS
            return iosRewardedAdUnitId;
#else
            return androidRewardedAdUnitId;
#endif
        }
    }

    public bool UseAdMob =>
        adProvider == AdProvider.AdMob
        && (useAdMobInEditor || !Application.isEditor);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ShopService = new MockShopService();
        AdService = AdServiceFactory.Create(this);

        if (UseAdMob)
        {
            AdMobAdService.InitializeSdk();
            Debug.Log($"[GameServices] AdMob 모드 — Reward Unit: {RewardedAdUnitId}");
        }
    }

    public void RebuildAdService()
    {
        AdService = AdServiceFactory.Create(this);
    }
}
