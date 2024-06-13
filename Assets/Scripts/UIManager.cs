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
    public SettingSO settings;
    public GameObject UIObject;
    public Image TimerSprite;
    public TextMeshProUGUI KillText;
    public bool RunTimer = false;

    [Header("UI Objects")]
    [Space(20)]
    [SerializeField] private GameObject GameStartUI;
    [SerializeField] private GameObject FrogUIObj;
    [SerializeField] private GameObject SprayUIObj;
    [SerializeField] private GameObject UpgradeUIObj;
    [SerializeField] private GameObject SwatterUIObj;
    [SerializeField] private GameObject BossfightUI;
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
    [SerializeField] private InteractableUnityEventWrapper FrogStartButton;
    [SerializeField] private InteractableUnityEventWrapper SprayStartButton;
    [SerializeField] private InteractableUnityEventWrapper SwatterStartButton;
    [SerializeField] private InteractableUnityEventWrapper GameExitButton;


    [Header("Power Up")]
    [Space(20)]
    [SerializeField] private GameObject Swatter;

    private float TimeChange = 0.1f;
    private float quickStart = 0.5f;
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

    private void IsNotNull()
    {
        Assert.IsNotNull(GameStartUI, "UI not Assigned");
        Assert.IsNotNull(FrogUIObj, "UI not Assigned");
        Assert.IsNotNull(SprayUIObj, "UI not Assigned");
        Assert.IsNotNull(UpgradeUIObj, "UI not Assigned");
        Assert.IsNotNull(SwatterUIObj, "UI not Assigned");

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
        // subscribe to all events
        GameEnds.OnEventRaised += KillUpdate;
        FrogPowerUp.OnEventRaised += FrogUI;
        SprayPowerUp.OnEventRaised += SprayUI;
        ElectricSwatterPowerUp.OnEventRaised += SwatterUI;
        UpgradePowerUps.OnEventRaised += UpgradeUI;
        BossFightEvent.OnEventRaised += BossFight;

        StartNextWaveEvent.OnEventRaised += UpdateLevel;

        GameStartButton.WhenSelect.AddListener(StartGameLoopTrigger);
        FrogStartButton.WhenSelect.AddListener(FrogStart);
        SprayStartButton.WhenSelect.AddListener(SprayStart);
        SwatterStartButton.WhenSelect.AddListener(SwatterStart);
        GameExitButton.WhenSelect.AddListener(QuitGame);

        Invoke(nameof(UpdateStarterMenu), 1.5f);
    }

    private void UpdateStarterMenu()
    {
        FaceCamera(GameStartUI);
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
        // settings.waveTimeToComplete = 10.0f;
    }

    public void Update()
    {
        if (RunTimer)
        {
            UpdateTimer();

            if (TimeChange > settings.waveWaitTime)
            {
                RunTimer = false;
                TimeChange = 0;
            }

            TimeChange += Time.deltaTime;
        }
    }

    public void UpdateTimer()
    {
        // TimerText.text = Mathf.RoundToInt(TimeChange) + "/" + Mathf.RoundToInt(settings.waveTimeToComplete);
        // TimerSprite.fillAmount = TimeChange / settings.waveTimeToComplete;
    }

    public void ScoreUpdate()
    {
        // ScoreText.text = "Score : " + settings.score.ToString();
    }

    public void FailedPanel(bool state, int kills, int currentIndex)
    {
        // show failure panel
        FailurePanel.SetActive(state);
        FailText.text = "you killed " + settings.numberOfKills + " flies and made " + settings.Cash + " cash";
        FaceCamera(FailurePanel);
    }

    #region UI QUICK START
    private void FrogStart()
    {
        DestroyPanel(FrogUIObj, quickStart);
        StartNextWaveEvent.RaiseEvent();
    }

    private void SprayStart()
    {
        Destroy(SprayUIObj, quickStart);
        StartNextWaveEvent.RaiseEvent();
    }

    private void SwatterStart()
    {
        Destroy(SwatterUIObj, quickStart);
        StartNextWaveEvent.RaiseEvent();
    }

    private void UpgradeStart()
    {
        Destroy(UpgradeUIObj, quickStart);
        StartNextWaveEvent.RaiseEvent();
    }

    private void BossStart()
    {
        //Destroy(BossFightStartBtn, quickStart);
        StartNextWaveEvent.RaiseEvent();
    }
    #endregion  

    #region  UI POPUP
    public void FrogUI()
    {
        FrogUIObj.SetActive(true);
        FaceCamera(FrogUIObj);
    }

    public void SwatterUI()
    {
        SwatterUIObj.SetActive(true);
        FaceCamera(SwatterUIObj);
    }

    public void UpgradeUI()
    {
        UpgradeUIObj.SetActive(true);
        FaceCamera(UpgradeUIObj);
    }

    public void SprayUI()
    {
        SwatterUIObj.SetActive(false);
        SprayUIObj.SetActive(true);
        FaceCamera(SprayUIObj);
    }

    public void BossFight()
    {
        BossfightUI.SetActive(true);
        FaceCamera(BossfightUI);
    }
    #endregion

    public void DestroyPanel(GameObject obj, float waitTime)
    {
        if (obj != null)
        {
            Destroy(obj, waitTime);
        }
    }

    public void KillUpdate()
    {
        UIObject.SetActive(true);
        // set position to forward vector of center eye with match height
        FaceCamera(UIObject);

        if (settings.numberOfKills == 1)
        {
            KillText.text = "you killed " + "<color=red>" + settings.numberOfKills + "</color>" + " fly...";
        }
        else
        {
            KillText.text = "you killed " + "<color=red>" + settings.numberOfKills + "</color>" + " flies...";
        }
    }

    public void UpdateScore(float cashAmount, Vector3 position)
    {
        tempText = UIScoreObj.GetComponentInChildren<TextMeshProUGUI>();
        tempText.text = cashAmount.ToString();
        tempObj = Instantiate(UIScoreObj, position, Quaternion.identity);
        UpdateCashUI();
        FaceCamera(tempObj);
        Destroy(tempObj, 1f);
    }

    public void AddTextPopUp(string text, Vector3 position)
    {
        tempText = UIScoreObj.GetComponentInChildren<TextMeshProUGUI>();
        tempText.text = text;
        tempObj = Instantiate(UIScoreObj, position, Quaternion.identity);
        FaceCamera(tempObj);
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
        Debug.Log("Start Button clicked");
        Destroy(GameStartUI, quickStart);
        UpdateLevel();
        GameBegins.RaiseEvent();
    }

    public void FaceCamera(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * settings.distanceFromCamera;
            obj.transform.position = new Vector3(obj.transform.position.x, Camera.main.transform.position.y, obj.transform.position.z);
            obj.transform.forward = Camera.main.transform.forward;
            obj.transform.eulerAngles = new Vector3(0, obj.transform.eulerAngles.y, obj.transform.eulerAngles.z);
        }
    }

    public void IncrementKill(Vector3 pos, int amount)
    {
        settings.numberOfKills += 1;
        ScoreUIUpdateEvent.RaiseEvent(amount, pos);
    }
}