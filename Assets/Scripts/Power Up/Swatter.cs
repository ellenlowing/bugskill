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
            if (ForDebugging) IsHeld = true;
        }

        new void Start()
        {
            base.Start();
            PointableEventWrapper.WhenSelect.AddListener(OnGrabbableSelect);
            PointableEventWrapper.WhenUnselect.AddListener(OnGrabbableUnselect);
            // ActivateButton.WhenActivated.AddListener(OnButtonActivated);
            // ActivateButton.WhenDeactivated.AddListener(OnButtonDeactivated);
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
            // TODO do dissolve effect
            if (PowerCapacity <= 0)
            {
                PowerCapacity = 0;
                ToggleEffects(false, DepletedSoundClip);
            }
        }

        public override void UpdateInactiveState()
        {
            // base.UpdateInactiveState();
            // Charge();
            // if (PowerCapacity >= MaxPowerCapacity)
            // {
            //     ToggleEffects(false, RechargeSoundClip); // May need to adjust timing that ElectricityEffect and Active sound effect begin relative to recharge sound effect  
            // }
        }

        public override void EnterActiveState()
        {
            ToggleEffects(true, null);
        }

        public override void UpdateActiveState()
        {
            base.UpdateActiveState();

            if (PowerCapacity < 0)
            {
                EnterState(PowerUpState.INACTIVE);
                Debug.Log("Power capacity is less than 0");
            }
        }

        // Change and play particle and sound effects 
        private void ToggleEffects(bool active, AudioClip clip)
        {
            if (clip != null)
            {
                Debug.Log($"ToggleEffects({active}, {clip.name})");
            }
            else
            {
                Debug.Log($"ToggleEffects({active}");
            }

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
            if (other.gameObject.CompareTag("Fly") && CurrentState == PowerUpState.ACTIVE)
            {
                HitSoundPlayer.Play();

               
                other.transform.SetParent(SwatterPosition);

                // Instantiate shock effect on fly
                ParticleSystem hitEffectInstance =
                    Instantiate(HitEffect, other.transform.position, Quaternion.identity);
                hitEffectInstance.Play();
                Destroy(hitEffectInstance.gameObject, hitEffectInstance.main.duration);
                if(other.gameObject.transform.localScale == Vector3.one)
                {
                    settings.Cash += (int)SCOREFACTOR.SLIM;
                    totalCash += (int)SCOREFACTOR.SLIM;
                }
                else
                {
                    settings.Cash += (int)SCOREFACTOR.FAT;
                    totalCash += (int)SCOREFACTOR.FAT;
                }

                settings.Cash += (int)SCOREFACTOR.SWATTER;
                totalCash += (int)SCOREFACTOR.SWATTER;
                UIManager.Instance.IncrementKill(other.transform.position, totalCash);
                totalCash = 0;
                // Destroy fly after delay 
                Destroy(other.gameObject, destroyFlyDelay);
            }
        }

        private void OnGrabbableSelect(PointerEvent arg0)
        {
            IsHeld = true;
            EnterState(PowerUpState.ACTIVE);
        }

        private void OnGrabbableUnselect(PointerEvent arg0)
        {
            IsHeld = false;
            EnterState(PowerUpState.IDLE);
        }

        private void OnButtonActivated()
        {
            if (PowerCapacity > 0 && IsHeld)
            {
                EnterState(PowerUpState.ACTIVE);
            }
        }

        private void OnButtonDeactivated()
        {
            if (IsHeld)
            {
                EnterState(PowerUpState.INACTIVE);
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
        }
    }
}

