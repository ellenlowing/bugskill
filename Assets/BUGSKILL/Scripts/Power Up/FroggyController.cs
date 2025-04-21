using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

public class FroggyController : BasePowerUpBehavior
{
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
    public Vector3 FroggyPositionOffset;
    public Vector3 FroggyRotationOffset;

    [Header("SphereCast")]
    public float SphereCastRadius = 0.2f;
    public float SphereCastDistance = 4f;
    private RaycastHit[] _previousHits;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip munchClip;
    public AudioClip croakClip;
    public AudioClip slurpClip;

    private Vector3 _originalFrogTongueScale;
    private bool _initialized = false;

    new void Start()
    {
        base.Start();
        Initialize();
    }

    new void Update()
    {
        base.Update();

        // Sync tongue tip gameobject with extended tongue
        TongueTipObjectTransform.position = TongueTipTargetTransform.position;
        TongueTipObjectTransform.rotation = TongueTipTargetTransform.rotation;

#if UNITY_EDITOR
        if (Keyboard.current[Key.F].wasPressedThisFrame)
        {
            TriggerPress();
        }
#endif
    }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState();

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, SphereCastRadius, -transform.right, SphereCastDistance, GameManager.Instance.FlyLayerMask);

        // Disable outline for previous hits if they are not in the current hits
        foreach (var hit in _previousHits)
        {
            if (!hits.Contains(hit))
            {
                if (hit.collider != null)
                {
                    var outline = hit.collider.GetComponentInChildren<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }
        }

        // Enable outline for all current hits
        foreach (var hit in hits)
        {
            var outline = hit.collider.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
            }
        }

        _previousHits = hits;

        // Check if active hand is pinching
        if (ActiveOVRHand != null && ActiveOVRHand.IsTracked)
        {
            bool isPinching = ActiveOVRHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            if (isPinching)
            {
                TriggerPress();
            }
        }
    }

    public void Initialize()
    {
        if (!_initialized)
        {
            FrogTongueTransform.localScale = new Vector3(1, 0.1f, 1);
            _originalFrogTongueScale = FrogTongueTransform.localScale;
            audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.clip = croakClip;
            audioSource.spatialBlend = 1f;
            audioSource.loop = false;
            audioSource.spatialize = true;
            audioSource.playOnAwake = false;
            _initialized = true;
            _previousHits = new RaycastHit[0];
        }
    }

    void TriggerPress()
    {
        if (!FroggyActive)
        {
            if (_previousHits.Length > 0)
            {
                float distance = Vector3.Distance(_previousHits[0].transform.position, TongueTipObjectTransform.position);
                float scaleY = Mathf.Clamp(map(distance, 0, SphereCastDistance, MinScaleY, MaxScaleY), MinScaleY, MaxScaleY);
                StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, scaleY, 1f), GrabSpeed, ReturnSpeed));
            }
            else
            {
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

        t = 0;
        while (Vector3.Distance(FrogTongueTransform.localScale, inScale) > 0.001f)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(outScale, inScale, t);
            t += Time.deltaTime * returnSpeed;
            yield return null;
        }

        FrogTongueTransform.localScale = inScale;
        UsePowerCapacity();
        FroggyActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Fly")
        {
            BaseFlyBehavior fly = other.GetComponent<BaseFlyBehavior>();
            fly.Outline.enabled = false;
            other.GetComponentInChildren<Animator>().speed = 0;
            other.transform.parent = TongueTipObjectTransform;
            other.transform.position = GetRandomPointWithinBounds(TongueTipObjectTransform.gameObject);
            other.transform.localScale = other.transform.localScale * 0.5f;

            UIManager.Instance.IncrementKill(other.transform.position, (int)SCOREFACTOR.FROG);
            fly.Kill();
        }
    }

    public override void ResetPowerUp()
    {
        base.ResetPowerUp();
        FrogTongueTransform.localScale = _originalFrogTongueScale;
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
