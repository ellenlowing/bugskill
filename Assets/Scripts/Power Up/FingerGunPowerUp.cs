using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class FingerGunPowerUp : BasePowerUpBehavior
{
    public GameObject Bullet;
    public GameObject Crosshair;
    public ParticleSystem GunshotEffect;
    public SelectorUnityEventWrapper GunIdleEvent;
    public SelectorUnityEventWrapper GunTriggerEvent;
    public bool IsLeftHand = false;
    public float BulletSpeed = 1f;
    public float FiringRate = 0.5f;

    private OVRSkeleton _handSkeleton;
    private Transform _handIndexTipTransform;
    private bool _isIdle = false;
    private bool _isFiring = false;
    private float _lastFiringTime = 0f;
    private LayerMask _landingLayerMask;

    new void Start()
    {
        base.Start();
        _handSkeleton = ActiveHand.GetComponent<OVRSkeleton>();
        GunIdleEvent.WhenSelected.AddListener(() => { _isIdle = true; });
        GunIdleEvent.WhenUnselected.AddListener(() => { _isIdle = false; });
        GunTriggerEvent.WhenSelected.AddListener(TurnOnFiring);
        GunTriggerEvent.WhenUnselected.AddListener(TurnOffFiring);
        _landingLayerMask = LayerMask.NameToLayer(GameManager.Instance.LandingLayerName);
    }

    new void Update()
    {
        base.Update();

        if (ActiveHand.IsTracked)
        {
            foreach (var b in _handSkeleton.Bones)
            {
                if (b.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    _handIndexTipTransform = b.Transform;
                    break;
                }
            }
        }
        else
        {
            _handIndexTipTransform = null;
        }

        if (_handIndexTipTransform != null && (_isIdle || _isFiring))
        {
            Vector3 gunDirection = _handIndexTipTransform.right;
            if (IsLeftHand)
            {
                gunDirection = -gunDirection;
            }
            if (Physics.Raycast(_handIndexTipTransform.position, gunDirection, out RaycastHit hit, _landingLayerMask))
            {
                Crosshair.transform.position = hit.point;
                Crosshair.transform.up = hit.normal;
            }
        }

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
        if (_handIndexTipTransform == null)
        {
            return;
        }

        var bullet = Instantiate(Bullet, _handIndexTipTransform.position, _handIndexTipTransform.rotation);
        if (IsLeftHand)
        {
            bullet.transform.Rotate(-90, 0, 0);
            bullet.transform.Rotate(0, 180, 0);
        }
        else
        {
            bullet.transform.Rotate(90, 0, 0);
        }
        // GunshotEffect.transform.position = _handIndexTipTransform.position;
        // GunshotEffect.Stop();
        // GunshotEffect.Play();

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.right * BulletSpeed;

        _lastFiringTime = Time.time;
    }
}
