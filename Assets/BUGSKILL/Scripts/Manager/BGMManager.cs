using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    public AudioSource src;
    public float fadeDuration = 1;
    public float originalVolume;
    public float subtleVolume;

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
        StartCoroutine(FadeOutIn(clip, loop));
    }

    public void FadeIn()
    {
        StartCoroutine(FadeAudio(subtleVolume, originalVolume));
    }

    public void FadeOut()
    {
        StartCoroutine(FadeAudio(originalVolume, subtleVolume));
    }

    private IEnumerator FadeAudio(float startVolume, float targetVolume)
    {
        float currentTime = 0;
        src.volume = startVolume;

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
