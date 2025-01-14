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
    private Vector3 targetPosition;
    private Vector3 targetNormal;
    private float restTimer;
    private float slowdownTimer;
    private bool needNewTarget;
    public MeshRenderer[] _renderers;

    public void Start()
    {
        EnterState(FlyState.FLYING);
        settings = GameManager.Instance.settings;
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
        rb = GetComponent<Rigidbody>();
        _renderers = GetComponentsInChildren<MeshRenderer>();
        SetDepthBias(settings.EnvironmentDepthBias);
    }

    public void Update()
    {
        UpdateState();

        if (IsSlowed && (Time.time - slowdownTimer >= CurrentFlyStat.flySlowDownTime))
        {
            IsSlowed = false;
            slowdownTimer = -1;
            FlyingSpeed = Random.Range(CurrentFlyStat.minSpeed, CurrentFlyStat.maxSpeed);
            if (TryGetComponent<FlyAudioSource>(out var src))
            {
                src.MoveClip();
            };
            ChangeEyeType(false);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            EnterState(FlyState.DYING);
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
    }
    public virtual void UpdateIdleState() { }
    public virtual void EnterFlyingState()
    {
        animator.speed = 1;
    }
    public virtual void UpdateFlyingState()
    {
        int tries = 0;
        while (needNewTarget)
        {
            FindNewPosition();

            tries++;
            if (tries > 30)
            {
                Debug.Log(gameObject.name + " couldn't find a new position");
                break;
            }
        }

        if (!needNewTarget)
        {
            MoveTowardsTargetPosition();
            if (Vector3.Distance(transform.position, targetPosition) < 0.08f)
            {
                if (Random.Range(0, 1f) < TakeoffChance)
                {
                    needNewTarget = true;
                }
                else
                {
                    EnterState(FlyState.RESTING);
                }
            }
        }
    }

    public virtual void EnterRestingState()
    {
        restTimer = Time.time;
        animator.speed = 0;
        transform.up = targetNormal;
        transform.position = targetPosition;
        transform.rotation = transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
    }

    public virtual void UpdateRestingState()
    {
        if (Time.time - restTimer >= RestDuration)
        {
            EnterState(FlyState.FLYING);
        }
    }

    public virtual void EnterDyingState()
    {
        animator.speed = 0;
        rb.isKinematic = false;
        rb.useGravity = true;
    }
    public virtual void UpdateDyingState() { }
    public virtual void EnterDeadState()
    {
        animator.speed = 0;
        UIManager.Instance.IncrementKill(transform.position, (int)SCOREFACTOR.SPRAY);
        Destroy(gameObject);
    }
    public virtual void UpdateDeadState() { }

    public void SlowDown()
    {
        IsSlowed = true;
        slowdownTimer = Time.time;
        FlyingSpeed = CurrentFlyStat.flySlowDownSpeed;
        if (TryGetComponent<FlyAudioSource>(out var src))
        {
            src.DizzyClip();
        };
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

    private void FindNewPosition()
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
                CheckValidPosition(position, normal);
            }
        }
    }

    private void CheckValidPosition(Vector3 position, Vector3 normal)
    {
        Vector3 direction = (position - transform.position).normalized;
        bool isPathClear = !Physics.Raycast(transform.position, direction, Vector3.Distance(transform.position, position)); // If there's no obstacle in the fly's path
        Collider[] nearbyFlies = Physics.OverlapSphere(position, MinNearbyFlyDistance, GameManager.Instance.FlyLayerMask);
        bool isFlyNearby = nearbyFlies.Length > 0; // If there are flies nearby

        if (isPathClear && !isFlyNearby)
        {
            targetPosition = position;
            targetNormal = normal;
            needNewTarget = false;
        }
    }

    public void SetDepthBias(float value)
    {
        foreach (var renderer in _renderers)
        {
            Material[] materials = renderer.materials;
            foreach (var material in materials)
            {
                material.SetFloat("_EnvironmentDepthBias", value);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (CurrentState)
        {
            case FlyState.DYING:
                if (GameManager.Instance.IsOnAnyLandingLayer(other.gameObject))
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
        Gizmos.DrawSphere(transform.position, MinNearbyFlyDistance);
    }
}
