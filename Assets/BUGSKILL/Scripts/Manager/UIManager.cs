using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oculus.Interaction;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Meta.XR.MRUtilityKit;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Objects")]
    [Space(20)]
    [SerializeField] private GameObject GameStartUI;
    [SerializeField] private GameObject GameTitle;
    [SerializeField] public GameObject HowToPlayUI;
    [SerializeField] private GameObject UIScoreObj;
    [SerializeField] private GameObject FailurePanel;
    [SerializeField] private TextMeshProUGUI FailText;
    [SerializeField] private GameObject PlayerSettingsUI;
    public TextMeshProUGUI WalletText;
    public TextMeshProUGUI ShopWalletText;
    public TextMeshProUGUI GameUIWalletText;
    [SerializeField] private TextMeshProUGUI LevelGoalText;
    [SerializeField] private TextMeshProUGUI LevelNumberText;
    public TextMeshProUGUI CatchCountUIText;
    public GameObject RoundStartUI;
    public TextMeshProUGUI RoundStartGoalText;
    public float RoundStartUIDuration = 2f;
    public GameObject GameEndUI;
    public TextMeshProUGUI GameEndWinText;
    public TextMeshProUGUI GameEndLoseText;

    [Header("Audio")]
    public MCFlyBehavior WinstonFly;
    public AudioClip WinClip;
    public AudioClip LoseClip;

    [Header("Flask")]
    [Space(20)]
    public Renderer FlaskRenderer;
    public ParticleSystem FlaskParticles;
    public float FlaskFillSpeed = 1f;
    private Coroutine FlaskFillCoroutine;
    public TextMeshProUGUI NumKillsLeftText;
    public Material FlaskPopUpMaterial;
    public Color GoalClearedColor;
    public Color GoalNotClearedColor;
    public GameObject KillsLeftGroup;
    public GameObject LevelClearedGroup;
    public ParticleSystem GoalClearedParticles;
    public AudioSource LevelClearedSound;

    [Header("Buttons")]
    [Space(20)]
    [SerializeField] private InteractableUnityEventWrapper GameStartButton;
    [SerializeField] private InteractableUnityEventWrapper HowToPlayButton;
    [SerializeField] private InteractableUnityEventWrapper PlayerSettingsButton;
    [SerializeField] private InteractableUnityEventWrapper GameExitButton;

    [Header("Events")]
    [Space(20)]
    [Tooltip("Subscribe to run before first game wave")]
    public VoidEventChannelSO GameBegins;
    [Tooltip("Subscribe to activate FrogPowerUp panels and tutorials during cooldown time after a wave")]
    public VoidEventChannelSO FrogPowerUp;
    [Tooltip("Subscribe to activate SprayPowerUp panels and tutorials during cooldown time after a wave")]
    public VoidEventChannelSO SprayPowerUp;
    [Tooltip("Subscribe to activate ElectricSwatter panels and tutorials during cooldown time after a wave")]
    public VoidEventChannelSO ElectricSwatterPowerUp;
    [Tooltip("Subscribe to activate power up upgrades")]
    public VoidEventChannelSO UpgradePowerUps;
    [Tooltip("Subscribe to activate when game ends")]
    public VoidEventChannelSO GameEnds;
    public VoidEventChannelSO BossFightEvent;
    public VoidEventChannelSO StartNextWaveEvent;
    public FVEventSO ScoreUIUpdateEvent;

    private GameObject tempObj;
    private TextMeshProUGUI tempText;
    private SettingSO settings;
    private bool isGameStarted = false;

    private void IsNotNull()
    {
        Assert.IsNotNull(GameStartUI, "UI not Assigned");
        Assert.IsNotNull(GameBegins, "Event not Assigned");
        Assert.IsNotNull(FrogPowerUp, "Event not Assigned");
        Assert.IsNotNull(SprayPowerUp, "Event not Assigned");
        Assert.IsNotNull(ElectricSwatterPowerUp, "Event not Assigned");
        Assert.IsNotNull(UpgradePowerUps, "Event not Assigned");
        Assert.IsNotNull(GameEnds, "Event not Assigned");
    }

    private void OnEnable()
    {
        ScoreUIUpdateEvent.OnEventRaised.AddListener(UpdateScore);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        IsNotNull();

        settings = GameManager.Instance.settings;
        HidePlayerSettings();
        HideHowToPlayScreen();
        GameEndUI.SetActive(false);
        HideRoundStartUI();

        // subscribe to all events
        GameEnds.OnEventRaised += EndGame;
        GameBegins.OnEventRaised += StoreManager.Instance.InitializeStorePosition;
        GameBegins.OnEventRaised += UpdateLevel;
        GameBegins.OnEventRaised += HidePlayerSettings; // TODO change it so that an settings close button is attached to HidePlayerSettings
        StartNextWaveEvent.OnEventRaised += UpdateLevel;

        GameStartButton.WhenSelect.AddListener(StartGameLoopTrigger);
        HowToPlayButton.WhenSelect.AddListener(ShowHowToPlayScreen);
        GameExitButton.WhenSelect.AddListener(QuitGame);
        PlayerSettingsButton.WhenSelect.AddListener(ShowPlayerSettings);

        Invoke(nameof(ShowGameStartScreen), 1.5f);
    }

    private void ShowGameStartScreen()
    {
        GameTitle.GetComponent<FollowCamera>().ResetPosition();
        GameTitle.SetActive(true);
    }

    public void ShowHowToPlayScreen()
    {
        FaceCamera(obj: HowToPlayUI, distanceFromCamera: -0.5f);
        GameTitle.SetActive(false);
    }

    public void ShowPlayerSettings()
    {
        PlayerSettingsUI.SetActive(true);
    }

    public void HideHowToPlayScreen()
    {
        HowToPlayUI.SetActive(false);
    }

    public void HideRoundStartUI()
    {
        RoundStartUI.SetActive(false);
    }

    public void HidePlayerSettings()
    {
        PlayerSettingsUI.SetActive(false);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void FailedPanel(bool state)
    {
        if (state)
        {
            FailText.text = "you killed " + settings.totalKills + " flies and made " + settings.Cash + " dollars...";
            FaceCamera(FailurePanel);
        }
        FailurePanel.SetActive(state);
    }

    public void ShowGameEndPanel(bool win)
    {
        GameManager.Instance.SetHandVisualsActive(true);
        FaceCamera(GameEndUI);
        if (win)
        {
            GameEndWinText.text = String.Format("Splendid work! You've killed {0} flies and saved me from another day of sipping tea and pondering the futility of existence. Onward, dear friend, to our next adventure!", settings.totalKills);
            GameEndWinText.gameObject.SetActive(true);
            GameEndLoseText.gameObject.SetActive(false);
            WinstonFly.PlayClip(WinClip, 3, false);
        }
        else
        {
            GameEndLoseText.text = String.Format("{0} flies down, and yet, here I am - still stuck in this tea-sipping nightmare. Bravo. Perhaps next time we’ll aim for success instead of existential dread, hmm?", settings.totalKills);
            GameEndWinText.gameObject.SetActive(false);
            GameEndLoseText.gameObject.SetActive(true);
            WinstonFly.PlayClip(LoseClip, 3, false);
        }
    }

    public void DestroyPanel(GameObject obj, float waitTime)
    {
        if (obj != null)
        {
            Destroy(obj, waitTime);
        }
    }

    public void UpdateScore(float cashAmount, Vector3 position)
    {
        tempText = UIScoreObj.GetComponentInChildren<TextMeshProUGUI>();
        tempText.text = "+" + cashAmount.ToString();
        tempObj = Instantiate(UIScoreObj, position, Quaternion.identity);
        UpdateCashUI();
        tempObj.transform.forward = GameManager.Instance.MainCameraTransform.forward;
        tempObj.transform.eulerAngles = new Vector3(0, tempObj.transform.eulerAngles.y, tempObj.transform.eulerAngles.z);

        Destroy(tempObj, 1f);
    }

    public void UpdateCashUI()
    {
        WalletText.text = settings.localKills.ToString(); // TODO Update later
        ShopWalletText.text = "$" + settings.Cash.ToString();
        GameUIWalletText.text = "$" + settings.Cash.ToString();
        CatchCountUIText.text = FormatCatchCountUIText();
    }

    public void UpdateLevel()
    {
        int level = settings.waveIndex + 1;
        LevelNumberText.text = level.ToString();
        LevelGoalText.text = settings.LevelGoals[settings.waveIndex].ToString();
        FlaskRenderer.sharedMaterial.SetFloat("_Fill", 0);
        NumKillsLeftText.text = settings.LevelGoals[settings.waveIndex].ToString();
        KillsLeftGroup.SetActive(true);
        LevelClearedGroup.SetActive(false);
        GoalClearedParticles.Stop();
        FlaskPopUpMaterial.color = GoalNotClearedColor;
        CatchCountUIText.text = FormatCatchCountUIText();
        RoundStartGoalText.text = String.Format("DAY {0}:    KILL <color=#FF0000>{1}</color>", level, settings.LevelGoals[settings.waveIndex]);

        // Old approach: Place RoundStart banner above store 
        // RoundStartUI.transform.position = StoreManager.Instance.StoreUI.transform.position + StoreManager.Instance.StoreUI.transform.up;
        // RoundStartUI.transform.forward = -StoreManager.Instance.StoreUI.transform.forward;

        // Spawn roundstartUI in front of user
        FaceCamera(obj: RoundStartUI, distanceFromCamera: settings.farDistanceFromCamera);
        // RoundStartUI.GetComponent<FollowCamera>().ResetPosition();
        RoundStartUI.SetActive(true);
        Invoke(nameof(HideRoundStartUI), RoundStartUIDuration);
    }

    public string FormatCatchCountUIText()
    {
        int kills = settings.localKills;
        int goal = settings.LevelGoals[settings.waveIndex];
        if (kills < goal)
        {
            return "<color=#FF0000>" + kills + "</color> / " + goal;
        }
        else
        {
            return "<color=#00FF00>" + kills + "</color> / " + goal;
        }
    }

    public void StartGameLoopTrigger()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            HideHowToPlayScreen();
            GameEndUI.SetActive(false);
            GameTitle.SetActive(false);
            Debug.Log("Start Button clicked");
            GameBegins.RaiseEvent();
        }
    }

    public void EndGame()
    {
        Debug.Log("UI: Game ended");
        isGameStarted = false;
    }

    public void FaceCamera(GameObject obj, float yOffset = 0f, float distanceFromCamera = -1f, bool flipForwardVector = false, bool placeOnFloor = false)
    {
        if (obj != null)
        {
            float distance = distanceFromCamera == -1f ? settings.nearDistanceFromCamera : distanceFromCamera;

            Vector3 pos = GameManager.Instance.MainCameraTransform.position + GameManager.Instance.MainCameraTransform.forward * distance;
            pos = new Vector3(pos.x, GameManager.Instance.MainCameraTransform.position.y + yOffset, pos.z);

            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            if (room != null)
            {
                while ((!room.IsPositionInRoom(pos, true) || room.IsPositionInSceneVolume(pos, true, 0)) && distance > 0)
                {
                    distance -= 0.1f;
                    pos = GameManager.Instance.MainCameraTransform.position + GameManager.Instance.MainCameraTransform.forward * distance;
                    pos = new Vector3(pos.x, GameManager.Instance.MainCameraTransform.position.y + yOffset, pos.z);

                    Debug.Log("Not in room by distance: " + distance);
                }

                if (placeOnFloor)
                {
                    pos.y = room.FloorAnchor.transform.position.y;
                }
            }

            obj.transform.position = pos;

            if (flipForwardVector)
            {
                obj.transform.forward = -GameManager.Instance.MainCameraTransform.forward;
            }
            else
            {
                obj.transform.forward = GameManager.Instance.MainCameraTransform.forward;
            }
            obj.transform.eulerAngles = new Vector3(0, obj.transform.eulerAngles.y, obj.transform.eulerAngles.z);

            obj.SetActive(true);
        }
    }

    public void IncrementKill(Vector3 pos, int amount)
    {
        settings.localKills += 1;
        settings.Cash += amount;
        ScoreUIUpdateEvent.RaiseEvent(amount, pos);
        // GameManager.Instance.UpdateHandMaterialColor();

        if (settings.localKills <= settings.LevelGoals[settings.waveIndex])
        {
            NumKillsLeftText.text = (settings.LevelGoals[settings.waveIndex] - settings.localKills).ToString();

            if (FlaskFillCoroutine != null)
            {
                StopCoroutine(FlaskFillCoroutine);
                FlaskFillCoroutine = null;
            }
            FlaskFillCoroutine = StartCoroutine(AnimateFlaskLiquidLevel());
        }

        if (KillsLeftGroup.activeInHierarchy && settings.localKills >= settings.LevelGoals[settings.waveIndex])
        {
            FlaskPopUpMaterial.color = GoalClearedColor;
            KillsLeftGroup.SetActive(false);
            LevelClearedGroup.SetActive(true);
            GoalClearedParticles.Play();
            LevelClearedSound.Play();
        }

        FlaskParticles.Stop();
        FlaskParticles.Play();
    }

    IEnumerator AnimateFlaskLiquidLevel()
    {
        float fill = FlaskRenderer.sharedMaterial.GetFloat("_Fill");
        float targetFill = (float)settings.localKills / (float)settings.LevelGoals[settings.waveIndex];
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * FlaskFillSpeed;
            FlaskRenderer.sharedMaterial.SetFloat("_Fill", Mathf.Lerp(fill, targetFill, t));
            yield return null;
        }
    }
}