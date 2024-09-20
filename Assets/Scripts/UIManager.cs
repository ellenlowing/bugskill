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
    [SerializeField] private GameObject LevelProgressUI;
    public TextMeshProUGUI WalletText;
    public TextMeshProUGUI ShopWalletText;
    [SerializeField] private TextMeshProUGUI LevelGoalText;
    [SerializeField] private TextMeshProUGUI LevelNumberText;

    [Header("Buttons")]
    [Space(20)]
    [SerializeField] private InteractableUnityEventWrapper GameStartButton;
    [SerializeField] private InteractableUnityEventWrapper HowToPlayButton;
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
        HowToPlayUI.SetActive(false);

        // subscribe to all events
        GameEnds.OnEventRaised += EndGame;

        GameBegins.OnEventRaised += UpdateLevel;
        StartNextWaveEvent.OnEventRaised += UpdateLevel;

        GameStartButton.WhenSelect.AddListener(StartGameLoopTrigger);
        HowToPlayButton.WhenSelect.AddListener(ShowHowToPlayScreen);
        GameExitButton.WhenSelect.AddListener(QuitGame);

        Invoke(nameof(ShowGameStartScreen), 1.5f);
    }

    private void ShowGameStartScreen()
    {
        // FaceCamera(obj: GameStartUI, yOffset: -0.15f, flipForwardVector: true);
        FaceCamera(obj: GameTitle, yOffset: 0.4f);
    }

    public void ShowHowToPlayScreen()
    {
        FaceCamera(obj: HowToPlayUI, distanceFromCamera: 0f);
        GameTitle.SetActive(false);
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
        FailurePanel.SetActive(state);
        FailText.text = "you killed " + settings.totalKills + " flies and made " + settings.Cash + " dollars...";
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
        tempObj.transform.forward = Camera.main.transform.forward;
        tempObj.transform.eulerAngles = new Vector3(0, tempObj.transform.eulerAngles.y, tempObj.transform.eulerAngles.z);

        Destroy(tempObj, 1f);
    }

    public void UpdateCashUI()
    {
        WalletText.text = settings.localKills.ToString(); // TODO Update later
        ShopWalletText.text = "$" + settings.Cash.ToString();
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
            HowToPlayUI.SetActive(false);
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

    public void FaceCamera(GameObject obj, float yOffset = 0f, float distanceFromCamera = -1f, bool flipForwardVector = false)
    {
        if (obj != null)
        {
            float distance = distanceFromCamera == -1f ? settings.distanceFromCamera : distanceFromCamera;

            Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * distance;
            pos = new Vector3(pos.x, Camera.main.transform.position.y + yOffset, pos.z);

            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            if (room != null)
            {
                while ((!room.IsPositionInRoom(pos, true) || room.IsPositionInSceneVolume(pos, true, 0)) && distance > 0)
                {
                    distance -= 0.1f;
                    pos = Camera.main.transform.position + Camera.main.transform.forward * distance;
                    pos = new Vector3(pos.x, Camera.main.transform.position.y + yOffset, pos.z);

                    Debug.Log("Not in room by distance: " + distance);
                }
            }

            obj.transform.position = pos;
            if (flipForwardVector)
            {
                obj.transform.forward = -Camera.main.transform.forward;
            }
            else
            {
                obj.transform.forward = Camera.main.transform.forward;
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
    }
}