using Oculus.Interaction;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

namespace Power_Up
{
    public class Swatter : BasePowerUpBehavior
    {
        public bool IsHeld { get; set; } = false;

        [Header("Settings Data")]
        [SerializeField] private SettingSO settings;

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

        // [Header("Activate Button")]
        // public SwatterActivateButton ActivateButton;

        private ParticleSystem electricityEffectInstance;

        private void Awake()
        {
        }

        new void Start()
        {
            settings = GameManager.Instance.settings;
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

            if (!StoreManager.Instance.IsStoreActive)
            {
                if (PowerCapacity > 0)
                {
                    Debug.Log("PowerCapacity: " + PowerCapacity);
                    PowerCapacity -= UsePowerRate;
                }
            }
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
            // if (clip != null)
            // {
            //     Debug.Log($"ToggleEffects({active}, {clip.name})");
            // }
            // else
            // {
            //     Debug.Log($"ToggleEffects({active}");
            // }

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

        private int totalCash;

        private void OnTriggerEnter(Collider other)
        {
            if (GameManager.Instance.IsTNTTriggered)
            {
                return;
            }

            if (CurrentState == PowerUpState.ACTIVE)
            {
                if (other.gameObject.CompareTag("Fly"))
                {
                    HitSoundPlayer.Play();
                    other.transform.SetParent(SwatterPosition);

                    // Instantiate shock effect on fly
                    ParticleSystem hitEffectInstance =
                        Instantiate(HitEffect, other.transform.position, Quaternion.identity);
                    hitEffectInstance.Play();
                    Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration);

                    settings.Cash += (int)SCOREFACTOR.SWATTER;
                    totalCash += (int)SCOREFACTOR.SWATTER;
                    UIManager.Instance.IncrementKill(other.transform.position, totalCash);
                    totalCash = 0;
                    // Destroy fly after delay 
                    Destroy(other.gameObject, destroyFlyDelay);
                }
                else if (other.gameObject.tag == "TNT")
                {
                    GameManager.Instance.TriggerTNT(other.transform.position, other.gameObject);
                }
            }
        }
    }
}

