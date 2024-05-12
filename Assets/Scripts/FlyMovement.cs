using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Random = UnityEngine.Random;

public class FlyMovement : MonoBehaviour
{
    [SerializeField]
    private MRUKAnchor.SceneLabels canLandLabels = MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING;

    public float minRestDuration = 2f;
    public float maxRestDuration = 5f;
    public float speed = 2.0f;
    public float rotationSpeed = 10f;
    public LayerMask obstacleLayers;
    public float checkDistance = 1f; // Distance to check for obstacles
    public bool isResting = false;
    
    private Vector3 targetPosition;
    private Vector3 targetNormal;
    private Vector3 lastValidPosition; // To keep track of the last known good position

    private void Start()
    {
        lastValidPosition = transform.position;
        StartCoroutine(FlyBehaviorLoop());
    }

    private void Update()
    {
        if (!isResting)
        {
            MoveTowardsTargetPosition();
        }
    }

    private IEnumerator FlyBehaviorLoop()
    {
        while (true)
        {
            FindNewPosition();
            yield return new WaitUntil(() => Vector3.Distance(transform.position, targetPosition) < 0.1f);
            StartCoroutine(Rest());
            yield return new WaitUntil(() => !isResting);
        }
    }

    private void MoveTowardsTargetPosition()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (!CheckPathClear(direction))
        {
            // Attempt to find a clear path by checking alternative directions
            direction = FindClearPath(direction);
        }

        transform.position += direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
    }

    private bool CheckPathClear(Vector3 direction)
    {
        return !Physics.Raycast(transform.position, direction, checkDistance, obstacleLayers);
    }

    private Vector3 FindClearPath(Vector3 originalDirection)
    {
        Vector3 bestDirection = Vector3.zero;
        float bestDistance = float.MaxValue;  // Use max value for comparison to find the minimum

        for (int i = 0; i < 360; i += 30) // Check every 30 degrees
        {
            Vector3 testDirection = Quaternion.Euler(0, i, 0) * Vector3.forward;
            if (CheckPathClear(testDirection))
            {
                float distance = Vector3.Distance(transform.position + testDirection * checkDistance, targetPosition);
                if (bestDirection == Vector3.zero || distance < bestDistance)
                {
                    bestDirection = testDirection;
                    bestDistance = distance;
                }
            }
        }

        // Return the opposite direction if no clear path is found
        return bestDirection == Vector3.zero ? -originalDirection : bestDirection;
    }

    private void FindNewPosition()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            var labelFilter = LabelFilter.FromEnum(canLandLabels);
            if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP | MRUK.SurfaceType.VERTICAL, 0.1f, labelFilter, out Vector3 position, out Vector3 normal)) 
            {
                targetPosition = position;
                targetNormal = normal;
            }
        }
    }

    private IEnumerator Rest()
    {
        isResting = true;
        transform.up = targetNormal;
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);

        yield return new WaitForSeconds(Random.Range(minRestDuration, maxRestDuration));
        isResting = false;
    }
}




    // public float minRestDuration = 2f; // Minimum rest duration
    // public float maxRestDuration = 5f; // Maximum rest duration
    // public float takeOffTime = 0.5f;   // Time the fly takes off from the landing surface
    // public float detectionRange = 2.0f;// Range to detect landing surfaces
    // public float takeOffChance = 0.8f; // Chance to take off rather than keep resting
    // public float speed = 2.0f;         // Movement speed of the fly
    // public float rotationSpeed = 10f;  // Speed of rotation towards the new direction
    //
    // private bool isResting = false;    // Flag to check if the fly is resting
    // private Vector3 targetPosition;    // Target position for the fly to land on
    // private float timeSinceLastMove;
    //
    // private void Start()
    // {
    //     targetPosition = transform.position;  // Initialize target position to the current position of the fly
    //     timeSinceLastMove = takeOffTime;
    //     StartCoroutine(MoveToNewPosition());
    // }
    //
    // private void Update()
    // {
    //     if (!isResting)
    //     {
    //         // Move the fly towards the target position
    //         Vector3 direction = (targetPosition - transform.position).normalized;
    //         transform.position += direction * speed * Time.deltaTime;
    //         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
    //
    //         timeSinceLastMove += Time.deltaTime;
    //
    //         // Check if the fly has reached the target position
    //         if (Vector3.Distance(transform.position, targetPosition) < 0.1f || timeSinceLastMove > takeOffTime)
    //         {
    //             isResting = true;
    //             StartCoroutine(Rest());
    //         }
    //     }
    // }
    //
    // private IEnumerator MoveToNewPosition()
    // {
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(Random.Range(minRestDuration, maxRestDuration));
    //         if (isResting)
    //         {
    //             FindNewPosition();
    //             isResting = false;
    //             timeSinceLastMove = 0;
    //         }
    //     }
    // }
    //
    // private void FindNewPosition()
    // {
    //     MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
    //     if (currentRoom != null) 
    //     {
    //         Vector3 position, normal;
    //         //MRUK.SurfaceType surfaceTypes = MRUK.SurfaceType.FACING_UP | MRUK.SurfaceType.VERTICAL;
    //         float minDistanceToEdge = 0.1f;
    //         var labelFilter = LabelFilter.FromEnum(MRUKAnchor.SceneLabels.WALL_FACE | MRUKAnchor.SceneLabels.CEILING | MRUKAnchor.SceneLabels.FLOOR);
    //
    //         if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP | MRUK.SurfaceType.VERTICAL, minDistanceToEdge, labelFilter, out position, out normal)) {
    //             targetPosition = position;
    //             // Adjust fly rotation to land properly on the surface
    //             transform.up = normal;
    //         }
    //     }
    // }
    //
    // private IEnumerator Rest()
    // {
    //     // Adjust the fly rotation to make it look like it's resting on the surface
    //     transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
    //
    //     // Wait for rest time before taking off again
    //     yield return new WaitForSeconds(Random.Range(minRestDuration, maxRestDuration));
    //     isResting = false;
    // }
    //
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, detectionRange);
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(targetPosition, 0.1f);
    // }


