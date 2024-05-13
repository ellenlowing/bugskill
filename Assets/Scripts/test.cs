using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{   

    /*public float minSpeed = 2.0f;
    public float maxSpeed = 10.0f;
    public float rotationSpeed = 5.0f;          // Rotation speed of the fly
    public float minRestTime = 2.0f;
    public float maxRestTime = 5.0f;
    public float takeOffTime = 0.5f;             // Time the fly takes off from the landing surface
    public float detectionRange = 2.0f;          // Range to detect landing surfaces
    public float takeOffChance = 0.8f;
    public LayerMask landingSurfaceLayer;        // Layer for landing surfaces
    
    private Vector3 targetPosition;    // Target position for the fly to land on
    private bool isResting = false;              // Flag to check if the fly is resting
    private float timeSinceLastRest;
    private float restTime = 2.0f;
    private float speed = 2.0f;
    private bool restedOnce = false;
    
    private void Start()
    {
        timeSinceLastRest = takeOffTime;
        restTime = Random.Range(minRestTime, maxRestTime);
        speed = Random.Range(minSpeed, maxSpeed);
    }
    
    private void Update()
    {
        if (!isResting)
        {
            MoveToNewPosition();
            
            // Check if the fly has reached the target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f || timeSinceLastMove > takeOffTime)
            {
                isResting = true;
                StartCoroutine(Rest());
            }
    
            // // If fly has had enough time to leave landed surface
            // bool shouldLand = timeSinceLastRest > takeOffTime;
    
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
    
    private void MoveToNewPosition()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
    
        timeSinceLastRest += Time.deltaTime;
    }
    
    private IEnumerator Rest(Collider landingSurface)
    {
        if (!restedOnce)
        {
            restedOnce = true;
        }
        isResting = true;
        transform.position = landingSurface.ClosestPoint(transform.position);
        transform.up = landingSurface.transform.forward;
        transform.rotation = transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
        yield return new WaitForSeconds(restTime);
        isResting = false;
        timeSinceLastRest = 0;
        randomDirection = GenerateOppositeDirection(landingSurface.transform);
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(closestPoint, 0.1f);
    }*/
}
