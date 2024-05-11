using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public bool IsRightHand;

    bool isTouchingLandingSurface = false;
    bool isTouchingOtherHand = false;
    bool isTouchingFly = false;
    Transform touchedWallTransform = null;
    Transform touchedFlyTransform = null;
    Transform SlowedDownFlyTransform = null;

    public float SlowDownTime = 4f;
    public float SlowDownSpeed = 1.0f;
    public float InitialTime = 0.0f;

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
            if(other.gameObject.tag == "Fly")
            {
                touchedFlyTransform= other.transform;
            }
            StartCoroutine(CheckFlyHit());
            
        }
        else if (other.gameObject.tag == "Fly")
        {
            touchedFlyTransform = other.transform;

            // slow mid air if only fly is touched
            if(!isTouchingLandingSurface)
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

            if (isTouchingLandingSurface | (isTouchingOtherHand && IsRightHand))
            {
                GameObject splatterPrefab = GameManager.Instance.BloodSplatterPrefabs[Random.Range(0, GameManager.Instance.BloodSplatterPrefabs.Count)];

                if (isTouchingLandingSurface)
                {
                    var splatter = Instantiate(splatterPrefab, touchedFlyTransform.position, Quaternion.identity);
                    splatter.transform.up = touchedWallTransform.forward;
                }
                else if (isTouchingOtherHand)
                {
                    Instantiate(GameManager.Instance.splatterParticle, touchedFlyTransform.position, Quaternion.identity);
                    Debug.LogWarning("Two Hand Smash");
                }

                Destroy(touchedFlyTransform.gameObject);
                isTouchingFly = false;
                isTouchingLandingSurface = false;
                touchedWallTransform = null;
            }
        }
        yield return null;
    }
}
