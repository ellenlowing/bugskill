using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public float yOffset = 0f;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    private void Update()
    {
        Vector3 targetPosition = GameManager.Instance.MainCameraTransform.position + GameManager.Instance.MainCameraTransform.forward * GameManager.Instance.settings.nearDistanceFromCamera;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        // transform.forward = GameManager.Instance.MainCameraTransform.forward;
        // transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}

