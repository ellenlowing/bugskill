using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using Oculus.Interaction.Input;

public enum SlingshotState
{
    Idle,
    PreLaunch,
    InLaunch
}

public class SlingshotBall : MonoBehaviour
{
    public SlingshotState CurrentState;
    public PointableUnityEventWrapper BallPinchEvent;
    public float LaunchForce;
    public Raycaster RaycastVisualizer;
    public Raycaster IndexFingerLine;
    public Raycaster MiddleFingerLine;
    public float TimeToDestroyIdleBomb = 3f;

    [HideInInspector] public Hand PrimaryHand;

    private HandJointId _indexFingerJoint = HandJointId.HandIndexTip;
    private HandJointId _middleFingerJoint = HandJointId.HandMiddleTip;
    private Vector3 _pinchDownPosition;
    private Rigidbody _rb;
    private Pose _indexFingerTipPose;
    private Pose _middleFingerTipPose;
    private Vector3 _averageFingerTipPosition;
    private Vector3 _averageFingerTipEulerAngles;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        BallPinchEvent.WhenSelect.AddListener(OnSelect);
        BallPinchEvent.WhenUnselect.AddListener(OnUnselect);
        StartCoroutine(DestroyIdleBomb());
        SetState(SlingshotState.Idle);
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

    public void OnSelect(PointerEvent arg0)
    {
        if (CurrentState == SlingshotState.Idle)
        {
            SetState(SlingshotState.PreLaunch);
        }
    }

    public void OnUnselect(PointerEvent arg0)
    {
        if (CurrentState == SlingshotState.PreLaunch)
        {
            SetState(SlingshotState.InLaunch);
        }
    }

    private void EnterIdleState()
    {
    }

    private void UpdateIdleState()
    {
        UpdateFingerPose();

        transform.position = _averageFingerTipPosition;
        transform.rotation = Quaternion.LookRotation(_indexFingerTipPose.up);

        DrawSlingshotLines();
    }

    private void EnterPreLaunchState()
    {
    }

    private void UpdatePreLaunchState()
    {
        // Draw trajectory
        Vector3 force = (_pinchDownPosition - transform.position) * LaunchForce;
        Vector3[] trajectoryPoints = GetTrajectoryPoints(force, _rb, transform.position); // should last param be indexFingerTipPose.position?
        RaycastVisualizer.DrawProjectile(trajectoryPoints);

        // Update ball position
        UpdateFingerPose();
        DrawSlingshotLines();
    }

    private void EnterInLaunchState()
    {
        // Calculate launch parameters
        Vector3 force = (_pinchDownPosition - transform.position) * LaunchForce;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.AddForce(force);
    }

    private void UpdateInLaunchState()
    {
        // SetState(SlingshotState.Idle);
    }
    void UpdateFingerPose()
    {
        PrimaryHand.GetJointPose(_indexFingerJoint, out _indexFingerTipPose);
        PrimaryHand.GetJointPose(_middleFingerJoint, out _middleFingerTipPose);
        _averageFingerTipPosition = (_indexFingerTipPose.position + _middleFingerTipPose.position) / 2;
        _averageFingerTipEulerAngles = (_indexFingerTipPose.rotation.eulerAngles + _middleFingerTipPose.rotation.eulerAngles) / 2;
        _pinchDownPosition = _averageFingerTipPosition;
    }

    IEnumerator DestroyIdleBomb()
    {
        yield return new WaitForSeconds(TimeToDestroyIdleBomb);

        if (CurrentState == SlingshotState.Idle)
        {
            Destroy(gameObject);
            Debug.Log("Destroy idle TNT");
        }
    }

    void DrawSlingshotLines()
    {
        // IndexFingerLine.positionCount = 2;
        // IndexFingerLine.SetPositions(new Vector3[] { _indexFingerTipPose.position, transform.position });
        // MiddleFingerLine.positionCount = 2;
        // MiddleFingerLine.SetPositions(new Vector3[] { _middleFingerTipPose.position, transform.position });

        var indexFingerLinePoints = SamplePointsAlongLine(_indexFingerTipPose.position, transform.position, 10);
        var middleFingerLinePoints = SamplePointsAlongLine(_middleFingerTipPose.position, transform.position, 10);

        IndexFingerLine.DrawProjectile(indexFingerLinePoints);
        MiddleFingerLine.DrawProjectile(middleFingerLinePoints);
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

    public Vector3[] SamplePointsAlongLine(Vector3 start, Vector3 end, int numPoints)
    {
        List<Vector3> sampledPoints = new List<Vector3>();

        // Ensure that there are at least 2 points to sample
        if (numPoints < 2)
        {
            Debug.LogWarning("Number of points should be at least 2.");
            return sampledPoints.ToArray();
        }

        // Calculate the step size between each point
        Vector3 step = (end - start) / (numPoints - 1);

        // Generate points along the line
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 point = start + step * i;
            sampledPoints.Add(point);
        }

        return sampledPoints.ToArray();
    }

    void OnCollisionEnter(Collision other)
    {
        if (CurrentState == SlingshotState.InLaunch)
        {
            Debug.Log("TNT collided with " + other.gameObject.name);

            if (other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.LandingLayerName) || other.gameObject.layer == LayerMask.NameToLayer(GameManager.Instance.FloorLayerName))
            {
                Vector3 contactPoint = other.contacts[0].point;
                GameManager.Instance.TriggerTNT(contactPoint);
                RaycastVisualizer.HideProjectile();
                Destroy(gameObject);
            }
        }
    }

    // void OnCollisionStay(Collision other)
    // {
    //     if (CurrentState == SlingshotState.InLaunch)
    //     {
    //         Destroy(gameObject);
    //     }
    // }

}
