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
    public Transform FirePoint;
    public OVRHand ActiveOVRHand;

    private bool _isIdle = false;
    private bool _isFiring = false;
    private float _lastFiringTime = 0f;
    private Vector3 _firePosition;
    private Vector3 _fireDirection;
    private int _crosshairRaycastLayerMaskInt;
    private FingerGunPowerUp _corePowerUpBehavior;

    void Start()
    {
        GunIdleEvent.WhenSelected.AddListener(() => { _isIdle = true; });
        GunIdleEvent.WhenUnselected.AddListener(() => { _isIdle = false; });
        GunTriggerEvent.WhenSelected.AddListener(TurnOnFiring);
        GunTriggerEvent.WhenUnselected.AddListener(TurnOffFiring);
        _crosshairRaycastLayerMaskInt = 1 << LayerMask.NameToLayer(GameManager.Instance.LandingLayerName);
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
        _firePosition = FirePoint.position;
        _fireDirection = FirePoint.right;

        // Update crosshair position
        if (_firePosition != null && _fireDirection != null && (_isIdle || _isFiring))
        {
            Vector3 gunDirection = _fireDirection;

            if (Physics.Raycast(
                origin: _firePosition,
                direction: gunDirection,
                hitInfo: out RaycastHit hit,
                maxDistance: 100f,
                layerMask: _crosshairRaycastLayerMaskInt)
            )
            {
                Crosshair.transform.position = hit.point;
                Crosshair.transform.up = hit.normal;
            }
        }

        // Fire bullet at firing rate
        if (_isFiring && Time.time - _lastFiringTime > FiringRate)
        {
            FireBullet();
        }
    }

    void TurnOnFiring()
    {
        _isFiring = true;
    }

    void TurnOffFiring()
    {
        _isFiring = false;
        _lastFiringTime = 0f;
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
