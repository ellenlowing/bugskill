using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeSpiral : MonoBehaviour
{
    public Vector3 direction;
    public SettingSO settings;
    [HideInInspector] public bool canRotate = false;

    private void Update()
    {
        if (canRotate)
        {
            transform.Rotate(direction * settings.eyeSpiralSpeed * Time.deltaTime);
        } 
    }
}
