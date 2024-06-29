using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;

    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    [SerializeField] float fadeDuration = 0.75f;

    AudioClip currMusic;
    float originalMusicVolume;
    Dictionary<AudioId, AudioData> sfxLookup;

    Coroutine musicCoroutine;

    public static AudioManager i { get; private set; }
    private void Start()
    {
        originalMusicVolume = VolumeSettings.instance.musicSlider.value;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    private void Awake()
    {
        i = this;
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null)
        {
            return;
        }

        if (pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }
        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySfx(AudioId audioId, bool pauseMusic = false)
    {
        if (!sfxLookup.ContainsKey(audioId))
        {
            return;
        }
        var audioData = sfxLookup[audioId];
        PlaySfx(audioData.clip, pauseMusic);
    }

    
    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if ((clip == null || clip == currMusic) && clip != WordManager.i.sceneMusic)
        {
            Debug.Log("nag return lods");
            return;
        }

        currMusic = clip;

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
        }

        // Start the new coroutine and store a reference to it
        musicCoroutine = StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if (fade)
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.time = 0f;
        musicPlayer.Play();

        if (fade)
        {
            yield return musicPlayer.DOFade(originalMusicVolume, fadeDuration).WaitForCompletion();
        }

        if (loop)
        {
            while (true)
            {
                yield return new WaitForSeconds(clip.length);
                musicPlayer.time = 0f; // Reset to the original start of the music
            }
        }
    }

    public void PlayMusicWithLoopPoints(AudioClip clip, bool loop = true, bool fade = false, float loopStartPoint = 0f, float loopEndPoint = 0f)
    {
        if (clip == null || clip == currMusic)
        {
            return;
        }

        currMusic = clip;

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
        }

        // Start the new coroutine and store a reference to it
        musicCoroutine = StartCoroutine(PlayMusicAsyncWithLoopPoints(clip, loop, fade, loopStartPoint, loopEndPoint));
    }

    IEnumerator PlayMusicAsyncWithLoopPoints(AudioClip clip, bool loop, bool fade, float loopStartPoint, float loopEndPoint)
    {
        if (fade)
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = false; // We'll handle looping manually
        musicPlayer.time = 0f; // Start from the original start of the music
        musicPlayer.Play();


        if (fade)
        {
            yield return musicPlayer.DOFade(originalMusicVolume, fadeDuration).WaitForCompletion();
        }

        if (loop && loopEndPoint > loopStartPoint)
        {
            float loopDuration = loopEndPoint - loopStartPoint;
            // Wait until the loop end point is reached
            while (musicPlayer.time < loopEndPoint)
            {
                yield return null;
            }
            while (true)
            {
                musicPlayer.time = loopStartPoint;
                yield return new WaitForSeconds(loopDuration);
            }
        }

    }
    public IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0f;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVolume, fadeDuration);
    }

    public IEnumerator PauseMusic()
    {
        yield return musicPlayer.DOFade(0, fadeDuration);
        musicPlayer.Pause();
        //yield return musicPlayer.volume = 0;
    }

    public IEnumerator PauseBGM()
    {
        musicPlayer.Pause();
        yield break;
    }

    public IEnumerator UnPauseBGM()
    {
        musicPlayer.UnPause();
        yield break;
    }

    public IEnumerator StopBGM()
    {
        StopCoroutine(musicCoroutine);
        musicPlayer.Stop();
        yield break;
    }

    public IEnumerator FadeMusic()
    {
        yield return musicPlayer.DOFade(0, fadeDuration);
    }
}


public enum AudioId { UISelect, Hit, Faint, ExpGain, ItemObtained, FakemonObtained, PlayerJump, EnterDoor, FakemonHeal, GameOver, MenuOpen, MenuClose, EndlessPause, EndlessGameOver }

[System.Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
