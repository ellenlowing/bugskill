using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine;

public class Swatter : BasePowerUpBehavior
{
    [Header("Effects and Sounds")]
    public ParticleSystem ElectricityEffect;
    public ParticleSystem HitEffect; // New particle system for hit effect
    public AudioSource ActiveSound;
    public AudioSource HitSound;
    public AudioSource RechargeSound;
    public AudioSource DepletedSound;

    [Header("Recharge Settings")]
    public float RechargeDelay = 5.0f;  // Time it takes to start recharging after depletion
    private float rechargeTimer;

    public bool IsHeld { get; set; } = false;

    private int flyLayer;

    private void Awake()
    {
        // Set the layer number based on the layer name
        flyLayer = LayerMask.NameToLayer("Fly");
    }

    private void ResetEffects()
    {
        ElectricityEffect.Stop();
        ActiveSound.Stop();
    }

    public override void EnterIdleState()
    {
        base.EnterIdleState();
        if (!IsHeld)
        {
            ResetEffects();
        }
    }

    public override void EnterInactiveState()
    {
        base.EnterInactiveState();
        ResetEffects();
        rechargeTimer = RechargeDelay;
    }

    public override void EnterActiveState()
    {
        base.EnterActiveState();
        if (PowerCapacity > 0)
        {
            ElectricityEffect.Play();
            ActiveSound.Play();
        }
        else
        {
            EnterState(PowerUpState.INACTIVE);
        }
    }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState();
        if (PowerCapacity <= 0)
        {
            EnterState(PowerUpState.INACTIVE);
            DepletedSound.Play();
        }
    }

    public override void UpdateIdleState()
    {
        if (IsHeld)
        {
            EnterState(PowerUpState.ACTIVE);
        }
        else
        {
            Charge();
        }
    }

    public override void UpdateInactiveState()
    {
        if (rechargeTimer > 0)
        {
            rechargeTimer -= Time.deltaTime;
        }
        else if (IsHeld && PowerCapacity > 0)
        {
            RechargeSound.Play();
            EnterState(PowerUpState.IDLE);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == flyLayer && CurrentState == PowerUpState.ACTIVE)
        {
            HitSound.Play();
            ParticleSystem hitEffectInstance = Instantiate(HitEffect, collision.contacts[0].point, Quaternion.identity);
            hitEffectInstance.Play();
            Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration); // Clean up the particle effect after it plays

            // Logic to handle fly being hit
        }
    }
}
