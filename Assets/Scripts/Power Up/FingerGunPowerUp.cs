using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class FingerGunPowerUp : BasePowerUpBehavior
{
    public GameObject Bullet;
    public ParticleSystem GunshotEffect;
    public SelectorUnityEventWrapper SelectorEventWrapper;
    public bool IsLeftHand = false;
    public float BulletSpeed = 1f;
    public float FiringRate = 0.5f;

    public DebugGizmos dg;

    private OVRSkeleton _handSkeleton;
    private Transform _handIndexTipTransform;
    private bool _isFiring = false;
    private float _lastFiringTime = 0f;

    new void Start()
    {
        base.Start();
        _handSkeleton = ActiveHand.GetComponent<OVRSkeleton>();
        SelectorEventWrapper.WhenSelected.AddListener(TurnOnFiring);
        SelectorEventWrapper.WhenUnselected.AddListener(TurnOffFiring);
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

        var bullet = Instantiate(Bullet, _handIndexTipTransform.position, ActiveHand.transform.rotation);
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
