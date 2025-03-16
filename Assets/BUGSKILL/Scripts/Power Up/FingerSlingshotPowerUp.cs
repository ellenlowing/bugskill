using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class FingerSlingshotPowerUp : BasePowerUpBehavior
{
    [HideInInspector] public GameObject LeftFingerSlingshot;
    [HideInInspector] public GameObject RightFingerSlingshot;

    [Header("Status")]
    public GameObject StatusIndicator;
    public Color FingerSlingshotActiveColor;
    public Color FingerSlingshotIdleColor;
    public Color FingerSlingshotFiringColor;

    private Hand _activeHand;
    private bool _isActiveHandLeft;

    new void Start()
    {
        base.Start();
        LeftFingerSlingshot = GameManager.Instance.LeftFingerSlingshot;
        RightFingerSlingshot = GameManager.Instance.RightFingerSlingshot;
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
        if (_isEquipped)
        {
            return;
        }

        base.OnGrabbableSelect(arg0);
        // transform.parent = null;

        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;

        if (handedness == Handedness.Right)
        {
            RightFingerSlingshot.SetActive(true);
            LeftFingerSlingshot.SetActive(false);
            RightFingerSlingshot.GetComponent<Slingshot>().StatusIndicator = StatusIndicator;
            RightFingerSlingshot.GetComponent<Slingshot>().CorePowerUp = this;
            _activeHand = GameManager.Instance.RightHand;
            _isActiveHandLeft = false;
        }
        else
        {
            LeftFingerSlingshot.SetActive(true);
            RightFingerSlingshot.SetActive(false);
            LeftFingerSlingshot.GetComponent<Slingshot>().StatusIndicator = StatusIndicator;
            LeftFingerSlingshot.GetComponent<Slingshot>().CorePowerUp = this;
            _activeHand = GameManager.Instance.LeftHand;
            _isActiveHandLeft = true;
        }
    }

    public override void OnGrabbableUnselect(PointerEvent arg0)
    {
        base.OnGrabbableUnselect(arg0);

        if (!IsSold && StoreManager.Instance.IsStoreActive)
        {
            _activeHand = null;
            LeftFingerSlingshot.SetActive(false);
            RightFingerSlingshot.SetActive(false);
        }
    }

    public void MoveToWrist(Hand hand)
    {
        bool handPoseAvailable = hand.GetRootPose(out Pose pose);
        if (handPoseAvailable)
        {
            transform.rotation = Quaternion.LookRotation(-pose.up, pose.forward);
            transform.position = pose.position - transform.up * 0.1f;
        }
    }

    public override void Dissolve()
    {
        LeftFingerSlingshot.SetActive(false);
        RightFingerSlingshot.SetActive(false);
        SlingshotBall bomb = FindAnyObjectByType<SlingshotBall>();
        if (bomb != null)
        {
            bomb.RaycastVisualizer.HideProjectile();
            Destroy(bomb.gameObject);
        }
        base.Dissolve();
    }
}
