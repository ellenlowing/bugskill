using UnityEngine;


[CreateAssetMenu(fileName = "FlySO")]
public class FlySO : ScriptableObject
{

    public float minRestDuration = 2f;
    public float maxRestDuration = 5f;
    public float minSpeed = 2.0f;
    public float maxSpeed = 10.0f;
    public float speed = 2.0f;
    public float rotationSpeed = 10f;
    public float distanceToEdges = 0.2f;
    public float checkDistance = 0.15f;  // Small forward distance to project the spherecast along the normal
    public float radius = 0.15f;  // Radius to check around the target position
    public float flySlowDownTime = 8.0f;
    public float flySlowDownSpeed = 0.5f;
}
