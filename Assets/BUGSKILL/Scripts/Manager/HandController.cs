using Oculus.Interaction.PoseDetection.Debug;
using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using Oculus.Interaction;

public class HandController : MonoBehaviour
{
    public bool IsRightHand;
    public GameObject HandSplat;
    public float BloodSplatTimeout = 2f;
    public ParticleSystem BloodSplatParticles;

    bool isTouchingLandingSurface = false;
    bool isTouchingOtherHand = false;
    HandController otherHandController = null;
    bool isTouchingFly = false;
    Collider touchedSurfaceCollider = null;
    Transform touchedFlyTransform = null;
    float BloodSplatTimer = 0;
    private SettingSO settings;

    void OnEnable()
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
        if (GameManager.Instance.IsOnAnyLandingLayer(other.gameObject))
        {
            isTouchingLandingSurface = true;
            touchedSurfaceCollider = other;
            StartCoroutine(CheckFlyHit());
        }
        else if (other.gameObject.tag == "Hands")
        {
            isTouchingOtherHand = true;
            otherHandController = other.GetComponent<HandController>();
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.Instance.IsOnAnyLandingLayer(other.gameObject))
        {
            isTouchingLandingSurface = false;
            touchedSurfaceCollider = null;
        }
        else if (other.gameObject.tag == "Hands")
        {
            isTouchingOtherHand = false;
            otherHandController = null;
        }
        else if (other.gameObject.tag == "Fly")
        {
            isTouchingFly = false;
            touchedFlyTransform = null;
        }
    }

    IEnumerator CheckFlyHit()
    {
        if (!isTouchingFly && touchedFlyTransform != null && !touchedFlyTransform.GetComponent<BaseFlyBehavior>().IsKilled)
        {
            BaseFlyBehavior fly = touchedFlyTransform.GetComponent<BaseFlyBehavior>();
            isTouchingFly = true;

            if (isTouchingLandingSurface || isTouchingOtherHand)
            {
                GameObject splatterPrefab = GameManager.Instance.BloodSplatterPrefabs[Random.Range(0, GameManager.Instance.BloodSplatterPrefabs.Count)];

                if (isTouchingLandingSurface)
                {
                    Vector3 contactPoint = touchedSurfaceCollider.ClosestPoint(touchedFlyTransform.position);
                    var splatter = Instantiate(splatterPrefab, contactPoint, Quaternion.identity);
                    if (Physics.Raycast(contactPoint, transform.up, out RaycastHit hit, 0.5f, GameManager.Instance.GetAnyLandingLayerMask(), QueryTriggerInteraction.Collide))
                    {
                        splatter.transform.up = hit.normal;
                    }
                    else
                    {
                        splatter.transform.up = touchedFlyTransform.up;
                    }
                    splatter.transform.parent = GameManager.Instance.BloodSplatContainer;
                    splatter.transform.localPosition = splatter.transform.localPosition + splatter.transform.up * settings.SplatDistanceOffset;

                    BloodSplatParticles.transform.position = touchedFlyTransform.position;
                    BloodSplatParticles.Stop();
                    BloodSplatParticles.Play();

                    UIManager.Instance.IncrementKill(touchedFlyTransform.position, (int)SCOREFACTOR.SLAP);

                    // TODO create a splat class so that the audio source component can be set on init
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
                    if (otherHandController != null)
                    {
                        otherHandController.HandSplat.SetActive(true);
                        otherHandController.BloodSplatTimer = Time.time;
                    }
                    BloodSplatParticles.transform.position = touchedFlyTransform.position;
                    BloodSplatParticles.Stop();
                    BloodSplatParticles.Play();

                    // TODO create a splat class so that the audio source component can be set on init
                    BloodSplatParticles.gameObject.GetComponent<AudioSource>().Play();
                    UIManager.Instance.IncrementKill(touchedFlyTransform.position, (int)SCOREFACTOR.CLAP);
                }

                fly.Kill();

                isTouchingFly = false;
                isTouchingLandingSurface = false;
                touchedSurfaceCollider = null;
            }
        }
        yield return null;
    }
}
