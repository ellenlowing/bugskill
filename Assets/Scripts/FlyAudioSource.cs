using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAudioSource : MonoBehaviour
{
    public List<AudioClip> flyMovingClip;
    public AudioClip flyDizzyClip;
    public AudioClip flySplatClip;
    private AudioClip prevClip;
    private FlyMovement flyMove;
    public float minVelocity = 0.1f;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, flyMovingClip.Count);
        if (idx > 1)
        {
            transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        }
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = flyMovingClip[idx];
        audioSource.spatialBlend = 1f;
        audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
        audioSource.loop = true;
        audioSource.spatialize = true;
        audioSource.playOnAwake = true;
        flyMove = gameObject.GetComponent<FlyMovement>();
        var metaAudio = gameObject.AddComponent<MetaXRAudioSource>();
        metaAudio.EnableSpatialization = true;
        audioSource.Play();
    }

    public void DizzyClip()
    {
        prevClip = audioSource.clip;
        audioSource.clip = flyDizzyClip;
        audioSource.Play();
    }

    public void MoveClip()
    {
        audioSource.clip = prevClip;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.mute = flyMove.isResting;
    }
}
