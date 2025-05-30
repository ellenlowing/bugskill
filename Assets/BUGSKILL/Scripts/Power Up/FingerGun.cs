using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;

public class FingerGun : MonoBehaviour
{
    public GameObject Bullet;
    public GameObject Crosshair;
    public SelectorUnityEventWrapper GunIdleEvent;
    public SelectorUnityEventWrapper GunTriggerEvent;
    public bool IsLeftHand = false;
    public float BulletSpeed = 1f;
    public float FiringRate = 0.5f;
    public OVRHand ActiveOVRHand;
    public GameObject StatusIndicator;

    public Transform FirePoint;
    public float SmoothFactor;

    private bool _isIdle = false;
    private bool _isFiring = false;
    private float _lastFiringTime = 0f;
    private Vector3 _firePosition;
    private Vector3 _fireDirection;
    private int _crosshairRaycastLayerMaskInt;
    private FingerGunPowerUp _corePowerUpBehavior;

    void Start()
    {
        GunIdleEvent.WhenSelected.AddListener(OnGunActive);
        GunIdleEvent.WhenUnselected.AddListener(OnGunIdle);
        GunTriggerEvent.WhenSelected.AddListener(TurnOnFiring);
        GunTriggerEvent.WhenUnselected.AddListener(TurnOffFiring);
        _crosshairRaycastLayerMaskInt = GameManager.Instance.GetAnyLandingLayerMask();

        _firePosition = FirePoint.position;
        _fireDirection = FirePoint.forward;
    }

    void Update()
    {
        if (_corePowerUpBehavior == null)
        {
            _corePowerUpBehavior = FindFirstObjectByType<FingerGunPowerUp>();
            Debug.Log("find core power up behavior for finger gun");
        }

        if (ActiveOVRHand == null) return;

        if (ActiveOVRHand.IsTracked != Crosshair.activeSelf)
        {
            Crosshair.SetActive(ActiveOVRHand.IsTracked);
        }

        if (!ActiveOVRHand.IsTracked) return;

        // Update firing position and firing direction
        _firePosition = Vector3.Lerp(_firePosition, FirePoint.position, SmoothFactor);
        _fireDirection = Vector3.Lerp(_fireDirection, FirePoint.forward, SmoothFactor);

        // Update crosshair position
        if (_firePosition != null && _fireDirection != null)
        {
            if (Physics.Raycast(
                origin: _firePosition,
                direction: _fireDirection,
                hitInfo: out RaycastHit hit,
                maxDistance: Mathf.Infinity,
                layerMask: _crosshairRaycastLayerMaskInt)
            )
            {
                Crosshair.SetActive(true);
                Crosshair.transform.position = hit.point;
                Crosshair.transform.up = hit.normal;
            }
            else
            {
                Crosshair.SetActive(false);
            }
        }

        // Fire bullet at firing rate
        if (_isFiring && Time.time - _lastFiringTime > FiringRate)
        {
            FireBullet();
        }
    }

    void OnGunActive()
    {
        _isIdle = true;
        // StatusIndicator.GetComponent<MeshRenderer>().material.color = _corePowerUpBehavior.FingerGunActiveColor;
    }

    void OnGunIdle()
    {
        _isIdle = false;
        // StatusIndicator.GetComponent<MeshRenderer>().material.color = _corePowerUpBehavior.FingerGunIdleColor;
    }

    void TurnOnFiring()
    {
        _isFiring = true;
        // StatusIndicator.GetComponent<MeshRenderer>().material.color = _corePowerUpBehavior.FingerGunFiringColor;
    }

    void TurnOffFiring()
    {
        _isFiring = false;
        _lastFiringTime = 0f;
        // StatusIndicator.GetComponent<MeshRenderer>().material.color = _corePowerUpBehavior.FingerGunIdleColor;
    }

    public void FireBullet()
    {
        if (_firePosition == null || _fireDirection == null)
        {
            return;
        }

        var projectile = Instantiate(Bullet, _firePosition, Quaternion.identity);
        projectile.transform.forward = _fireDirection;

        _lastFiringTime = Time.time;

        if (_corePowerUpBehavior != null)
        {
            _corePowerUpBehavior.UsePowerCapacity();
        }
    }

}
