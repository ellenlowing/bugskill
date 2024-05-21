using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public SettingSO settings;
    public GameObject UIObject;
    public Image TimerSprite;
    public TextMeshProUGUI KillText;

    [Header("UI Objects")]
    [Space(20)]
    [SerializeField] private GameObject GameStartUI;
    [SerializeField] private GameObject FrogUIObj;
    [SerializeField] private GameObject SprayUIObj;
    [SerializeField] private GameObject UpgradeUIObj;
    [SerializeField] private GameObject SwatterUIObj;
    [SerializeField] private GameObject BossfightUI;


    // public TextMeshProUGUI ScoreText;
    // public TextMeshProUGUI TimerText;

    public bool RunTimer = false;
    private float TimeChange = 0.1f;
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
   


    private void IsNotNull()
    {
        Assert.IsNotNull(GameStartUI, "UI not Assigned");
        Assert.IsNotNull(FrogUIObj , "UI not Assigned");
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

    public void FrogUI()
    {
        FrogUIObj.SetActive(true);
        FaceCamera(FrogUIObj);
        Destroy(FrogUIObj, settings.waveWaitTime);
    }

    public void SwatterUI()
    {
        SwatterUIObj.SetActive(true);
        FaceCamera(SwatterUIObj);
        Destroy(SwatterUIObj, settings.waveWaitTime);
    }

    public void UpgradeUI()
    {
        UpgradeUIObj.SetActive(true);
        FaceCamera(UpgradeUIObj);
        Destroy(UpgradeUIObj, settings.waveWaitTime);
    }

    public void SprayUI()
    {
        SprayUIObj.SetActive(true);
        FaceCamera(SprayUIObj);
        Destroy(SprayUIObj, settings.waveWaitTime);
    }

    public void BossFight()
    {
        BossfightUI.SetActive(true);
        FaceCamera(BossfightUI);
        Destroy(BossfightUI, settings.waveWaitTime);
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

    private void FaceCamera(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * settings.distanceFromCamera;
            obj.transform.forward = Camera.main.transform.forward;
        }
      
    }
}