using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FlyMovement : MonoBehaviour
{
    public MRUKAnchor.SceneLabels canLand = MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING;
    public SettingSO settings;
    public bool isResting = false;
    private Vector3 targetPosition;
    private Vector3 targetNormal;
    private bool needNewTarget = true;
    private bool isMoving = false;

    [HideInInspector]
    public float speed;

    private FlySO flyBehaviour;


    private void Start()
    {
        flyBehaviour = settings.flyIntelLevels[settings.waveIndex];
        speed = flyBehaviour.speed;
    }

    private void Update()
    {
        if (needNewTarget)
        {
            FindNewPosition();
        }

        // If not resting and has a target position, move to target position
        if (!isResting && !needNewTarget)
        {
            MoveTowardsTargetPosition();
        }

        // If moving and reached target position, rest 
        if (isMoving && Vector3.Distance(transform.position, targetPosition) < 0.08f)
        {
            StartCoroutine(Rest());
        }
    }

    private void MoveTowardsTargetPosition()
    {
        isMoving = true;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        // Fly rotates to face moving direction 
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), flyBehaviour.rotationSpeed * Time.deltaTime);
    }

    private void FindNewPosition()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            // Anchor labels that fly can land on (chosen in editor) 
            var labelFilter = LabelFilter.FromEnum(canLand);

            // Generate random position on any surface that is not facing down 
            // + position is not too close to anchor's edge 

            if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP | MRUK.SurfaceType.VERTICAL,flyBehaviour.distanceToEdges, labelFilter, out Vector3 position, out Vector3 normal))
            {
                CheckValidPosition(position, normal);
            }
        }
    }

    private void CheckValidPosition(Vector3 position, Vector3 normal)
    {
        Vector3 direction = (position - transform.position).normalized;

        // If there's no obstacle in the fly's path
        if (!Physics.Raycast(transform.position, direction, Vector3.Distance(transform.position, position)))
        {
            // If the player can reach the position
            if (IsPositionAccessible(position, normal))
            {
                targetPosition = position;
                targetNormal = normal;
                needNewTarget = false;
            }
        }
    }


    // Check if the position is sufficiently distant from other scene anchors
    private bool IsPositionAccessible(Vector3 position, Vector3 normal)
    {
        // Project the sphere slightly along the normal to ensure it starts checking from the surface outward
        Vector3 start = position + normal * flyBehaviour.radius;

        // Use Physics.SphereCast to check for collisions within the radius along a very short distance
        if (Physics.SphereCast(start, flyBehaviour.radius, normal, out RaycastHit hit, flyBehaviour.checkDistance))
        {
            return false;  // There is an object within the buffer zone
        }
        return true;  // No objects found within the buffer zone, position is accessible
    }


    private IEnumerator Rest()
    {
        isMoving = false;
        isResting = true;
        transform.up = targetNormal;  // Align the fly's 'up' with the surface normal
        transform.rotation = transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
        yield return new WaitForSeconds(Random.Range(flyBehaviour.minRestDuration, flyBehaviour.maxRestDuration));
        isResting = false;
        needNewTarget = true;  // Need new target position after resting 
    }

    private void OnDrawGizmos()
    {
        if (targetPosition != Vector3.zero)
        {
            // Draw the target position
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.01f);

            // Draw the raycast for checking the fly's path
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * distance); // Adjusted ray start and distance

            // Draw the spherecast for accessibility check
            Gizmos.color = Color.green;
            Vector3 startSphereCast = targetPosition + targetNormal * flyBehaviour.radius;  // Adjusted start position along the normal
            Gizmos.DrawWireSphere(startSphereCast, flyBehaviour.radius);  // Draw the sphere at the target position
            Vector3 endSphereCast = startSphereCast + targetNormal * flyBehaviour.checkDistance;  // End position of the spherecast
            Gizmos.DrawWireSphere(endSphereCast, flyBehaviour.radius);

            // Draw a line representing the spherecast path
            Gizmos.DrawLine(startSphereCast, endSphereCast);

            // Optional: Draw the normal at the target position
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(targetPosition, targetNormal * 0.5f);  // Show the direction of the normal // Draw the spherecast area
        }
    }
}



























// isResting = true;
// RaycastHit hit;
// if (Physics.Raycast(transform.position + targetNormal * 0.1f, -targetNormal, out hit, 0.2f))
// {
//     transform.position = hit.point;
//     Vector3 forwardDirection = Vector3.Cross(transform.right, targetNormal);
//     transform.rotation = Quaternion.LookRotation(forwardDirection, targetNormal);
// }
// else
// {
//     transform.up = targetNormal;
//     transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.right, targetNormal), targetNormal);
// }
// yield return new WaitForSeconds(Random.Range(minRestDuration, maxRestDuration));
// isResting = false;
// needNewTarget = true;