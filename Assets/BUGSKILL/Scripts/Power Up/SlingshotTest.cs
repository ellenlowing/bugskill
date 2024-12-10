using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;


public class SlingshotTest : MonoBehaviour
{
    public enum SlingshotState
    {
        Idle,
        PreLaunch,
        InLaunch
    }

    public SlingshotState CurrentState = SlingshotState.Idle;
    public GameObject TempBall;
    public float LaunchForce = 2f;
    public Raycaster raycaster;

    private OVRHand ovrHand;
    private Hand hand;
    private GameObject _newTempBall;
    private Rigidbody _tempBallRigidbody;
    private Vector3 _pinchDownPosition;
    private Vector3 _pinchUpPosition;
    private Pose _lastIndexFingerTipPose;
    private HandJointId _indexFingerJoint = HandJointId.HandIndexTip;

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

            // Draw trajectory
            Vector3 force = (_pinchDownPosition - indexFingerTipPose.position) * LaunchForce;
            Vector3[] trajectoryPoints = GetTrajectoryPoints(force, _tempBallRigidbody, indexFingerTipPose.position); // should last param be indexFingerTipPose.position?
            raycaster.DrawProjectile(trajectoryPoints);
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

    Vector3[] GetTrajectoryPoints(Vector3 force, Rigidbody rb, Vector3 launchPosition)
    {
        List<Vector3> trajectoryPoints = new List<Vector3>();
        Vector3 velocity = (force / rb.mass) * Time.fixedDeltaTime;
        float flightDuration = 2 * velocity.y / Physics.gravity.y;
        float lineSegmentCount = 30;
        float stepTime = flightDuration / lineSegmentCount;

        for (int i = 0; i < lineSegmentCount; i++)
        {
            float stepTimePassed = stepTime * i;
            Vector3 movementVector = velocity * stepTimePassed - 0.5f * Physics.gravity * stepTimePassed * stepTimePassed;
            RaycastHit hit;
            if (Physics.Raycast(launchPosition, -movementVector, out hit, movementVector.magnitude))
            {
                break;
            }
            trajectoryPoints.Add(launchPosition - movementVector);
        }

        return trajectoryPoints.ToArray();
    }
}