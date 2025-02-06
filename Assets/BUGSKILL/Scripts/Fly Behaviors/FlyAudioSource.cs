using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAudioSource : MonoBehaviour
{
    public List<AudioClip> flyMovingClip;
    public List<AudioClip> flyDizzyClip;
    public AudioClip flySplatClip;
    public float minVelocity = 0.1f;
    private AudioSource audioSource;
    private int idx = 0;
    private BaseFlyBehavior flyBehavior;

    void Awake()
    {
        idx = Random.Range(0, flyMovingClip.Count);
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = flyMovingClip[idx];
        audioSource.spatialBlend = 1f;
        audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
        audioSource.loop = true;
        audioSource.spatialize = true;
        audioSource.playOnAwake = true;
        var metaAudio = gameObject.AddComponent<MetaXRAudioSource>();
        metaAudio.EnableSpatialization = true;

        flyBehavior = GetComponent<BaseFlyBehavior>();
    }

    public void Mute(bool muted)
    {
        audioSource.mute = muted;
    }

    public void DizzyClip()
    {
        audioSource.loop = false;
        audioSource.clip = flyDizzyClip[idx > 1 ? 1 : 0];
        audioSource.Play();
    }

    public void MoveClip()
    {
        audioSource.clip = flyMovingClip[idx];
        audioSource.loop = true;
        audioSource.Play();
    }

}
