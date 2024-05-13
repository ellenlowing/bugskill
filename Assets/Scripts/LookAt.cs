using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    private Transform Target;
    [SerializeField] private Vector3 WorldDirection = Vector3.up;

    private void Start()
    {
        Target = OVRManager.instance.GetComponent<OVRCameraRig>().centerEyeAnchor;
    }

    private void Update()
    {
        Vector3 dirToTarget = (Target.position - transform.position).normalized;
        transform.LookAt(Target.position - dirToTarget, Vector3.up);
    }
}

