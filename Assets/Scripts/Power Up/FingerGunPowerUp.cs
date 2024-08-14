using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class FingerGunPowerUp : BasePowerUpBehavior
{
    public GameObject Bullet;
    public GameObject Crosshair;
    public SelectorUnityEventWrapper GunIdleEvent;
    public SelectorUnityEventWrapper GunTriggerEvent;
    public bool IsLeftHand = false;
    public float BulletSpeed = 1f;
    public float FiringRate = 0.5f;
    public Transform FirePoint;

    private bool _isIdle = false;
    private bool _isFiring = false;
    private float _lastFiringTime = 0f;
    private LayerMask _landingLayerMask;
    private Vector3 _firePosition;
    private Vector3 _fireDirection;

    new void Start()
    {
        base.Start();
        GunIdleEvent.WhenSelected.AddListener(() => { _isIdle = true; });
        GunIdleEvent.WhenUnselected.AddListener(() => { _isIdle = false; });
        GunTriggerEvent.WhenSelected.AddListener(TurnOnFiring);
        GunTriggerEvent.WhenUnselected.AddListener(TurnOffFiring);
        _landingLayerMask = LayerMask.NameToLayer(GameManager.Instance.LandingLayerName);
    }

    new void Update()
    {
        base.Update();

        if (ActiveHand.IsTracked != Crosshair.activeSelf)
        {
            Crosshair.SetActive(ActiveHand.IsTracked);
        }

        if (!ActiveHand.IsTracked) return;

        // Update firing position and firing direction
        _firePosition = FirePoint.position;
        _fireDirection = FirePoint.right;

        // Update crosshair position
        if (_firePosition != null && _fireDirection != null && (_isIdle || _isFiring))
        {
            Vector3 gunDirection = _fireDirection;
            if (Physics.Raycast(_firePosition, gunDirection, out RaycastHit hit, _landingLayerMask))
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
    }
}
