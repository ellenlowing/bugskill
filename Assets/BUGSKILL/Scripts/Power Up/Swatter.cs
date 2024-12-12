using Oculus.Interaction;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

public class Swatter : BasePowerUpBehavior
{
    public bool IsHeld { get; set; } = false;

    [Header("Effects and Sounds")]
    public ParticleSystem ElectricityEffect;
    public ParticleSystem HitEffect;
    public Transform ElectricityEffectPosition;
    public Transform SwatterPosition;
    public AudioSource ElectricityEffectSoundPlayer;
    public AudioSource BatteryLevelSoundPlayer;
    public AudioSource HitSoundPlayer;
    public float destroyFlyDelay = 0.5f;

    [Header("Audio Clips")]
    public AudioClip RechargeSoundClip;
    public AudioClip DepletedSoundClip;

    [Header("Recharge Settings")]
    public float RechargeDelay = 5.0f; // Time it takes to start recharging after depletion

    private ParticleSystem electricityEffectInstance;

    private void Awake()
    {
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();

    }

    public override void EnterIdleState()
    {
        ToggleEffects(false, null);
    }

    public override void UpdateIdleState()
    {
        base.UpdateIdleState();
    }

    public override void EnterInactiveState()
    {
    }

    public override void UpdateInactiveState()
    {
    }

    public override void EnterActiveState()
    {
        ToggleEffects(true, null);
    }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState();
        UsePowerCapacity();
    }

    public override void Dissolve()
    {
        if (electricityEffectInstance != null)
        {
            Destroy(electricityEffectInstance.gameObject);
        }
        ToggleEffects(false, DepletedSoundClip);
        base.Dissolve();
    }

    // Change and play particle and sound effects 
    private void ToggleEffects(bool active, AudioClip clip)
    {
        if (active)
        {
            electricityEffectInstance =
                Instantiate(ElectricityEffect, ElectricityEffectPosition.position, Quaternion.identity);
            electricityEffectInstance.transform.SetParent(ElectricityEffectPosition);
            electricityEffectInstance.Play();

            ElectricityEffectSoundPlayer.Play();
        }
        else
        {
            if (electricityEffectInstance != null)
            {
                Destroy(electricityEffectInstance.gameObject);
            }

            ElectricityEffectSoundPlayer.Stop();
        }

        if (clip != null)
        {
            BatteryLevelSoundPlayer.clip = clip;
            BatteryLevelSoundPlayer.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CurrentState == PowerUpState.ACTIVE)
        {
            if (other.gameObject.CompareTag("Fly"))
            {
                other.GetComponent<BaseFlyBehavior>().IsKilled = true;
                HitSoundPlayer.Play();
                other.transform.SetParent(SwatterPosition);

                // Instantiate shock effect on fly
                ParticleSystem hitEffectInstance =
                    Instantiate(HitEffect, other.transform.position, Quaternion.identity);
                hitEffectInstance.Play();
                Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration);

                UIManager.Instance.IncrementKill(other.transform.position, (int)SCOREFACTOR.SWATTER);
                // Destroy fly after delay 
                Destroy(other.gameObject, destroyFlyDelay);
            }
        }
    }
}
