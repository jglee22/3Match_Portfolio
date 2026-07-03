using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    const string BgmEnabledKey = "settings_bgm_enabled";
    const string SfxEnabledKey = "settings_sfx_enabled";

    [Header("오디오 소스")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("오디오 클립")]
    public AudioClip defaultBGM;
    public AudioClip clearBGM;
    public AudioClip failBGM;
    public AudioClip matchSFX;
    public AudioClip specialMatchSFX;

    AudioClip pendingBgm;

    public static bool IsBgmEnabled => PlayerPrefs.GetInt(BgmEnabledKey, 1) == 1;
    public static bool IsSfxEnabled => PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ApplySettings();
    }

    void Start()
    {
        PlayBGM(defaultBGM);
    }

    public static void SetBgmEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(BgmEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        Instance?.ApplySettings();
    }

    public static void SetSfxEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SfxEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        Instance?.ApplySettings();
    }

    public void ApplySettings()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !IsBgmEnabled;
            if (IsBgmEnabled && pendingBgm != null && !bgmSource.isPlaying)
                PlayBGM(pendingBgm);
            else if (!IsBgmEnabled)
                bgmSource.Stop();
        }

        if (sfxSource != null)
            sfxSource.mute = !IsSfxEnabled;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        pendingBgm = clip;
        if (!IsBgmEnabled)
        {
            bgmSource.clip = clip;
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!IsSfxEnabled || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
            bgmSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
    }
}
