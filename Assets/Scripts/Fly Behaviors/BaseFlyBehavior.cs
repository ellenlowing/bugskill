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
    public MRUKAnchor.SceneLabels LandingSurface;
    public bool IsSlowed;
    [SerializeField] private List<MeshRenderer> normalEyeMesh;
    [SerializeField] private List<MeshRenderer> circularEyeMesh;

    private SettingSO settings;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 targetNormal;
    private float restTimer;
    private float slowdownTimer;
    private bool needNewTarget;

    public void Start()
    {
        EnterState(FlyState.FLYING);
        settings = GameManager.Instance.settings;
        CurrentFlyStat = settings.flyIntelLevels[settings.waveIndex];
        FlyingSpeed = Random.Range(CurrentFlyStat.minSpeed, CurrentFlyStat.maxSpeed);
        RestDuration = Random.Range(CurrentFlyStat.minRestDuration, CurrentFlyStat.maxRestDuration);
        TakeoffChance = settings.TakeoffChances[settings.waveIndex];
        needNewTarget = true;
        IsSlowed = false;
        rb = GetComponent<Rigidbody>();
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

    public virtual void EnterIdleState() { }
    public virtual void UpdateIdleState() { }
    public virtual void EnterFlyingState() { }
    public virtual void UpdateFlyingState()
    {
        int tries = 0;
        while (needNewTarget)
        {
            FindNewPosition();

            tries++;
            if (tries > 10)
            {
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
        transform.up = targetNormal;
        transform.position = targetPosition + 0.01f * targetNormal;
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
        rb.isKinematic = false;
        rb.useGravity = true;
    }
    public virtual void UpdateDyingState() { }
    public virtual void EnterDeadState()
    {
        settings.Cash += (int)SCOREFACTOR.SPRAY;
        int totalCash = (int)SCOREFACTOR.SPRAY;
        UIManager.Instance.IncrementKill(transform.position, totalCash);
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

        // If there's no obstacle in the fly's path
        if (!Physics.Raycast(transform.position, direction, Vector3.Distance(transform.position, position)))
        {
            targetPosition = position;
            targetNormal = normal;
            needNewTarget = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (CurrentState)
        {
            case FlyState.DYING:
                if (other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.LandingLayerName))
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
}
