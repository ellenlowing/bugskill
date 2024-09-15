
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
    public GameObject HandSplat;
    public float BloodSplatTimeout = 2f;
    public ParticleSystem BloodSplatParticles;

    [Header("Events")]
    [SerializeField] private FVEventSO ScoreUpdateEvent;

    bool isTouchingLandingSurface = false;
    bool isTouchingOtherHand = false;
    bool isTouchingFly = false;
    Transform touchedFlyTransform = null;
    float BloodSplatTimer = 0;

    private int totalCash = 0;
    private SettingSO settings;

    void Start()
    {
        settings = GameManager.Instance.settings;
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
        if (other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.LandingLayerName) || other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.FloorLayerName))
        {
            isTouchingLandingSurface = true;

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

            // slow down if fly is touched by single hand in mid air
            if (!isTouchingLandingSurface && !isTouchingOtherHand)
            {
                // if (!FroggyController.Instance.IsActive || (FroggyController.Instance.IsActive && FroggyController.Instance.ActiveOVRHand == null))
                // {
                other.GetComponent<BaseFlyBehavior>().SlowDown();
                // }
            }
        }
        else if (other.gameObject.tag == "TNT")
        {
            GameManager.Instance.TriggerTNT(other.transform.position, other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.LandingLayerName) || other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.FloorLayerName))
        {
            isTouchingLandingSurface = false;
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
                bool isTouchingMC = touchedFlyTransform.gameObject.name == "MC Fly";

                if (isTouchingLandingSurface)
                {
                    var splatter = Instantiate(splatterPrefab, touchedFlyTransform.position, Quaternion.identity);
                    splatter.transform.up = touchedFlyTransform.up;
                    splatter.transform.parent = GameManager.Instance.BloodSplatContainer;

                    if (!isTouchingMC)
                    {
                        UIManager.Instance.IncrementKill(touchedFlyTransform.position, (int)SCOREFACTOR.SLAP);
                    }

                    var audioSource = splatter.GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = splatter.AddComponent<AudioSource>();
                        Debug.Log("NO AUDIO CLIP FOUND IN SPLATTER PREFAB");
                    }
                    audioSource.pitch = Random.Range(0.1f, 1.5f);
                    audioSource.Stop();
                    audioSource.Play();
                }
                else if (isTouchingOtherHand)
                {
                    HandSplat.SetActive(true);
                    BloodSplatTimer = Time.time;

                    if (IsRightHand)
                    {
                        BloodSplatParticles.transform.position = touchedFlyTransform.position;
                        BloodSplatParticles.Stop();
                        BloodSplatParticles.Play();
                        BloodSplatParticles.gameObject.GetComponent<AudioSource>().Play();
                        if (!isTouchingMC)
                        {
                            UIManager.Instance.IncrementKill(touchedFlyTransform.position, (int)SCOREFACTOR.CLAP);
                        }
                    }
                }

                if (IsRightHand && isTouchingMC)
                {
                    UIManager.Instance.StartGameLoopTrigger();
                    Debug.Log("MC FLY HIT: starting game loop");
                }

                settings.flies.Remove(touchedFlyTransform.gameObject);
                Destroy(touchedFlyTransform.gameObject);

                isTouchingFly = false;
                isTouchingLandingSurface = false;
            }
        }
        yield return null;
    }
}
