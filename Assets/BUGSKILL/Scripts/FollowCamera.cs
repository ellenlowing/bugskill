using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform cameraTransform; // Assign the VR camera (head) in Inspector
    public float followSpeed = 2f;    // Speed of interpolation
    public float distance = 0.8f;       // Distance in front of the user
    public Vector3 offset = new Vector3(0, 0.3f, 0); // Adjust height offset

    void Start()
    {
        ResetPosition();
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // Calculate target position in front of the camera
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * distance + offset;

        // Smoothly interpolate position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Lock rotation to face the user but prevent tilting
        Vector3 lookDirection = transform.position - cameraTransform.position;
        lookDirection.y = 0; // Remove tilt influence
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    public void ResetPosition()
    {
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * distance + offset;
        transform.position = targetPosition;
    }
}
