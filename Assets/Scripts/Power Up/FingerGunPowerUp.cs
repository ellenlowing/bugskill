using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class FingerGunPowerUp : BasePowerUpBehavior
{
    [HideInInspector] public GameObject LeftFingerGun;
    [HideInInspector] public GameObject RightFingerGun;

    private Hand _activeHand;

    new void Start()
    {
        base.Start();
        LeftFingerGun = GameManager.Instance.LeftFingerGun;
        RightFingerGun = GameManager.Instance.RightFingerGun;
        if (LeftFingerGun != null)
        {
            LeftFingerGun.GetComponent<FingerGun>().ActiveOVRHand = GameManager.Instance.LeftOVRHand;
        }
        if (RightFingerGun != null)
        {
            RightFingerGun.GetComponent<FingerGun>().ActiveOVRHand = GameManager.Instance.RightOVRHand;
        }
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
        transform.parent = null;

        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;

        if (handedness == Handedness.Right)
        {
            RightFingerGun.SetActive(true);
            LeftFingerGun.SetActive(false);
            // MoveToWrist(GameManager.Instance.RightHand);
            _activeHand = GameManager.Instance.RightHand;
        }
        else
        {
            LeftFingerGun.SetActive(true);
            RightFingerGun.SetActive(false);
            // MoveToWrist(GameManager.Instance.LeftHand);
            _activeHand = GameManager.Instance.LeftHand;
        }

    }

    public void MoveToWrist(Hand hand)
    {
        bool handPoseAvailable = hand.GetRootPose(out Pose pose);
        if (handPoseAvailable)
        {
            transform.position = pose.position;
            transform.rotation = Quaternion.LookRotation(pose.right, pose.up);
        }
        else
        {
            Debug.Log("Hand pose not available");
        }
    }

    public override void Dissolve()
    {
        LeftFingerGun.SetActive(false);
        RightFingerGun.SetActive(false);
        LeftFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
        RightFingerGun.GetComponent<FingerGun>().Crosshair.SetActive(false);
        base.Dissolve();
    }
}