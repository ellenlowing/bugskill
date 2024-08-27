using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

public enum SlingshotState
{
    Idle,
    PreLaunch,
    InLaunch
}

public class Slingshot : MonoBehaviour
{
    public GameObject TempBall;
    public float LaunchForce = 2f;

    private OVRHand ovrHand;
    private Hand hand;
    private GameObject _newTempBall;
    private Rigidbody _tempBallRigidbody;
    private Vector3 _pinchDownPosition;
    private Vector3 _pinchUpPosition;
    private Pose _lastIndexFingerTipPose;
    private HandJointId _indexFingerJoint = HandJointId.HandIndexTip;
    public SlingshotState CurrentState = SlingshotState.Idle;

    void Start()
    {
        ovrHand = GameManager.Instance.RightOVRHand;
        hand = GameManager.Instance.RightHand;
    }

    void Update()
    {
        switch (CurrentState)
        {
            case SlingshotState.Idle:
                UpdateIdleState();
                break;

            case SlingshotState.PreLaunch:
                UpdatePreLaunchState();
                break;

            case SlingshotState.InLaunch:
                UpdateInLaunchState();
                break;
        }
    }

    public void SetState(SlingshotState state)
    {
        if (state != CurrentState)
        {
            switch (state)
            {
                case SlingshotState.Idle:
                    EnterIdleState();
                    break;

                case SlingshotState.PreLaunch:
                    EnterPreLaunchState();
                    break;

                case SlingshotState.InLaunch:
                    EnterInLaunchState();
                    break;
            }

            CurrentState = state;
        }
    }

    private void EnterIdleState()
    {
    }

    private void UpdateIdleState()
    {
        if (ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            SetState(SlingshotState.PreLaunch);
        }
    }

    private void EnterPreLaunchState()
    {
        hand.GetJointPose(_indexFingerJoint, out Pose indexFingerTipPose);
        _pinchDownPosition = new Vector3(indexFingerTipPose.position.x, indexFingerTipPose.position.y, indexFingerTipPose.position.z);
        _newTempBall = Instantiate(TempBall, _pinchDownPosition, Quaternion.identity);
        _tempBallRigidbody = _newTempBall.GetComponent<Rigidbody>();
        _tempBallRigidbody.isKinematic = true;

        Debug.Log("Pinch Down Position: " + _pinchDownPosition);
    }

    private void UpdatePreLaunchState()
    {
        if (ovrHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            hand.GetJointPose(_indexFingerJoint, out Pose indexFingerTipPose);
            _newTempBall.transform.position = indexFingerTipPose.position;
            _lastIndexFingerTipPose = indexFingerTipPose;
        }
        else
        {
            SetState(SlingshotState.InLaunch);
        }
    }

    private void EnterInLaunchState()
    {
        _pinchUpPosition = _lastIndexFingerTipPose.position;

        // Calculate launch parameters
        Vector3 force = _pinchDownPosition - _pinchUpPosition;
        _tempBallRigidbody.isKinematic = false;
        _tempBallRigidbody.AddForce(new Vector3(force.x, force.y, force.z) * LaunchForce);

        Debug.Log("Pinch Up Position: " + _pinchUpPosition);
    }

    private void UpdateInLaunchState()
    {
        SetState(SlingshotState.Idle);
    }
}