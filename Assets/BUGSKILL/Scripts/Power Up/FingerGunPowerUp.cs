using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class FingerGunPowerUp : BasePowerUpBehavior
{
    [HideInInspector] public GameObject LeftFingerGun;
    [HideInInspector] public GameObject RightFingerGun;

    [Header("Status")]
    public GameObject StatusIndicator;
    public Color FingerGunActiveColor;
    public Color FingerGunIdleColor;
    public Color FingerGunFiringColor;

    private Hand _activeHand;
    private bool _isActiveHandLeft;

    new void Start()
    {
        base.Start();
        LeftFingerGun = GameManager.Instance.LeftFingerGun;
        RightFingerGun = GameManager.Instance.RightFingerGun;
    }

    new void Update()
    {
        base.Update();

        if (_isEquipped)
        {
            if (_activeHand != null)
            {
                MoveToWrist(_activeHand);
            }
        }
    }

    public override void OnGrabbableSelect(PointerEvent arg0)
    {
        base.OnGrabbableSelect(arg0);
        // transform.parent = null;

        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;

        if (handedness == Handedness.Right)
        {
            RightFingerGun.SetActive(true);
            LeftFingerGun.SetActive(false);
            RightFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(true);
            LeftFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
            RightFingerGun.GetComponent<FingerGun>().StatusIndicator = StatusIndicator;
            _activeHand = GameManager.Instance.RightHand;
            _isActiveHandLeft = false;
        }
        else
        {
            LeftFingerGun.SetActive(true);
            RightFingerGun.SetActive(false);
            LeftFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(true);
            RightFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
            LeftFingerGun.GetComponent<FingerGun>().StatusIndicator = StatusIndicator;
            _activeHand = GameManager.Instance.LeftHand;
            _isActiveHandLeft = true;
        }

        // TODO check why Base power up behavior's ongrabbale select is not called?

    }

    public override void OnGrabbableUnselect(PointerEvent arg0)
    {
        base.OnGrabbableUnselect(arg0);

        if (StoreManager.Instance.IsStoreActive)
        {
            _activeHand = null;
            LeftFingerGun.SetActive(false);
            RightFingerGun.SetActive(false);
            LeftFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
            RightFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
        }
    }

    public void MoveToWrist(Hand hand)
    {
        bool handPoseAvailable = hand.GetRootPose(out Pose pose);
        if (handPoseAvailable)
        {
            transform.rotation = Quaternion.LookRotation(pose.forward, -pose.right);
            if (_isActiveHandLeft)
            {
                transform.rotation = Quaternion.LookRotation(-pose.forward, pose.right);
            }
            transform.position = pose.position - transform.up * 0.1f;
        }
        // else
        // {
        //     Debug.Log("Hand pose not available");
        // }
    }

    public override void Dissolve()
    {
        LeftFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
        RightFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
        LeftFingerGun.SetActive(false);
        RightFingerGun.SetActive(false);
        base.Dissolve();
    }
}