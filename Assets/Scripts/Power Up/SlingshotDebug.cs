using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

public class SlingshotDebug : MonoBehaviour
{
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
    public SlingshotState CurrentState = SlingshotState.Idle;

    void Start()
    {
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
        if (Input.GetMouseButton(0))
        {
            SetState(SlingshotState.PreLaunch);
        }
    }

    private void EnterPreLaunchState()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        _pinchDownPosition = mouseWorldPosition;
        _newTempBall = Instantiate(TempBall, _pinchDownPosition, Quaternion.identity);
        _tempBallRigidbody = _newTempBall.GetComponent<Rigidbody>();
        _tempBallRigidbody.isKinematic = true;

        Debug.Log("Pinch Down Position: " + _pinchDownPosition);
    }

    private void UpdatePreLaunchState()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            _newTempBall.transform.position = mouseWorldPosition;
            _lastIndexFingerTipPose.position = mouseWorldPosition;

            // Draw trajectory
            Vector3 force = (_pinchDownPosition - mouseWorldPosition) * LaunchForce;
            Vector3[] trajectoryPoints = GetTrajectoryPoints(force, _tempBallRigidbody, mouseWorldPosition); // should last param be indexFingerTipPose.position?
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

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Step 2: Set the z-coordinate of the mouse to the distance from the camera to the world
        mouseScreenPosition.z = Camera.main.nearClipPlane;

        // Step 3: Convert the screen position to world position
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        return mouseWorldPosition;
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
                Debug.Log("Hit: " + hit.point);
                break;
            }
            trajectoryPoints.Add(launchPosition - movementVector);
        }

        return trajectoryPoints.ToArray();
    }
}