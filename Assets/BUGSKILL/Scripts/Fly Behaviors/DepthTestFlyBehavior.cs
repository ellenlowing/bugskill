using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTestFlyBehavior : BaseFlyBehavior
{
    private MeshRenderer[] _renderers;
    private float _EnvironmentDepthBias = 0;

    new void Start()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>();
    }

    new void Update()
    {

    }

    public void AdjustDepthBias(float value)
    {
        _EnvironmentDepthBias += value;
        foreach (var renderer in _renderers)
        {
            renderer.material.SetFloat("_EnvironmentDepthBias", _EnvironmentDepthBias);
        }
    }
}
