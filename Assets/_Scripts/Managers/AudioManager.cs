using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the audio for the game, including background music for different scenes (main menu and game),
/// sound effects (SFX), and provides functionality for playing, stopping, and fading in/out of audio.
/// Implements a singleton pattern to ensure only one instance of the AudioManager exists.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton

 
    public static AudioManager Instance { get; private set; }

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

    #endregion

    #region Fields

    [SerializeField] private AudioSource mainMenuMusicSource;
    [SerializeField] private AudioSource gameMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClipDataHolder audioClipDataHolder;

    #endregion

    #region Public Methods

    /// <summary>
    /// Plays the main menu music with optional fade-in.
    /// </summary>
    /// <param name="musicClipData">The audio clip data for the main menu music.</param>
    /// <param name="fade">Whether to fade in the music.</param>
    /// <param name="fadeDuration">Duration of the fade-in effect.</param>
    public void PlayMainMenuMusic(AudioClipData musicClipData, bool fade = true, float fadeDuration = 1f)
    {
        PlayMusic(mainMenuMusicSource, musicClipData, fade, fadeDuration);
    }

    /// <summary>
    /// Plays the game music with optional fade-in.
    /// </summary>
    /// <param name="musicClipData">The audio clip data for the game music.</param>
    /// <param name="fade">Whether to fade in the music.</param>
    /// <param name="fadeDuration">Duration of the fade-in effect.</param>
    public void PlayGameMusic(AudioClipData musicClipData, bool fade = true, float fadeDuration = 1f)
    {
        PlayMusic(gameMusicSource, musicClipData, fade, fadeDuration);
    }

    /// <summary>
    /// Stops the main menu music with optional fade-out.
    /// </summary>
    /// <param name="fade">Whether to fade out the music.</param>
    /// <param name="fadeDuration">Duration of the fade-out effect.</param>
    public void StopMainMenuMusic(bool fade = true, float fadeDuration = 1f)
    {
        StopMusic(mainMenuMusicSource, fade, fadeDuration);
    }

    /// <summary>
    /// Stops the game music with optional fade-out.
    /// </summary>
    /// <param name="fade">Whether to fade out the music.</param>
    /// <param name="fadeDuration">Duration of the fade-out effect.</param>
    public void StopGameMusic(bool fade = true, float fadeDuration = 1f)
    {
        StopMusic(gameMusicSource, fade, fadeDuration);
    }

    /// <summary>
    /// Plays a sound effect (SFX).
    /// </summary>
    /// <param name="sfxClipData">The audio clip data for the sound effect.</param>
    public void PlaySFX(AudioClipData sfxClipData)
    {
        if (sfxClipData == null) return;
        sfxClipData.ApplyToSource(sfxSource);
        sfxSource.PlayOneShot(sfxClipData.clip);
    }

    /// <summary>
    /// Retrieves the audio data (clips, volume, etc.).
    /// </summary>
    public AudioClipDataHolder GetAudioData()
    {
        return audioClipDataHolder;
    }

    #endregion

    #region Private Music Control Methods

    /// <summary>
    /// Plays the specified music clip with optional fade-in.
    /// </summary>
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

    /// <summary>
    /// Stops the specified music clip with optional fade-out.
    /// </summary>
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

    #endregion

    #region Fade Coroutines

    /// <summary>
    /// Fades in the specified music over a given duration.
    /// </summary>
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

    /// <summary>
    /// Fades out the specified music over a given duration.
    /// </summary>
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

    #endregion
}
