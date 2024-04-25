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
        }
        else if (other.gameObject.tag == "Fly")
        {
            touchedFlyTransform = other.transform;
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

            if (
                isTouchingLandingSurface
            // || (isTouchingOtherHand && IsRightHand)
            )
            {
                GameObject splatterPrefab = GameManager.Instance.BloodSplatterPrefabs[Random.Range(0, GameManager.Instance.BloodSplatterPrefabs.Count)];

                if (isTouchingLandingSurface)
                {
                    var splatter = Instantiate(splatterPrefab, touchedFlyTransform.position, Quaternion.identity);
                    splatter.transform.up = touchedWallTransform.forward;
                }
                // else if (isTouchingOtherHand)
                // {
                //     Instantiate(splatterPrefab, touchedFlyTransform.position, Quaternion.identity);
                // }
                Destroy(touchedFlyTransform.gameObject);
                isTouchingFly = false;
                isTouchingLandingSurface = false;
                touchedWallTransform = null;
            }
        }
        yield return null;
    }
}
