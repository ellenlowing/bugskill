using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwatterActivateButton : MonoBehaviour
{
    public bool Activated = false;
    public UnityEvent WhenActivated;
    public UnityEvent WhenDeactivated;

    void Start()
    {

    }

    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Swatter btn is touching: " + other.gameObject.name);

        if (!Activated && other.gameObject.CompareTag("Thumb"))
        {
            Debug.Log("Swatter btn is touching thumb");

            Activated = true;
            WhenActivated.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (Activated && other.gameObject.CompareTag("Thumb"))
        {
            Debug.Log("Swatter btn is no longer touching thumb");

            Activated = false;
            WhenDeactivated.Invoke();
        }
    }
}
