using UnityEngine;


[CreateAssetMenu(fileName = "FlySO")]
public class FlySO : ScriptableObject
{
    [Header("Rest Duration")]
    [Range(0, 20f)]
    public float minRestDuration = 2f;
    [Range(0, 20f)]
    public float maxRestDuration = 5f;

    [Header("Fly Speed")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 10.0f;
    public float speed = 2.0f;
    public float flySlowDownTime = 8.0f;
    public float flySlowDownSpeed = 0.5f;
    public float insaneSpeed = 10.0f;

    [Header("Others")]
    public float rotationSpeed = 10f;
    public float distanceToEdges = 0.2f;
    public float checkDistance = 0.15f;  // Small forward distance to project the spherecast along the normal
    public float radius = 0.15f;  // Radius to check around the target position

}
