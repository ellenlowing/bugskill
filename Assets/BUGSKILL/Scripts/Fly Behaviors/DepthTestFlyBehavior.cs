using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTestFlyBehavior : BaseFlyBehavior
{
    private MeshRenderer[] _renderers;

    new void Start()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>();
        TakeoffChance = 0;
    }

    new void Update()
    {
    }

    public void SetDepthBias(float value)
    {
        foreach (var renderer in _renderers)
        {
            Material[] materials = renderer.materials;
            foreach (var material in materials)
            {
                material.SetFloat("_EnvironmentDepthBias", value);
            }
        }
    }
}
