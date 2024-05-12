using System.Collections;
using System.Collections.Generic;
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

    public Transform FroggyParentTransform;
    public Transform FrogTongueTransform;
    public OVRHand FroggyActiveHand = null;
    public bool FroggyActive = false;
    public float GrabSpeed = 2;
    public float ReturnSpeed = 1;
    public float MaxScaleY = 5f;
    public OVRHand LeftHand;
    public OVRHand RightHand;
    public OVRSkeleton LeftHandSkeleton;
    public OVRSkeleton RightHandSkeleton;
    public float MinDistanceToActivateFroggy = 0.08f;
    public Vector3 FroggyPositionOffset;
    public Vector3 FroggyRotationOffset;
    public Transform TongueTipObjectTransform;
    public Transform TongueTipTargetTransform;

    private HandData _leftHandData;
    private HandData _rightHandData;
    private Vector3 _originalFrogTongueScale;

    void Start()
    {
        _originalFrogTongueScale = FrogTongueTransform.localScale;
        _leftHandData = new HandData(LeftHand, LeftHandSkeleton);
        _rightHandData = new HandData(RightHand, RightHandSkeleton);
        FrogTongueTransform.localScale = new Vector3(1, 0.1f, 1);
    }

    void Update()
    {
        UpdateHandData(_leftHandData);
        UpdateHandData(_rightHandData);

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
            handData.IsMiddleFingerPinching = handData.Hand.GetFingerIsPinching(OVRHand.HandFinger.Middle);

            var distanceFromMiddleFingerTipToThumbTip = Vector3.Distance(handData.MiddleFingerTipTransform.position, handData.ThumbTipTransform.position);
            if (FroggyActiveHand == null && distanceFromMiddleFingerTipToThumbTip < MinDistanceToActivateFroggy)
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
            else if (FroggyActiveHand == handData.Hand && distanceFromMiddleFingerTipToThumbTip >= MinDistanceToActivateFroggy)
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
            StartCoroutine(AnimateFrogTongueScale(_originalFrogTongueScale, new Vector3(1f, MaxScaleY, 1f), GrabSpeed, ReturnSpeed));
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

}
