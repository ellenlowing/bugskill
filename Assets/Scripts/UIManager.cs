using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oculus.Interaction;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Objects")]
    [Space(20)]
    [SerializeField] private GameObject GameStartUI;
    [SerializeField] private GameObject UIScoreObj;
    [SerializeField] private GameObject FailurePanel;
    [SerializeField] private TextMeshProUGUI FailText;
    [SerializeField] private GameObject LevelProgressUI;
    public TextMeshProUGUI WalletText;
    public TextMeshProUGUI ShopWalletText;
    [SerializeField] private TextMeshProUGUI LevelGoalText;
    [SerializeField] private TextMeshProUGUI LevelNumberText;

    [Header("Buttons")]
    [Space(20)]
    [SerializeField] private InteractableUnityEventWrapper GameStartButton;
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

        // subscribe to all events
        GameEnds.OnEventRaised += EndGame;

        GameBegins.OnEventRaised += UpdateLevel;
        StartNextWaveEvent.OnEventRaised += UpdateLevel;

        GameStartButton.WhenSelect.AddListener(StartGameLoopTrigger);
        GameExitButton.WhenSelect.AddListener(QuitGame);

        Invoke(nameof(UpdateStarterMenu), 1.5f);
    }

    private void UpdateStarterMenu()
    {
        FaceCamera(GameStartUI, -0.15f);
        GameStartUI.transform.forward = -Camera.main.transform.forward;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void OnDisable()
    {
    }

    public void FailedPanel(bool state, int kills, int currentIndex)
    {
        // show failure panel
        FailurePanel.SetActive(state);
        FailText.text = "you killed " + settings.numberOfKills + " flies and made " + settings.Cash + " dollars...";
        FaceCamera(FailurePanel);
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
        tempText.text = cashAmount.ToString();
        tempObj = Instantiate(UIScoreObj, position, Quaternion.identity);
        UpdateCashUI();
        // FaceCamera(tempObj);
        tempObj.transform.forward = Camera.main.transform.forward;
        tempObj.transform.eulerAngles = new Vector3(0, tempObj.transform.eulerAngles.y, tempObj.transform.eulerAngles.z);

        Destroy(tempObj, 1f);
    }

    public void UpdateCashUI()
    {
        WalletText.text = settings.Cash.ToString();
        ShopWalletText.text = settings.Cash.ToString();
    }

    public void UpdateLevel()
    {
        int level = settings.waveIndex + 1;
        LevelNumberText.text = level.ToString();
        LevelGoalText.text = settings.LevelGoals[settings.waveIndex].ToString();
    }

    public void StartGameLoopTrigger()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            Debug.Log("Start Button clicked");
            GameBegins.RaiseEvent();
        }
    }

    public void EndGame()
    {
        Debug.Log("UI: Game ended");
        isGameStarted = false;
    }

    public void FaceCamera(GameObject obj, float yOffset = 0f)
    {
        if (obj != null)
        {
            obj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * settings.distanceFromCamera;
            obj.transform.position = new Vector3(obj.transform.position.x, Camera.main.transform.position.y + yOffset, obj.transform.position.z);
            obj.transform.forward = Camera.main.transform.forward;
            obj.transform.eulerAngles = new Vector3(0, obj.transform.eulerAngles.y, obj.transform.eulerAngles.z);
        }
    }

    public void IncrementKill(Vector3 pos, int amount)
    {
        settings.numberOfKills += 1;
        settings.Cash += amount;
        ScoreUIUpdateEvent.RaiseEvent(amount, pos);
    }
}