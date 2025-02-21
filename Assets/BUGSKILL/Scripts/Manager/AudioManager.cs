using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource src;
    public float fadeDuration = 1;
    public float originalVolume;

    [Header("Audio Clips")]
    public AudioClip ingameClip;
    public AudioClip storeClip;
    public AudioClip endClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        originalVolume = src.volume;
    }

    public void CueGame()
    {
        PlayClip(ingameClip, true);
    }

    public void CueStore()
    {
        PlayClip(storeClip, true);
    }

    public void CueEnding()
    {
        PlayClip(endClip, false);
    }

    public void PlayClip(AudioClip clip, bool loop)
    {
        // if (src.isPlaying) FadeOut();
        // // src.Stop();
        // src.clip = clip;
        // src.loop = loop;

        // FadeIn();
        // // src.Play();
        StartCoroutine(FadeOutIn(clip, loop));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeAudio(0, originalVolume));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeAudio(originalVolume, 0));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume)
    {
        float currentTime = 0;
        src.volume = startVolume;
        src.Play();

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
            yield return null;
        }
    }


    private IEnumerator FadeOutIn(AudioClip newClip, bool loop)
    {
        float currentTime = 0;
        float startVolume = src.volume;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }

        src.Stop();
        src.clip = newClip;
        src.loop = loop;
        src.Play();

        currentTime = 0;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            src.volume = Mathf.Lerp(0, startVolume, currentTime / fadeDuration);
            yield return null;
        }
    }
}
