using UnityEngine;

public class DepthSorter : MonoBehaviour
{
    public Renderer transparentObjectRenderer;
    public Renderer opaqueObjectRenderer;

    void Start()
    {
        // Ensure the transparent object renders after the opaque object.
        transparentObjectRenderer.material.renderQueue = 3001;
        opaqueObjectRenderer.material.renderQueue = 3000;
    }
}