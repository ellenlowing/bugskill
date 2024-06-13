using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class FroggyController : BasePowerUpBehavior
{
    public class HandData
    {
        public OVRHand Hand;
        public OVRSkeleton Skeleton;
        public Transform ThumbTipTransform;
        public Transform IndexFingerTipTransform;
        public bool IsIndexFingerPinching = false;

        public HandData(OVRHand hand, OVRSkeleton skeleton)
        {
            Hand = hand;
            Skeleton = skeleton;
            foreach (var b in Skeleton.Bones)
            {
                if (b.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                {
                    ThumbTipTransform = b.Transform;
                }
                else if (b.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    IndexFingerTipTransform = b.Transform;
                }
            }
        }
    }

    public static FroggyController Instance;
    public bool IsActive = false;

    [Header("Settings Data")]
    [SerializeField] private SettingSO settings;

    [Header("Tongue")]
    public Transform FrogTongueTransform;
    public float GrabSpeed = 2;
    public float ReturnSpeed = 1;
    public float MinScaleY = 0.1f;
    public float MaxScaleY = 5f;
    public bool FroggyActive = false;
    public Transform TongueTipObjectTransform;
    public Transform TongueTipTargetTransform;
    public float MaxDistanceToActivateFroggy = 0.1f;
    public float MaxDistanceToGrab = 0.02f;

    [Header("Hands")]
    public OVRHand FroggyActiveHand = null;
    public GameObject LeftHand;
    public GameObject RightHand;
    public Vector3 FroggyPositionOffset;
    public Vector3 FroggyRotationOffset;

    [Header("SphereCast")]
    public float SphereCastRadius = 0.2f;
    public float SphereCastDistance = 4f;
    public LayerMask FlyLayerMask;
    private RaycastHit[] _previousHits;

    [Header("Cooldown")]
    public float CooldownTime = 3f;
    public float FroggyLastTriggeredTime = 0;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip munchClip;
    public AudioClip croakClip;
    public AudioClip slurpClip;

    private HandData _leftHandData;
    private HandData _rightHandData;
    private Vector3 _originalFrogTongueScale;
    private Collider _hitFlyCollider = null;
    private bool _successFly = false;
    private bool _initialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    new void Start()
    {
        base.Start();
        Initialize();
        PointableEventWrapper.WhenSelect.AddListener(OnGrabbableSelect);
        PointableEventWrapper.WhenUnselect.AddListener(OnGrabbableUnselect);
    }

    new void Update()
    {
        base.Update();
    }

    public override void EnterIdleState()
    {

    }

    public override void UpdateIdleState()
    {

    }

    public override void EnterInactiveState()
    {

    }

    public override void UpdateInactiveState()
    {

    }

    public override void EnterActiveState()
    {

    }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState();

        // Check if active hand is pinching
        if (FroggyActiveHand.IsTracked)
        {
            bool isPinching = FroggyActiveHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            if (isPinching)
            // && (Time.time - FroggyLastTriggeredTime) > CooldownTime)
            {
                TriggerPress();
                FroggyLastTriggeredTime = Time.time;
            }
        }

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, SphereCastRadius, -transform.right, SphereCastDistance, FlyLayerMask);

        // Disable outline for hits that are no longer in the current hits
        foreach (var hit in _previousHits)
        {
            if (!hits.Contains(hit))
            {
                if (hit.collider != null)
                {
                    hit.collider.GetComponentInChildren<Outline>().enabled = false;
                }
            }
        }

        // Enable outline for all current hits
        foreach (var hit in hits)
        {
            hit.collider.GetComponentInChildren<Outline>().enabled = true;
        }

        _previousHits = hits;

        // Sync tongue tip gameobject with extended tongue
        TongueTipObjectTransform.position = TongueTipTargetTransform.position;
        TongueTipObjectTransform.rotation = TongueTipTargetTransform.rotation;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerPress();
        }
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Initialize()
    {
        if (!_initialized)
        {
            FrogTongueTransform.localScale = new Vector3(1, 0.1f, 1);
            _originalFrogTongueScale = FrogTongueTransform.localScale;
            _leftHandData = new HandData(LeftHand.GetComponent<OVRHand>(), LeftHand.GetComponent<OVRSkeleton>());
            _rightHandData = new HandData(RightHand.GetComponent<OVRHand>(), RightHand.GetComponent<OVRSkeleton>());
            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = croakClip;
            audioSource.spatialBlend = 1f;
            audioSource.loop = false;
            audioSource.spatialize = true;
            audioSource.playOnAwake = false;
            var metaAudio = gameObject.AddComponent<MetaXRAudioSource>();
            metaAudio.EnableSpatialization = true;
            // HideAllRenderers();
            _initialized = true;
            _previousHits = new RaycastHit[0];
        }
    }

    void ShowAllRenderers()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }

        audioSource.mute = false;
    }

    void HideAllRenderers()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        //stop croaking while not in hand
        audioSource.mute = true;
    }

    private void OnGrabbableSelect(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
        if (handedness == Handedness.Right)
        {
            FroggyActiveHand = RightHand.GetComponent<OVRHand>();
        }
        else
        {
            FroggyActiveHand = LeftHand.GetComponent<OVRHand>();
        }
        EnterState(PowerUpState.ACTIVE);
    }

    private void OnGrabbableUnselect(PointerEvent arg0)
    {
        FroggyActiveHand = null;
        EnterState(PowerUpState.IDLE);
    }


    void TriggerPress()
    {
        if (FroggyActiveHand != null && !FroggyActive)
        {
            if (_previousHits.Length > 0)
            {
                _successFly = true;
                float distance = Vector3.Distance(_previousHits[0].transform.position, TongueTipObjectTransform.position);
                float scaleY = Mathf.Clamp(map(distance, 0, SphereCastDistance, MinScaleY, MaxScaleY), MinScaleY, MaxScaleY);
                StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, scaleY, 1f), GrabSpeed, ReturnSpeed));
            }
            else
            {
                _successFly = false;
                float scaleY = map(0.15f, 0, 1, MinScaleY, MaxScaleY);
                StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, scaleY, 1f), GrabSpeed, ReturnSpeed));
            }
        }
    }

    IEnumerator AnimateFrogTongueScale(Vector3 inScale, Vector3 outScale, float grabSpeed, float returnSpeed)
    {
        //start slurping while lashing tongue
        audioSource.clip = slurpClip;
        audioSource.loop = false;
        audioSource.Play();

        float t = 0;
        FroggyActive = true;
        while (t <= 1)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(inScale, outScale, t);
            t += Time.deltaTime * grabSpeed;
            yield return null;
        }
        yield return new WaitForSeconds(0.15f);

        Invoke(nameof(StartMunching), 0.2f);

        t = 0;
        while (Vector3.Distance(FrogTongueTransform.localScale, inScale) > 0.001f)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(outScale, inScale, t);
            t += Time.deltaTime * returnSpeed;
            yield return null;
        }
        FroggyActive = false;
        FrogTongueTransform.localScale = inScale;

        yield return new WaitForSeconds(0.2f);
    }

    private void StartMunching()
    {
        audioSource.clip = munchClip;
        audioSource.Play();
        _successFly = false;
    }

    private void RestartCroaking()
    {
        audioSource.clip = croakClip;
        // audioSource.loop = true;
        audioSource.Play();
    }

    private int totalCash = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Fly")
        {
            other.GetComponentInChildren<FlyMovement>().enabled = false;
            other.GetComponentInChildren<Outline>().enabled = false;
            other.GetComponentInChildren<Animator>().speed = 0;
            other.transform.parent = TongueTipObjectTransform;
            other.transform.position = GetRandomPointWithinBounds(TongueTipObjectTransform.gameObject);
            other.transform.localScale = other.transform.localScale * 0.5f;

            if (other.gameObject.transform.localScale == Vector3.one)
            {
                settings.Cash += (int)SCOREFACTOR.SLIM;
                totalCash += (int)SCOREFACTOR.SLIM;
            }
            else
            {
                settings.Cash += (int)SCOREFACTOR.FAT;
                totalCash += (int)SCOREFACTOR.FAT;
            }
            Destroy(other.gameObject, 0.5f);
            settings.Cash += (int)SCOREFACTOR.FROG;
            totalCash += (int)SCOREFACTOR.FROG;
            UIManager.Instance.IncrementKill(other.transform.position, totalCash);
            totalCash = 0;
        }
    }

    private float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    Vector3 GetRandomPointWithinBounds(GameObject obj)
    {
        // Get the Renderer component of the GameObject
        Renderer renderer = obj.GetComponent<Renderer>();

        Bounds bounds = renderer.bounds;

        // Generate random point within bounds
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(randomX, randomY, randomZ);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + SphereCastDistance * -transform.right, SphereCastRadius);
        Gizmos.DrawRay(transform.position, -transform.right * SphereCastDistance);
    }
}
