using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.ImmersiveDebugger;

public class DebugManager : MonoBehaviour
{
    [DebugMember(GizmoType = DebugGizmoType.Axis, Category = "Hands")]
    public Transform LeftHand;

    [DebugMember(GizmoType = DebugGizmoType.Axis, Category = "Hands")]
    public Transform RightHand;


    [DebugMember(Tweakable = true, Min = -0.1f, Max = 0.1f)]
    private float _xOffset;

    [DebugMember(Tweakable = true, Min = -0.05f, Max = 0.05f)]
    private float _yOffset;

    [DebugMember(Tweakable = true, Min = -0.05f, Max = 0.05f)]
    private float _zOffset;

    // void Update()
    // {
    //     LeftHand.transform.localPosition = new Vector3(_xOffset, _yOffset, _zOffset);
    //     RightHand.transform.localPosition = new Vector3(_xOffset, _yOffset, _zOffset);
    // }
}
