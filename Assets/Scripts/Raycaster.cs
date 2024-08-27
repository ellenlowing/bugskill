using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

namespace PikminPack
{
    public enum RaycastState
    {
        Idle,
        PreLaunch,
        InLaunch
    }

    public class Raycaster : MonoBehaviour
    {
        [HideInInspector] public RaycastState CurrentState = RaycastState.Idle;
        [HideInInspector] public float V0;
        [HideInInspector] public float LaunchDuration;
        [HideInInspector] public float LaunchAngle;
        [HideInInspector] public float LaunchHeight;
        [HideInInspector] public Vector3 GroundDirectionNorm;
        [HideInInspector] public Vector3 LaunchPosition;
        [HideInInspector] public RaycastHit RaycastHit;
        [HideInInspector] public Pose PointerPose;
        public Color RaycasterTint;

        [SerializeField] private TubeRenderer _tubeRenderer;
        [SerializeField] private float _tubeTrailLength = 2f;
        [SerializeField] private float _tubeTrailStep = 0.01f;
        [SerializeField] private Gradient _prelaunchGradient;
        [SerializeField] private Gradient _inlaunchGradient;
        [SerializeField] private LeanTweenType _inlaunchTweenType;
        private TubePoint [] _arcPoints;


        void Start()
        {
            CurrentState = RaycastState.Idle;
            PointerPose = new Pose(transform.position, transform.rotation);
        }

        void Update()
        {
            transform.SetPositionAndRotation(PointerPose.position, PointerPose.rotation);

            switch(CurrentState)
            {
                case RaycastState.Idle:
                    UpdateIdleState();
                    break;
                
                case RaycastState.PreLaunch:
                    UpdatePreLaunchState();
                    break;

                case RaycastState.InLaunch:
                    UpdateInLaunchState();
                    break;
            }            
        }

        public void SetState(RaycastState state)
        {
            if(state != CurrentState)
            {
                switch(state)
                {
                    case RaycastState.Idle:
                        EnterIdleState();
                        break;

                    case RaycastState.PreLaunch:
                        EnterPreLaunchState();
                        break;

                    case RaycastState.InLaunch:
                        EnterInLaunchState();
                        break;
                }

                CurrentState = state;
            }
        }

        private void EnterIdleState()
        {
            _tubeRenderer.Hide();
        }

        private void EnterPreLaunchState()
        {
            _tubeRenderer.Gradient = _prelaunchGradient;
            _tubeRenderer.Tint = RaycasterTint;
        } 

        private void EnterInLaunchState()
        {
            _tubeRenderer.Gradient = _inlaunchGradient;
            _tubeTrailLength = Mathf.Max(_tubeRenderer.TotalLength * 0.01f, _tubeRenderer.Feather);
            LeanTween.value(gameObject, _tweenStartFadeThreshold, -_tubeTrailLength, _tubeRenderer.TotalLength - _tubeTrailLength, LaunchDuration).setEase(_inlaunchTweenType);
            LeanTween.value(gameObject, _tweenEndFadeThreshold, _tubeRenderer.TotalLength - _tubeTrailLength, -_tubeTrailLength, LaunchDuration).setEase(_inlaunchTweenType);
        }

        private void _tweenStartFadeThreshold(float val)
        {
            _tubeRenderer.StartFadeThresold = val;
            _tubeRenderer.RenderTube(_arcPoints, Space.World);
        }
        private void _tweenEndFadeThreshold(float val)
        {
            _tubeRenderer.EndFadeThresold = val;
            _tubeRenderer.RenderTube(_arcPoints, Space.World);
        }

        private void UpdateIdleState()
        {
            _tubeRenderer.Hide();
        }

        private void UpdatePreLaunchState()
        {
            GetRaycastHit();
            LaunchPosition = transform.position;
            ProjectileLibrary.CalculatePathFromLaunchToTarget(RaycastHit.point, LaunchPosition, out GroundDirectionNorm, out LaunchHeight, out V0, out LaunchDuration, out LaunchAngle);
            Vector3 [] projectilePositions = ProjectileLibrary.GetProjectilePositions(LaunchPosition, GroundDirectionNorm, V0, LaunchDuration, LaunchAngle);
            DrawProjectile(projectilePositions);
        }

        private void UpdateInLaunchState()
        {

        }

        public void GetRaycastHit()
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit);
        }

        public void DrawProjectile(Vector3 [] projectilePositions)
        {
            UpdateProjectilePoints(projectilePositions);
            _tubeRenderer.StartFadeThresold = 0f;
            _tubeRenderer.EndFadeThresold = 0f;
            _tubeRenderer.Feather = _tubeRenderer.TotalLength * 0.1f;
            _tubeRenderer.RenderTube(_arcPoints, Space.World);
        }

        private void UpdateProjectilePoints(Vector3 [] projectilePositions)
        {
            _arcPoints = new TubePoint[projectilePositions.Length-1];
            float totalDistance = 0f;
            for(int i = 0; i < _arcPoints.Length; i++)
            {
                Vector3 position = projectilePositions[i+1];
                Vector3 difference = position - projectilePositions[i];
                totalDistance += difference.magnitude;

                _arcPoints[i].position = position;
                _arcPoints[i].rotation = Quaternion.LookRotation(difference.normalized);
            }

            for(int i = 1; i < _arcPoints.Length; i++)
            {
                float segmentLength = (_arcPoints[i - 1].position - _arcPoints[i].position).magnitude;
                _arcPoints[i].relativeLength = _arcPoints[i - 1].relativeLength + (segmentLength / totalDistance);
            }
        }

    }

}