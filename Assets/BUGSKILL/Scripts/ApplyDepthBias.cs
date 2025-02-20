using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.EnvironmentDepth;

public class ApplyDepthBias : MonoBehaviour
{
    private SettingSO settings;
    private MeshRenderer[] _renderers;

    void Start()
    {
        settings = GameManager.Instance.settings;
        if (EnvironmentDepthManager.IsSupported)
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
            SetDepthBias(settings.EnvironmentDepthBias);
        }
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
