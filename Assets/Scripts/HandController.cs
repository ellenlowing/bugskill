
using Meta.WitAi;
using Oculus.Interaction.PoseDetection.Debug;
using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public bool IsRightHand;
    public SettingSO settings;
    public GameObject HandSplat;
    public float BloodSplatTimeout = 2f;
    public FroggyController FroggyController;
    // public OVRSkeleton HandSkeleton;

    [Header("Events")]
    [Space(20)]
    [SerializeField] private FVEventSO ScoreUpdateEvent;

    bool isTouchingLandingSurface = false;
    bool isTouchingOtherHand = false;
    bool isTouchingFly = false;
    Transform touchedWallTransform = null;
    Transform touchedFlyTransform = null;
    float BloodSplatTimer = 0;

    private int totalCash = 0;

    void Start()
    {
        // if (HandSkeleton)
        // {
        //     foreach (var capsule in HandSkeleton.Capsules)
        //     {
        //         capsule.CapsuleCollider.isTrigger = true;
        //         capsule.CapsuleRigidbody.isKinematic = true;
        //         capsule.CapsuleCollider.gameObject.tag = "Hands";
        //     }
        // }
    }

    void Update()
    {
        if (Time.time - BloodSplatTimer > BloodSplatTimeout)
        {
            HandSplat.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            isTouchingLandingSurface = true;
            touchedWallTransform = other.transform;

            StartCoroutine(CheckFlyHit());
        }
        else if (other.gameObject.tag == "Hands")
        {
            isTouchingOtherHand = true;
            StartCoroutine(CheckFlyHit());
        }
        else if (other.gameObject.tag == "Fly")
        {
            touchedFlyTransform = other.transform;

            // slow mid air if only fly is touched
            if (!isTouchingLandingSurface && !isTouchingOtherHand)
            {
                if (!FroggyController.IsActive || (FroggyController.IsActive && FroggyController.FroggyActiveHand == null))
                {
                    other.GetComponent<SlowDown>().SlowDownFly();
                }
            }
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

            if (isTouchingLandingSurface || isTouchingOtherHand)
            {
                GameObject splatterPrefab = GameManager.Instance.BloodSplatterPrefabs[Random.Range(0, GameManager.Instance.BloodSplatterPrefabs.Count)];

                if (isTouchingLandingSurface)
                {
                    var splatter = Instantiate(splatterPrefab, touchedFlyTransform.position, Quaternion.identity);
                    splatter.transform.up = touchedFlyTransform.up;
                    splatter.transform.parent = GameManager.Instance.BloodSplatContainer;
                    CashSlimFly();
                    settings.Cash += (int)SCOREFACTOR.SLAP;
                    totalCash += (int)SCOREFACTOR.SLAP;
                    UIManager.Instance.IncrementKill(touchedFlyTransform.position, totalCash);
                    totalCash = 0;
                    Debug.Log("Instantiating splatter on wall");
                }
                else if (isTouchingOtherHand)
                {
                    HandSplat.SetActive(true);
                    BloodSplatTimer = Time.time;

                    if (IsRightHand)
                    {
                        CashSlimFly();
                        settings.Cash += (int)SCOREFACTOR.CLAP;
                        totalCash += (int)SCOREFACTOR.CLAP;
                        UIManager.Instance.IncrementKill(touchedFlyTransform.position, totalCash);
                        totalCash = 0;
                    }
                }

                if (touchedFlyTransform.gameObject.TryGetComponent<FlyAudioSource>(out var buzzSource))
                {
                    var audioSource = splatterPrefab.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = splatterPrefab.AddComponent<AudioSource>();
                    }
                    audioSource.clip = buzzSource.flySplatClip;
                    audioSource.pitch = Random.Range(0.1f, 2.0f);
                    audioSource.spatialBlend = 1f;
                    audioSource.volume = 1f;
                    audioSource.loop = false;
                    audioSource.spatialize = true;
                    audioSource.playOnAwake = true;
                    var metaAudio = splatterPrefab.GetComponent<MetaXRAudioSource>();
                    if (metaAudio == null)
                    {
                        metaAudio = splatterPrefab.AddComponent<MetaXRAudioSource>();
                    }
                    metaAudio.EnableSpatialization = true;
                    audioSource.Play();
                }

                Destroy(touchedFlyTransform.gameObject);

                isTouchingFly = false;
                isTouchingLandingSurface = false;
                touchedWallTransform = null;
            }
        }
        yield return null;
    }

    private void CashSlimFly()
    {
        if (touchedFlyTransform.localScale == new Vector3(1f, 1f, 1f))
        {
            settings.Cash += (int)SCOREFACTOR.SLIM;
            totalCash += (int)SCOREFACTOR.SLIM;
        }
        else
        {
            settings.Cash += (int)SCOREFACTOR.FAT;
            totalCash += (int)SCOREFACTOR.FAT;
        }
    }
}
