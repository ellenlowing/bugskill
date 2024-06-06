using TMPro;
using UnityEngine;

namespace Power_Up
{
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

        private float rechargeTimer;
        private bool charged = true;
        private ParticleSystem electricityEffectInstance;


        private void Awake()
        {
            if (ForDebugging) IsHeld = true;
        }

        public override void EnterIdleState()
        {
            // Debug.Log("EnterIdleState");
            ToggleEffects(false, null);
        }

        public override void UpdateIdleState()
        {
            // Debug.Log("UpdateIdleState");
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
            // Debug.Log("EnterInactiveState");
            ToggleEffects(false, DepletedSoundClip);
            charged = false;
            rechargeTimer = RechargeDelay;
        }

        public override void UpdateInactiveState()
        {
            // Debug.Log("UpdateInactiveState");
            // May remove; redundant? just slow recharge? 
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
                    ToggleEffects(false, RechargeSoundClip); // May need to adjust timing that ElectricityEffect and Active sound effect begin relative to recharge sound effect  
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
            // Debug.Log("EnterActiveState");
            ToggleEffects(true, null);
        }

        public override void UpdateActiveState()
        {
            // Debug.Log("UpdateActiveState");
            base.UpdateActiveState(); // Decrements PowerCapacity by UsePowerRate
            if (PowerCapacity <= 0)
            {
                EnterState(PowerUpState.INACTIVE);
                return;
            }

            if (!IsHeld)
            {
                EnterState(PowerUpState.IDLE);
            }
        }

        // Change and play particle and sound effects 
        private void ToggleEffects(bool active, AudioClip clip)
        {
            if (clip != null)
            {
                Debug.Log($"ToggleEffects({active}, {clip.name})");
            }
            else Debug.Log($"ToggleEffects({active}");

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
            Debug.Log("OnTriggerEnter()");

            // If the net collides with a fly while Active 
            if (other.gameObject.CompareTag("Fly") && CurrentState == PowerUpState.ACTIVE)
            {
                Debug.Log("OnTriggerEnter() past checks");
                HitSoundPlayer.Play();

                UIManager.Instance.IncrementKill(other.transform.position);
                other.transform.SetParent(SwatterPosition);

                // Instantiate shock effect on fly
                ParticleSystem hitEffectInstance =
                    Instantiate(HitEffect, other.transform.position, Quaternion.identity);
                hitEffectInstance.Play();
                Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration);

                // Destroy fly after delay 
                Destroy(other.gameObject, destroyFlyDelay);
            }
        }

        [Header("Debugging")]
        public bool ForDebugging = false;
        public TMP_Text isHeldText;
        public TMP_Text currentStateText;
        public TMP_Text powerCapacityText;
        public TMP_Text batteryEffectsText;
        public TMP_Text chargedText;

        public void DebugLogMessage(string message)
        {
            Debug.Log($"{message}");
        }
        private void FixedUpdate()
        {
            UpdateDebugText();
        }

        private void UpdateDebugText()
        {
            isHeldText.text = $"{IsHeld}";
            currentStateText.text = $"{CurrentState}";
            powerCapacityText.text = $"{PowerCapacity}";
            if (BatteryLevelSoundPlayer.clip != null)
            {
                batteryEffectsText.text = $"{BatteryLevelSoundPlayer.clip.name}";
            }
            chargedText.text = $"{charged}";
        }
    }
}

