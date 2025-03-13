using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teaser : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float hueSpeed = 1f;
    public Material material;
    private float hue;

    void Start()
    {
        Color.RGBToHSV(material.color, out hue, out _, out _);
    }

    void Update()
    {
        transform.localEulerAngles = new Vector3(0, 0, Time.time * rotationSpeed);

        hue += hueSpeed * Time.deltaTime;
        if (hue > 1) hue -= 1; // Keep hue in the range [0,1]

        // Convert back to RGB and set the material color
        material.color = Color.HSVToRGB(hue, 1, 1);
    }
}
