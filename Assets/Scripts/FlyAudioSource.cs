using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAudioSource : MonoBehaviour
{
    public List<AudioClip> flyMovingClip;
    public List<AudioClip> flyDizzyClip;
    public AudioClip flySplatClip;
    private FlyMovement flyMove;
    public float minVelocity = 0.1f;
    private AudioSource audioSource;
    private int idx=0;

    // Start is called before the first frame update
    void Start()
    {
        idx = Random.Range(0, flyMovingClip.Count);
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
        audioSource.loop=false;
        audioSource.clip = flyDizzyClip[idx>1?1:0];
        audioSource.Play();
    }

    public void MoveClip()
    {
        audioSource.clip = flyMovingClip[idx];
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.mute = flyMove.isResting;
    }
}
