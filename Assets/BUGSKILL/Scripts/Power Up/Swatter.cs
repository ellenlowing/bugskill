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
    public Transform SwatterPosition;
    public AudioSource ElectricityEffectSoundPlayer;
    public AudioSource HitSoundPlayer;
    public float destroyFlyDelay = 0.5f;

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
        ToggleEffects(false);
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
        ToggleEffects(true);
    }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState();
        UsePowerCapacity();
    }

    public override void Dissolve()
    {
        ToggleEffects(false);
        base.Dissolve();
    }

    // Change and play particle and sound effects 
    private void ToggleEffects(bool active)
    {
        if (active)
        {
            ElectricityEffect.Play();
            ElectricityEffectSoundPlayer.Play();
        }
        else
        {
            ElectricityEffect.Stop();
            ElectricityEffectSoundPlayer.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CurrentState == PowerUpState.ACTIVE)
        {
            if (other.gameObject.CompareTag("Fly"))
            {
                BaseFlyBehavior fly = other.gameObject.GetComponent<BaseFlyBehavior>();
                HitSoundPlayer.Play();
                other.transform.SetParent(SwatterPosition);

                // Instantiate shock effect on fly
                HitEffect.Play();

                UIManager.Instance.IncrementKill(other.transform.position, (int)SCOREFACTOR.SWATTER);
                fly.Kill(destroyFlyDelay);
            }
        }
    }
}
