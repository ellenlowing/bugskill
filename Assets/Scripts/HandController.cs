
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public bool IsRightHand;
    public UIManager UIM;
    public SettingSO settings;

    bool isTouchingLandingSurface = false;
    bool isTouchingOtherHand = false;
    bool isTouchingFly = false;
    Transform touchedWallTransform = null;
    Transform touchedFlyTransform = null;

    void Start()
    {

    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == 6)
        {
            isTouchingLandingSurface = true;
            touchedWallTransform = other.transform;
        }
        else if (other.gameObject.tag == "Hands")
        {
            isTouchingOtherHand = true;
            // if (other.gameObject.tag == "Fly")
            // {
            //     touchedFlyTransform = other.transform;
            // }
        }
        else if (other.gameObject.tag == "Fly")
        {
            touchedFlyTransform = other.transform;

            // slow mid air if only fly is touched
            if (!isTouchingLandingSurface)
            {
                //SlowedDownFlyTransform = touchedFlyTransform;
                other.GetComponent<SlowDown>().SlowDownFly();
            }

            StartCoroutine(CheckFlyHit());
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            isTouchingLandingSurface = false;
            touchedWallTransform = null;
        }
        else if (other.gameObject.tag == "Hands")
        {
            isTouchingOtherHand = false;
        }
        else if (other.gameObject.tag == "Fly")
        {
            isTouchingFly = false;
            touchedFlyTransform = null;
        }
    }


    IEnumerator CheckFlyHit()
    {
        if (!isTouchingFly && touchedFlyTransform != null)
        {
            isTouchingFly = true;

            if (isTouchingLandingSurface || (isTouchingOtherHand && IsRightHand))
            {
                GameObject splatterPrefab = GameManager.Instance.BloodSplatterPrefabs[Random.Range(0, GameManager.Instance.BloodSplatterPrefabs.Count)];

                if (isTouchingLandingSurface)
                {
                    var splatter = Instantiate(splatterPrefab, touchedFlyTransform.position, Quaternion.identity);
                    splatter.transform.up = touchedFlyTransform.up;

                    Debug.Log("Instantiating splatter on wall");
                }
                else if (isTouchingOtherHand)
                {
                    if (IsRightHand)
                    {
                        var splatter = Instantiate(splatterPrefab, transform);
                        splatter.transform.up = -transform.up;
                        splatter.transform.localScale = splatter.transform.localScale * 2f;
                        // splatter.transform.parent = transform;
                        splatter.transform.localPosition = Vector3.zero;
                        Destroy(splatter, 2f);
                    }
                    // Instantiate(GameManager.Instance.splatterParticle, touchedFlyTransform.position, Quaternion.identity);
                    Debug.Log("Two Hand Smash");
                }

                if (touchedFlyTransform.gameObject.TryGetComponent<FlyAudioSource>(out var buzzSource))
                {
                    var audioSource = splatterPrefab.AddComponent<AudioSource>();
                    audioSource.clip = buzzSource.flySplatClip;
                    audioSource.pitch = Random.Range(0.1f, 2.0f);
                    audioSource.spatialBlend = 1f;
                    audioSource.volume = 10f;
                    audioSource.loop = false;
                    audioSource.spatialize = true;
                    audioSource.playOnAwake = true;
                    var metaAudio = splatterPrefab.AddComponent<MetaXRAudioSource>();
                    metaAudio.EnableSpatialization = true;
                    //metaAudio.
                    audioSource.Play();
                }

                Destroy(touchedFlyTransform.gameObject);

                isTouchingFly = false;
                isTouchingLandingSurface = false;
                touchedWallTransform = null;


                // game flow updates
                settings.numberOfKills += 1;
                // settings.score += settings.scoreMulFactor;
                // UIM.ScoreUpdate();
                // UIM.KillUpdate();
            }
        }
        yield return null;
    }
}
