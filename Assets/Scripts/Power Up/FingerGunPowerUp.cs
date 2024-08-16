using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class FingerGunPowerUp : BasePowerUpBehavior
{
    [HideInInspector] public GameObject LeftFingerGun;
    [HideInInspector] public GameObject RightFingerGun;

    new void Start()
    {
        base.Start();
        LeftFingerGun = GameManager.Instance.LeftFingerGun;
        RightFingerGun = GameManager.Instance.RightFingerGun;
        if (LeftFingerGun != null)
        {
            LeftFingerGun.GetComponent<FingerGun>().ActiveHand = GameManager.Instance.LeftHand.GetComponent<OVRHand>();
        }
        if (RightFingerGun != null)
        {
            RightFingerGun.GetComponent<FingerGun>().ActiveHand = GameManager.Instance.RightHand.GetComponent<OVRHand>();
        }
    }

    new void Update()
    {
        base.Update();
    }

    public override void OnGrabbableSelect(PointerEvent arg0)
    {
        base.OnGrabbableSelect(arg0);

        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;

        if (handedness == Handedness.Right)
        {
            RightFingerGun.SetActive(true);
            LeftFingerGun.SetActive(false);
        }
        else
        {
            LeftFingerGun.SetActive(true);
            RightFingerGun.SetActive(false);
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