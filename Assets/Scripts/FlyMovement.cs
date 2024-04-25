using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMovement : MonoBehaviour
{
    public float minSpeed = 2.0f;
    public float maxSpeed = 10.0f;
    public float rotationSpeed = 5.0f;          // Rotation speed of the fly
    public float minRestTime = 2.0f;
    public float maxRestTime = 5.0f;
    public float takeOffTime = 0.5f;             // Time the fly takes off from the landing surface
    public float detectionRange = 2.0f;          // Range to detect landing surfaces
    public float takeOffChance = 0.8f;
    public LayerMask landingSurfaceLayer;        // Layer for landing surfaces

    private bool isResting = false;              // Flag to check if the fly is resting
    private Vector3 randomDirection;             // Random direction for the fly to move
    private float timeSinceLastRest;
    private Vector3 closestPoint;
    private float restTime = 2.0f;
    private float speed = 2.0f;
    private bool restedOnce = false;

    private void Start()
    {
        // Set initial random direction
        randomDirection = Random.insideUnitSphere.normalized;
        timeSinceLastRest = takeOffTime;
        restTime = Random.Range(minRestTime, maxRestTime);
        speed = Random.Range(minSpeed, maxSpeed);
    }

    private void Update()
    {
        if (!isResting)
        {
            // Move the fly
            transform.position += randomDirection * speed * Time.deltaTime;
            timeSinceLastRest += Time.deltaTime;

            // If fly has had enough time to leave landed surface
            bool shouldLand = timeSinceLastRest > takeOffTime;

            // Rotate the fly towards the moving direction
            Quaternion lookRotation = Quaternion.LookRotation(randomDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

            // Check for landing surfaces
            if (shouldLand)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, landingSurfaceLayer);

                if (hitColliders.Length > 0)
                {
                    // Get closest landing surface
                    Collider closestCollider = null;
                    float closestDistance = float.MaxValue;

                    foreach (Collider col in hitColliders)
                    {
                        float distance = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestCollider = col;
                        }
                    }

                    // If landing surface detected, start resting
                    if (!restedOnce || Random.Range(0, 1f) < takeOffChance)
                    {
                        StartCoroutine(Rest(closestCollider));
                    }
                    else
                    {
                        randomDirection = GenerateOppositeDirection(closestCollider.transform);
                    }
                }
            }
        }
    }

    private IEnumerator Rest(Collider landingSurface)
    {
        if (!restedOnce)
        {
            restedOnce = true;
        }
        isResting = true;
        transform.position = landingSurface.ClosestPoint(transform.position);
        closestPoint = landingSurface.ClosestPoint(transform.position);
        transform.up = landingSurface.transform.forward;
        transform.rotation = transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
        yield return new WaitForSeconds(restTime);
        isResting = false;
        timeSinceLastRest = 0;
        randomDirection = GenerateOppositeDirection(landingSurface.transform);
    }

    private Vector3 GenerateOppositeDirection(Transform wallTransform)
    {
        // Get the wall's surface normal
        Vector3 wallNormal = wallTransform.forward; // Assuming the wall's forward direction is its surface normal

        // Generate a random direction
        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        // Calculate a direction somewhat opposite to the wall's normal
        Vector3 oppositeDirection = wallNormal + randomDirection * 0.5f;

        // Normalize the direction
        oppositeDirection.Normalize();

        return oppositeDirection;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(closestPoint, 0.1f);
    }
}
