using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.rotation = Quaternion.Euler(-90f, Time.time * rotationSpeed, 0);
    }
}
