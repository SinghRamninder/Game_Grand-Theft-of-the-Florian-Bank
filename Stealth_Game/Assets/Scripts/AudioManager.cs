using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource sfx;

    [Header("Music")]
    public AudioClip startBackground;
    public AudioClip chaseMusic;
    public AudioClip endCredits;

    [Header("SFX")]
    public AudioClip siren;

    [Header("Settings")]
    [SerializeField] private float defaultFadeDuration = 0.5f;

    private Coroutine musicFadeRoutine;
    private Coroutine sfxFadeRoutine;

    private float musicBaseVolume = 1f;
    private float sfxBaseVolume = 1f;

    private void Awake()
    {
        if (backgroundMusic) musicBaseVolume = backgroundMusic.volume;
        if (sfx) sfxBaseVolume = sfx.volume;
    }

    private void Start()
    {
        if (backgroundMusic && startBackground)
        {
            backgroundMusic.clip = startBackground;
            backgroundMusic.volume = 0f;
            backgroundMusic.Play();
            musicFadeRoutine = StartCoroutine(FadeTo(backgroundMusic, musicBaseVolume, defaultFadeDuration));
        }
    }

    // -------------------- PUBLIC MUSIC API --------------------

    public void PlayMusic(AudioClip clip, float fadeDuration = -1f, bool loop = true)
    {
        if (!backgroundMusic || clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        backgroundMusic.loop = loop;

        if (backgroundMusic.clip == clip && backgroundMusic.isPlaying)
            return;

        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(SwapClipWithFade(backgroundMusic, clip, musicBaseVolume, fadeDuration));
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        if (!backgroundMusic) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(FadeOutAndStop(backgroundMusic, fadeDuration));
    }

    public void SetMusicVolume(float volume, float fadeDuration = -1f)
    {
        if (!backgroundMusic) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        musicBaseVolume = Mathf.Clamp01(volume);

        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(FadeTo(backgroundMusic, musicBaseVolume, fadeDuration));
    }

    public void PlayMusicOnce(AudioClip clip, float fadeDuration = -1f)
    {
        if (!backgroundMusic || clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        if (musicFadeRoutine != null)
            StopCoroutine(musicFadeRoutine);

        backgroundMusic.loop = false;

        musicFadeRoutine = StartCoroutine(PlayOnceRoutine(clip, fadeDuration));
    }

    public void PauseMusic()
    {
        if (!backgroundMusic) return;
        backgroundMusic.Pause();
    }

    public void ResumeMusic()
    {
        if (!backgroundMusic) return;
        backgroundMusic.UnPause();
    }

    public float GetMusicVolume()
    {
        return musicBaseVolume;
    }

    // -------------------- PUBLIC SFX API --------------------

    public void PlaySFXOneShot(AudioClip clip, float volumeScale = 1f, float fadeDuration = -1f)
    {
        if (!sfx || clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        // Quick fade-in on the SFX source volume, then OneShot.
        if (sfxFadeRoutine != null) StopCoroutine(sfxFadeRoutine);
        sfxFadeRoutine = StartCoroutine(FadeTo(sfx, sfxBaseVolume, fadeDuration));

        sfx.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlaySFXLoop(AudioClip clip, float fadeDuration = -1f)
    {
        if (!sfx || clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        sfx.loop = true;

        if (sfx.clip == clip && sfx.isPlaying)
            return;

        if (sfxFadeRoutine != null) StopCoroutine(sfxFadeRoutine);
        sfxFadeRoutine = StartCoroutine(SwapClipWithFade(sfx, clip, sfxBaseVolume, fadeDuration));
    }

    public void StopSFX(float fadeDuration = -1f)
    {
        if (!sfx) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        if (sfxFadeRoutine != null) StopCoroutine(sfxFadeRoutine);
        sfxFadeRoutine = StartCoroutine(FadeOutAndStop(sfx, fadeDuration));
    }

    public void SetSFXVolume(float volume, float fadeDuration = -1f)
    {
        if (!sfx) return;
        if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;

        sfxBaseVolume = Mathf.Clamp01(volume);

        if (sfxFadeRoutine != null) StopCoroutine(sfxFadeRoutine);
        sfxFadeRoutine = StartCoroutine(FadeTo(sfx, sfxBaseVolume, fadeDuration));
    }

    public float GetSFXVolume()
    {
        return sfxBaseVolume;
    }

    public void PauseSFX()
    {
        if (!sfx) return;
        sfx.Pause();
    }

    public void ResumeSFX()
    {
        if (!sfx) return;
        sfx.UnPause();
    }

    // -------------------- OPTIONAL: QUICK HELPERS --------------------

    public void PlayChaseMusic(float fadeDuration = -1f) => PlayMusic(chaseMusic, fadeDuration);
    public void PlayStartMusic(float fadeDuration = -1f) => PlayMusic(startBackground, fadeDuration);
    public void PlayEndCredits(float fadeDuration = -1f) => PlayMusic(endCredits, fadeDuration);

    // -------------------- FADE UTILITIES --------------------

    private IEnumerator SwapClipWithFade(AudioSource source, AudioClip newClip, float targetVolume, float duration)
    {
        yield return FadeTo(source, 0f, duration);

        source.clip = newClip;
        source.volume = 0f;
        source.Play();

        yield return FadeTo(source, targetVolume, duration);
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        yield return FadeTo(source, 0f, duration);
        source.Stop();
        // keep clip as-is, so you can resume if you want
    }

    private IEnumerator FadeTo(AudioSource source, float target, float duration)
    {
        if (!source) yield break;

        float start = source.volume;

        if (duration <= 0f)
        {
            source.volume = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            source.volume = Mathf.Lerp(start, target, k);
            yield return null;
        }

        source.volume = target;
    }

    private IEnumerator PlayOnceRoutine(AudioClip clip, float fadeDuration)
    {
        yield return FadeTo(backgroundMusic, 0f, fadeDuration);

        backgroundMusic.clip = clip;
        backgroundMusic.volume = 0f;
        backgroundMusic.Play();

        yield return FadeTo(backgroundMusic, musicBaseVolume, fadeDuration);

        yield return new WaitForSecondsRealtime(clip.length);

        // Do nothing after this — music ends naturally
    }

}
