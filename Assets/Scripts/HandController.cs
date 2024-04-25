using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public GameObject SplatterPrefab;

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
            // GetComponent<Renderer>().material.color = Color.green;
            isTouchingLandingSurface = true;
            touchedWallTransform = other.transform;
        }
        else if (other.gameObject.tag == "Hands")
        {
            // GetComponent<Renderer>().material.color = Color.red;
            isTouchingOtherHand = true;
        }
        else if (other.gameObject.tag == "Fly")
        {
            // GetComponent<Renderer>().material.color = Color.yellow;
            // isTouchingFly = true;
            touchedFlyTransform = other.transform;
            StartCoroutine(CheckFlyHit());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            // GetComponent<Renderer>().material.color = Color.blue;
            isTouchingLandingSurface = false;
            touchedWallTransform = null;
        }
        else if (other.gameObject.tag == "Hands")
        {
            // GetComponent<Renderer>().material.color = Color.blue;
            isTouchingOtherHand = false;
        }
        else if (other.gameObject.tag == "Fly")
        {
            // GetComponent<Renderer>().material.color = Color.blue;
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
                if (isTouchingLandingSurface)
                {
                    Instantiate(SplatterPrefab, touchedFlyTransform.position, Quaternion.LookRotation(touchedWallTransform.right, Vector3.up));
                }
                else if (isTouchingOtherHand)
                {
                    Instantiate(SplatterPrefab, touchedFlyTransform.position, Quaternion.identity);

                }
                Destroy(touchedFlyTransform.gameObject);
                isTouchingFly = false;
            }
        }
        yield return null;
    }
}
