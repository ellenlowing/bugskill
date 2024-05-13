using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.XR;

public class FroggyController : MonoBehaviour
{
    public class HandData
    {
        public OVRHand Hand;
        public OVRSkeleton Skeleton;
        public Transform ThumbTipTransform;
        public Transform MiddleFingerTipTransform;
        public bool IsMiddleFingerPinching = false;

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
                else if (b.Id == OVRSkeleton.BoneId.Hand_MiddleTip)
                {
                    MiddleFingerTipTransform = b.Transform;
                }
            }
        }
    }

    public static FroggyController Instance;

    public Transform FroggyParentTransform;
    public Transform FrogTongueTransform;
    public OVRHand FroggyActiveHand = null;
    public bool FroggyActive = false;
    public float GrabSpeed = 2;
    public float ReturnSpeed = 1;
    public float MinScaleY = 0.1f;
    public float MaxScaleY = 5f;
    public OVRHand LeftHand;
    public OVRHand RightHand;
    public OVRSkeleton LeftHandSkeleton;
    public OVRSkeleton RightHandSkeleton;
    public float MaxDistanceToActivateFroggy = 0.1f;
    public float MaxDistanceToGrab = 0.02f;
    public Vector3 FroggyPositionOffset;
    public Vector3 FroggyRotationOffset;
    public Transform TongueTipObjectTransform;
    public Transform TongueTipTargetTransform;
    public float SphereCastRadius = 0.2f;
    public float SphereCastDistance = 4f;
    public LayerMask FlyLayerMask;

    private HandData _leftHandData;
    private HandData _rightHandData;
    private Vector3 _originalFrogTongueScale;
    public Collider _hitFlyCollider = null;

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

    void Start()
    {
        FrogTongueTransform.localScale = new Vector3(1, 0.1f, 1);
        _originalFrogTongueScale = FrogTongueTransform.localScale;
        _leftHandData = new HandData(LeftHand, LeftHandSkeleton);
        _rightHandData = new HandData(RightHand, RightHandSkeleton);
    }

    void Update()
    {
        // Get the hand data
        // UpdateHandData(_leftHandData);
        UpdateHandData(_rightHandData);

        // Check if there's a fly in front
        if (Physics.SphereCast(FroggyParentTransform.position, SphereCastRadius, -FroggyParentTransform.right, out RaycastHit hit, SphereCastDistance, FlyLayerMask))
        {
            if (_hitFlyCollider == null || hit.collider != _hitFlyCollider)
            {
                _hitFlyCollider = hit.collider;
                Debug.Log("Fly in front!");
            }
        }
        else
        {
            _hitFlyCollider = null;
        }

        // Sync tongue tip gameobject with extended tongue
        TongueTipObjectTransform.position = TongueTipTargetTransform.position;
        TongueTipObjectTransform.rotation = TongueTipTargetTransform.rotation;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerPress();
        }
    }

    void UpdateHandData(HandData handData)
    {
        if (handData.Hand.IsTracked)
        {
            var distanceFromMiddleFingerTipToThumbTip = Vector3.Distance(handData.MiddleFingerTipTransform.position, handData.ThumbTipTransform.position);
            handData.IsMiddleFingerPinching = distanceFromMiddleFingerTipToThumbTip < MaxDistanceToGrab;

            if (FroggyActiveHand == null && distanceFromMiddleFingerTipToThumbTip < MaxDistanceToActivateFroggy)
            {
                FroggyActiveHand = handData.Hand;
                ShowAllRenderers();
                FroggyParentTransform.parent = FroggyActiveHand.transform;
                FroggyParentTransform.localPosition = FroggyPositionOffset;
                FroggyParentTransform.localEulerAngles = FroggyRotationOffset;

                if (FroggyActiveHand == LeftHand)
                {
                    FroggyParentTransform.localPosition = -FroggyPositionOffset;
                    FroggyParentTransform.localEulerAngles = FroggyRotationOffset + new Vector3(0, 0, 180);
                }
            }
            else if (FroggyActiveHand == handData.Hand && !FroggyActive && distanceFromMiddleFingerTipToThumbTip >= MaxDistanceToActivateFroggy)
            {
                FroggyActiveHand = null;
                FroggyParentTransform.parent = null;
                HideAllRenderers();
            }

            if (handData.IsMiddleFingerPinching)
            {
                TriggerPress();
            }
        }
    }

    void ShowAllRenderers()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
    }

    void HideAllRenderers()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }

    void TriggerPress()
    {
        if (FroggyActiveHand != null && !FroggyActive)
        {
            if (_hitFlyCollider != null)
            {
                float distance = Vector3.Distance(_hitFlyCollider.transform.position, TongueTipObjectTransform.position);
                float scaleY = map(distance, 0, 2f, MinScaleY, MaxScaleY);
                Debug.Log("Hitting " + _hitFlyCollider.name + ", extending tongue by " + scaleY);
                StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, scaleY, 1f), GrabSpeed, ReturnSpeed));
            }
            else
            {
                float scaleY = map(0.2f, 0, 1, MinScaleY, MaxScaleY);
                StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, scaleY, 1f), GrabSpeed, ReturnSpeed));
            }
        }
    }

    IEnumerator AnimateFrogTongueScale(Vector3 inScale, Vector3 outScale, float grabSpeed, float returnSpeed)
    {
        // PlaySound("Reload");
        float t = 0;
        FroggyActive = true;
        while (t <= 1)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(inScale, outScale, t);
            t += Time.deltaTime * grabSpeed;
            yield return null;
        }

        // if (returnSpeed == FastClawReturnAnimationSpeed)
        // {
        // PlaySound("GrabSuccess");
        // }
        // else
        // {
        // PlaySound("GrabFailure");
        // }
        // yield return new WaitForSeconds(0.7f);
        // StopSound();

        t = 0;
        while (Vector3.Distance(FrogTongueTransform.localScale, inScale) > 0.001f)
        {
            FrogTongueTransform.localScale = Vector3.Lerp(outScale, inScale, t);
            t += Time.deltaTime * returnSpeed;
            yield return null;
        }
        FroggyActive = false;
        FrogTongueTransform.localScale = inScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Fly")
        {
            Debug.Log("Fly caught!");
            other.GetComponentInParent<FlyMovement>().isCaught = true;
            other.GetComponentInChildren<Animator>().speed = 0;
            other.transform.parent = TongueTipObjectTransform;
            other.transform.position = TongueTipObjectTransform.GetComponent<Collider>().ClosestPoint(other.transform.position);
            other.transform.localScale = other.transform.localScale * 0.5f;

            Destroy(other.gameObject, 1f);
        }
    }

    private float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(FroggyParentTransform.position + SphereCastDistance * -FroggyParentTransform.right, SphereCastRadius);
        Gizmos.DrawRay(FroggyParentTransform.position, -FroggyParentTransform.right * SphereCastDistance);
    }
}
