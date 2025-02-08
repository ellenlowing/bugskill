using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BaseFlyBehavior : MonoBehaviour
{
    public enum FlyType
    {
        REGULAR,
        TNT,
        INTRO
    }

    public enum FlyState
    {
        IDLE,
        FLYING,
        RESTING,
        DYING,
        DEAD
    }

    [Header("Fly Settings")]
    public FlyType Type;
    public FlyState CurrentState;
    public FlySO CurrentFlyStat;
    public float FlyingSpeed;
    public float RestDuration;
    public float TakeoffChance;
    public float MinNearbyFlyDistance;
    public MRUKAnchor.SceneLabels LandingSurface;
    public bool IsSlowed;
    public bool IsKilled;
    [SerializeField] private List<MeshRenderer> normalEyeMesh;
    [SerializeField] private List<MeshRenderer> circularEyeMesh;
    [SerializeField] private Animator animator;
    private SettingSO settings;
    private Rigidbody rb;
    private FlyAudioSource flyAudio;
    public Vector3 targetPosition;
    private Vector3 targetNormal;
    private float restTimer;
    private float slowdownTimer;
    private float evadeTimer;
    private bool needNewTarget;
    private int landingLayerMask;
    public MRUKAnchor currentLandingSurface;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        flyAudio = GetComponent<FlyAudioSource>();
        settings = GameManager.Instance.settings;
        landingLayerMask = GameManager.Instance.GetAnyLandingLayerMask();
        CurrentFlyStat = settings.flyIntelLevels[settings.waveIndex];
        FlyingSpeed = Random.Range(CurrentFlyStat.minSpeed, CurrentFlyStat.maxSpeed);
        RestDuration = Random.Range(CurrentFlyStat.minRestDuration, CurrentFlyStat.maxRestDuration);
        TakeoffChance = settings.TakeoffChances[settings.waveIndex];
        MinNearbyFlyDistance = settings.minNearbyFlyDistances[settings.waveIndex];
        if (Random.Range(0f, 1f) < TakeoffChance)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        needNewTarget = true;
        IsSlowed = false;
        EnterState(FlyState.FLYING);
    }

    public void Update()
    {
        UpdateState();

        if (IsSlowed && (Time.time - slowdownTimer >= CurrentFlyStat.flySlowDownTime))
        {
            IsSlowed = false;
            slowdownTimer = -1;
            FlyingSpeed = Random.Range(CurrentFlyStat.minSpeed, CurrentFlyStat.maxSpeed);
            ChangeEyeType(false);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
            if (currentRoom != null)
            {
                bool isTargetPositionInVolume = currentRoom.IsPositionInSceneVolume(targetPosition, out MRUKAnchor anchor, true);
                Debug.Log(gameObject.name + " is in volume?: " + isTargetPositionInVolume + " " + anchor);
            }
        }
#endif
    }

    public void EnterState(FlyState state)
    {
        switch (state)
        {
            case FlyState.IDLE:
                EnterIdleState();
                break;
            case FlyState.FLYING:
                EnterFlyingState();
                break;
            case FlyState.RESTING:
                EnterRestingState();
                break;
            case FlyState.DYING:
                EnterDyingState();
                break;
            case FlyState.DEAD:
                EnterDeadState();
                break;
        }

        CurrentState = state;
    }

    public void UpdateState()
    {
        switch (CurrentState)
        {
            case FlyState.IDLE:
                UpdateIdleState();
                break;
            case FlyState.FLYING:
                UpdateFlyingState();
                break;
            case FlyState.RESTING:
                UpdateRestingState();
                break;
            case FlyState.DYING:
                UpdateDyingState();
                break;
            case FlyState.DEAD:
                UpdateDeadState();
                break;
        }
    }

    public virtual void EnterIdleState()
    {
        animator.speed = 0;
        if (flyAudio != null)
        {
            flyAudio.Mute(true);
        }
    }
    public virtual void UpdateIdleState() { }
    public virtual void EnterFlyingState()
    {
        animator.speed = 1;
        flyAudio.Mute(false);
        flyAudio.MoveClip();
    }
    public virtual void UpdateFlyingState()
    {
        int tries = 0;
        while (needNewTarget)
        {
            FindNewPosition(true);

            tries++;
            if (tries > 30)
            {
                FindNewPosition(false);
                Debug.Log(gameObject.name + " couldn't find a new position");
                break;
            }
        }

        if (!needNewTarget)
        {
            MoveTowardsTargetPosition();
            if (Vector3.Distance(transform.position, targetPosition) < 0.025f) // !! REASON FOR GLITCHING WHEN EVADING
            {
                if (evadeTimer != -1 || Random.Range(0, 1f) >= TakeoffChance)
                {
                    EnterState(FlyState.RESTING);
                }
                else
                {
                    needNewTarget = true;
                }
            }
        }
    }

    public virtual void EnterRestingState()
    {
        restTimer = Time.time;
        evadeTimer = -1;
        animator.speed = 0;
        flyAudio.Mute(true);
        transform.up = targetNormal;
        transform.position = targetPosition;
        transform.rotation = transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
        Collider[] overlapColliders = Physics.OverlapSphere(targetPosition, 0.05f, landingLayerMask);
        if (overlapColliders.Length > 0)
        {
            currentLandingSurface = overlapColliders[0].GetComponentInParent<MRUKAnchor>();
        }
    }

    public virtual void UpdateRestingState()
    {
        if (Time.time - restTimer >= RestDuration)
        {
            EnterState(FlyState.FLYING);
        }

        Collider[] detectedHands = Physics.OverlapSphere(transform.position, CurrentFlyStat.detectionRadius, GameManager.Instance.HandsLayerMask);
        if (detectedHands.Length > 0)
        {
            // If the fly is near the hand, it should start counting down to evading
            if (evadeTimer == -1)
            {
                evadeTimer = Time.time;
            }
            else if (Time.time - evadeTimer >= CurrentFlyStat.evadeTime)
            {
                // Hop to nearby location
                Transform hand = detectedHands[0].transform;
                Vector3 awayFromHand = hand.position - transform.position; // Direction away from the hand
                awayFromHand = Vector3.ProjectOnPlane(awayFromHand, transform.up).normalized; // TODO 2nd parameter should be the landing surface's normal vector
                float randomAngle = Random.Range(-90f, 90f);
                Quaternion randomRotation = Quaternion.Euler(randomAngle * transform.up);
                awayFromHand = randomRotation * awayFromHand;
                Vector3 evadePosition = transform.position + awayFromHand * CurrentFlyStat.evadeDistance;
                Vector3 closestPosition = evadePosition;
                Vector3 closestNormal = transform.up;
                if (currentLandingSurface != null)
                {
                    // Improvement Notes
                    // 1. Make sure the evaded position is within the bounds of the surface that the fly initially landed on
                    // 2. Account for the landing surface's rotation
                    float closestDistance = currentLandingSurface.GetClosestSurfacePosition(evadePosition, out closestPosition, out closestNormal);
                }
                targetPosition = closestPosition;
                targetNormal = closestNormal;
                EnterState(FlyState.FLYING);
            }
        }
        else
        {
            evadeTimer = -1;
        }
    }

    public virtual void EnterDyingState()
    {
        animator.speed = 0;
        flyAudio.Mute(true);
        rb.isKinematic = false;
        rb.useGravity = true;
    }
    public virtual void UpdateDyingState() { }
    public virtual void EnterDeadState()
    {
        animator.speed = 0;
        flyAudio.Mute(true);
        UIManager.Instance.IncrementKill(transform.position, (int)SCOREFACTOR.SPRAY);
        Destroy(gameObject);
    }
    public virtual void UpdateDeadState() { }

    public void SlowDown()
    {
        IsSlowed = true;
        slowdownTimer = Time.time;
        FlyingSpeed = CurrentFlyStat.flySlowDownSpeed;
        flyAudio.DizzyClip();
        ChangeEyeType(true);
    }

    public void ChangeEyeType(bool circular)
    {
        circularEyeMesh[0].enabled = circular;
        circularEyeMesh[1].enabled = circular;
        normalEyeMesh[0].enabled = !circular;
        normalEyeMesh[1].enabled = !circular;

        circularEyeMesh[0].transform.gameObject.GetComponent<EyeSpiral>().canRotate = circular;
        circularEyeMesh[1].transform.gameObject.GetComponent<EyeSpiral>().canRotate = circular;
    }

    private void MoveTowardsTargetPosition()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * FlyingSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), CurrentFlyStat.rotationSpeed * Time.deltaTime);
    }

    private void FindNewPosition(bool needCheck)
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            // Anchor labels that fly can land on (chosen in editor) 
            var labelFilter = LabelFilter.Included(LandingSurface);

            // Generate random position on any surface that is not facing down 
            // + position is not too close to anchor's edge 
            if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP | MRUK.SurfaceType.VERTICAL, CurrentFlyStat.distanceToEdges, labelFilter, out Vector3 position, out Vector3 normal))
            {
                CheckValidPosition(position, normal, needCheck);
            }
        }
    }

    private void CheckValidPosition(Vector3 position, Vector3 normal, bool needCheck)
    {
        if (!needCheck)
        {
            AssignNewTarget(position, normal);
        }
        else
        {
            bool isFlyNearby = false;
            foreach (var fly in settings.flies)
            {
                if (Vector3.Distance(fly.GetComponent<BaseFlyBehavior>().targetPosition, position) < MinNearbyFlyDistance)
                {
                    isFlyNearby = true;
                    break;
                }
            }
            if (isFlyNearby) return;

            AssignNewTarget(position, normal);
        }
    }

    private void AssignNewTarget(Vector3 position, Vector3 normal)
    {
        targetPosition = position;
        targetNormal = normal;
        needNewTarget = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        bool isLanded = GameManager.Instance.IsOnAnyLandingLayer(other.gameObject);

        switch (CurrentState)
        {
            case FlyState.DYING:
                if (isLanded)
                {
                    foreach (ContactPoint contact in other.contacts)
                    {
                        if (Mathf.Abs(Vector3.Dot(contact.normal, Vector3.up)) >= 0.9f)
                        {
                            rb.isKinematic = true;
                            EnterState(FlyState.DEAD);
                        }
                    }
                }
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (CurrentFlyStat != null) Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
}
