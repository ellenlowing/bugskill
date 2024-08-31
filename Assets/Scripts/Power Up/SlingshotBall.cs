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
    public SlingshotState CurrentState = SlingshotState.Idle;
    public PointableUnityEventWrapper BallPinchEvent;
    public float LaunchForce;
    public Raycaster RaycastVisualizer;

    private Vector3 _pinchDownPosition;
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        BallPinchEvent.WhenSelect.AddListener(OnSelect);
        BallPinchEvent.WhenUnselect.AddListener(OnUnselect);
        RaycastVisualizer = FindObjectOfType<Raycaster>();
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
    }

    private void EnterPreLaunchState()
    {
        _pinchDownPosition = transform.position;
    }

    private void UpdatePreLaunchState()
    {
        // Draw trajectory
        Vector3 force = (_pinchDownPosition - transform.position) * LaunchForce;
        Vector3[] trajectoryPoints = GetTrajectoryPoints(force, _rb, transform.position); // should last param be indexFingerTipPose.position?
        RaycastVisualizer.DrawProjectile(trajectoryPoints);
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

    void OnCollisionEnter(Collision other)
    {
        if (CurrentState == SlingshotState.InLaunch)
        {
            if (other.gameObject.tag == "Fly")
            {
                UIManager.Instance.IncrementKill(transform.position, (int)SCOREFACTOR.SLINGSHOT);
                Destroy(other.gameObject);
            }

            Destroy(gameObject);
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (CurrentState == SlingshotState.InLaunch)
        {
            Destroy(gameObject);
        }
    }

}
