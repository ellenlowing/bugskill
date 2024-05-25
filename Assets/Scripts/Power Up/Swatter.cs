using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swatter : BasePowerUpBehavior
{
    public float destroyFlyDelay = 0.5f;
    public Transform ElectricityEffectPosition; 
    public bool IsHeld { get; set; } = false;
    
    [Header("Effects and Sounds")]
    public ParticleSystem ElectricityEffect;
    public ParticleSystem HitEffect;
    public AudioSource SoundPlayer;

    [Header("Audio Clips")]
    public AudioClip ActiveSoundClip;
    public AudioClip HitSoundClip;
    public AudioClip RechargeSoundClip;
    public AudioClip DepletedSoundClip;

    [Header("Recharge Settings")]
    public float RechargeDelay = 5.0f;  // Time it takes to start recharging after depletion
    
    private float rechargeTimer;
    private bool charged = true;
    private int flyLayer;
    private ParticleSystem electricityEffectInstance; 


    private void Awake()
    {
        flyLayer = LayerMask.NameToLayer("Fly");
    }
    
    public override void EnterIdleState()
    {
        ToggleEffects(false, null, false); //Need to check if null causes errors
    }
    public override void UpdateIdleState()
    {
        if (!charged)
        {
            EnterState(PowerUpState.INACTIVE);
        }
        else if (IsHeld)
        {
            EnterState(PowerUpState.ACTIVE);
        }
    }
    public override void EnterInactiveState()
    {
        ToggleEffects(false, DepletedSoundClip, false);
        charged = false;
        rechargeTimer = RechargeDelay;
    }


    public override void UpdateInactiveState()
    {
        if (rechargeTimer > 0)
        {
            rechargeTimer -= Time.deltaTime;
        }
        else
        {
            Charge();
            if (PowerCapacity >= MaxPowerCapacity)
            {
                charged = true;
                ToggleEffects(true, RechargeSoundClip, false); // May need to adjust timing that ElectricityEffect and Active sound effect begin relative to recharge sound effect  
                if (IsHeld)
                {
                    EnterState(PowerUpState.ACTIVE);
                }
                else //come back 
                {
                    EnterState(PowerUpState.IDLE);
                }
            }
        }
    }
    
    public override void EnterActiveState()
     {
         ToggleEffects(true, ActiveSoundClip, true);
     }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState(); // Decrements PowerCapacity by UsePowerRate
        if (PowerCapacity <= 0)
        {
            EnterState(PowerUpState.INACTIVE);
        }
    }

    // Change and play particle and sound effects 
    private void ToggleEffects(bool active, AudioClip clip, bool loop)
    {
        if (clip != null)
        {
            SoundPlayer.clip = clip;
            SoundPlayer.loop = loop; 
        }

        if (active)
        {
            electricityEffectInstance = Instantiate(ElectricityEffect, ElectricityEffectPosition.position, Quaternion.identity);
            electricityEffectInstance.Play();        
        }
        else
        {
            if (electricityEffectInstance != null)
            {
                Destroy(electricityEffectInstance); 
            }
            SoundPlayer.Stop();
            return; 
        }
        
        SoundPlayer.Play();
    }
    private void OnCollisionEnter(Collision collision)
    {
        // If the net collides with a fly while Active 
        if (collision.gameObject.layer == flyLayer && CurrentState == PowerUpState.ACTIVE)
        {
            // Play a sound
            SoundPlayer.clip = HitSoundClip;
            SoundPlayer.loop = false;  
            SoundPlayer.Play();
            
            // Instantiate shock effect at contact point 
            ParticleSystem hitEffectInstance = Instantiate(HitEffect, collision.contacts[0].point, Quaternion.identity);
            hitEffectInstance.Play();
            Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration);
            
            // Destroy fly after delay 
            Destroy(collision.gameObject, destroyFlyDelay);
        }
    }
}


//
// private void ToggleActiveEffects(bool on)
// {
//     if (on)
//     {
//         ElectricityEffect.Play();
//         SoundPlayer.clip = ActiveSoundClip;
//         SoundPlayer.loop = true;
//         SoundPlayer.Play();
//     }
//     else
//     {
//         ElectricityEffect.Stop();
//         SoundPlayer.Stop();
//         SoundPlayer.loop = false; 
//     }
// }


//
// public class Swatter : BasePowerUpBehavior
// {
//     // Idle = no sound or particle effects
//     // Active = sound and particle effects while held
//     // Inactive = no sound and particle effects while held (because dead and/or recharging) 
//     
//     [Header("Effects and Sounds")]
//     public ParticleSystem ElectricityEffect;
//     public ParticleSystem HitEffect;
//     public AudioSource SoundPlayer;
//
//     [Header("Audio Clips")]
//     public AudioClip ActiveSoundClip;
//     public AudioClip HitSoundClip;
//     public AudioClip RechargeSoundClip;
//     public AudioClip DepletedSoundClip;
//
//     [Header("Recharge Settings")]
//     public float RechargeDelay = 5.0f;  // Time it takes to start recharging after depletion
//     private float rechargeTimer;
//
//     public bool IsHeld { get; set; } = false;
//
//     private int flyLayer;
//
//     private void Awake()
//     {
//         flyLayer = LayerMask.NameToLayer("Fly");
//     }
//
//     private void ResetEffects()
//     {
//         ElectricityEffect.Stop();
//         SoundPlayer.Stop();
//     }
//
//     public override void EnterIdleState()
//     {
//         base.EnterIdleState();
//         if (!IsHeld)
//         {
//             ResetEffects();
//         }
//     }
//
//     public override void EnterInactiveState()
//     {
//         base.EnterInactiveState();
//         ResetEffects();
//         rechargeTimer = RechargeDelay;
//         SoundPlayer.clip = DepletedSoundClip;
//         SoundPlayer.Play();
//     }
//
//     public override void EnterActiveState()
//     {
//         base.EnterActiveState();
//         if (PowerCapacity > 0)
//         {
//             ElectricityEffect.Play();
//             SoundPlayer.clip = ActiveSoundClip;
//             SoundPlayer.loop = true;
//             SoundPlayer.Play();
//         }
//         else
//         {
//             EnterState(PowerUpState.INACTIVE);
//         }
//     }
//
//     public override void UpdateActiveState()
//     {
//         base.UpdateActiveState();
//         if (PowerCapacity <= 0)
//         {
//             EnterState(PowerUpState.INACTIVE);
//             SoundPlayer.loop = false;
//         }
//     }
//
//     public override void UpdateIdleState()
//     {
//         if (IsHeld)
//         {
//             EnterState(PowerUpState.ACTIVE);
//         }
//         else
//         {
//             Charge();
//         }
//     }
//
//     public override void UpdateInactiveState()
//     {
//         if (rechargeTimer > 0)
//         {
//             rechargeTimer -= Time.deltaTime;
//         }
//         else
//         {
//             Charge();
//             if (PowerCapacity >= MaxPowerCapacity)
//             {
//                 SoundPlayer.clip = RechargeSoundClip;
//                 SoundPlayer.Play();
//                 EnterState(PowerUpState.IDLE);
//             }
//         }
//     }
//
//     private void OnCollisionEnter(Collision collision)
//     {
//         if (collision.gameObject.layer == flyLayer && CurrentState == PowerUpState.ACTIVE)
//         {
//             SoundPlayer.clip = HitSoundClip;
//             SoundPlayer.Play();
//             ParticleSystem hitEffectInstance = Instantiate(HitEffect, collision.contacts[0].point, Quaternion.identity);
//             hitEffectInstance.Play();
//             Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration);
//         }
//     }
// }