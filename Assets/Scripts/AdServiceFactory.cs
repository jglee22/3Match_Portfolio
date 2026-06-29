/// <summary>IAdService 구현체 선택 — Mock 기본, AdMob은 SDK 설치 시 활성화</summary>
public static class AdServiceFactory
{
    public static IAdService Create(GameServicesBootstrap settings)
    {
        if (settings != null && settings.UseAdMob)
            return new AdMobAdService(settings);

        return new MockAdService(
            settings != null ? settings.MockPlaybackSeconds : 2f,
            settings != null && settings.MockSimulateFailure,
            settings != null ? settings.MockFailureChance : 0.1f);
    }
}
