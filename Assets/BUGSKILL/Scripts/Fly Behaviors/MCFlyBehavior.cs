using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MCFlyBehavior : MonoBehaviour
{
    public AudioSource src;
    public float originalVolume;
    private bool _isPlaying = false;

    void OnEnable()
    {
        originalVolume = src.volume;
    }

    public void PlayClip(AudioClip clip, float delay = 0, bool loop = false)
    {
        // NOTE: delay must be larger than BGMManager fadeDuration*2
        if (delay < BGMManager.Instance.fadeDuration * 2)
        {
            delay = BGMManager.Instance.fadeDuration * 2;
        }
        StartCoroutine(DelayPlay(clip, delay, loop));
        StartCoroutine(WaitForAudioEnd(clip, delay));
    }

    private IEnumerator DelayPlay(AudioClip newClip, float delay, bool loop)
    {
        yield return new WaitForSeconds(delay);
        BGMManager.Instance.FadeOut();

        src.Stop();
        src.clip = newClip;
        src.loop = loop;
        src.Play();
        _isPlaying = true;
    }

    private IEnumerator WaitForAudioEnd(AudioClip newClip, float delay)
    {
        yield return new WaitForSeconds(newClip.length + delay);
        BGMManager.Instance.FadeIn();
        _isPlaying = false;
    }
}