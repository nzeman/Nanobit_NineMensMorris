using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource mainMenuMusicSource;
    [SerializeField] private AudioSource gameMusicSource;
    [SerializeField] private AudioSource sfxSource;

    public AudioClipDataHolder audioClipDataHolder;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        PlayMainMenuMusic(audioClipDataHolder.mainMenuMusic);
    }

    public void PlayMainMenuMusic(AudioClipData musicClipData, bool fade = true, float fadeDuration = 1f)
    {
        PlayMusic(mainMenuMusicSource, musicClipData, fade, fadeDuration);
    }

    public void PlayGameMusic(AudioClipData musicClipData, bool fade = true, float fadeDuration = 1f)
    {
        PlayMusic(gameMusicSource, musicClipData, fade, fadeDuration);
    }

    public void StopMainMenuMusic(bool fade = true, float fadeDuration = 1f)
    {
        StopMusic(mainMenuMusicSource, fade, fadeDuration);
    }

    public void StopGameMusic(bool fade = true, float fadeDuration = 1f)
    {
        StopMusic(gameMusicSource, fade, fadeDuration);
    }

    public void PlaySFX(AudioClipData sfxClipData)
    {
        sfxClipData.ApplyToSource(sfxSource);
        sfxSource.PlayOneShot(sfxClipData.clip);
    }

    private void PlayMusic(AudioSource source, AudioClipData musicClipData, bool fade, float fadeDuration)
    {
        if (fade)
        {
            StartCoroutine(FadeInMusic(source, musicClipData, fadeDuration));
        }
        else
        {
            musicClipData.ApplyToSource(source);
            source.Play();
        }
    }

    private void StopMusic(AudioSource source, bool fade, float fadeDuration)
    {
        if (fade)
        {
            StartCoroutine(FadeOutMusic(source, fadeDuration));
        }
        else
        {
            source.Stop();
        }
    }

    private IEnumerator FadeInMusic(AudioSource source, AudioClipData musicClipData, float duration)
    {
        if (source.isPlaying)
        {
            yield return FadeOutMusic(source, duration);
        }

        musicClipData.ApplyToSource(source);
        source.volume = 0f;
        source.Play();

        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, musicClipData.volume, currentTime / duration);
            yield return null;
        }

        source.volume = musicClipData.volume;
    }

    private IEnumerator FadeOutMusic(AudioSource source, float duration)
    {
        float currentTime = 0f;
        float startVolume = source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }
}
