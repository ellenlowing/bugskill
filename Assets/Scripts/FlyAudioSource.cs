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

    void Start()
    {
        idx = Random.Range(0, flyMovingClip.Count);
        if (idx > 1)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = flyMovingClip[idx];
        audioSource.spatialBlend = 1f;
        audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
        audioSource.loop = true;
        audioSource.spatialize = true;
        audioSource.playOnAwake = true;
        var metaAudio = gameObject.AddComponent<MetaXRAudioSource>();
        metaAudio.EnableSpatialization = true;
        audioSource.Play();

        flyBehavior = GetComponent<BaseFlyBehavior>();
    }

    void Update()
    {
        audioSource.mute = flyBehavior.CurrentState == BaseFlyBehavior.FlyState.RESTING;
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
