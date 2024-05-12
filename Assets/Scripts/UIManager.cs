using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public SettingSO settings;
    public GameObject UIObject;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI KillText;
    public TextMeshProUGUI TimerText;
    public Image TimerSprite;
    public bool RunTimer = false;

    private float TimeChange = 0.1f;

    private void Start()
    {
        
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

            if(TimeChange > settings.WaveWaitTime)
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
        ScoreText.text = "Score : " + settings.score.ToString();
    }

    public void KillUpdate()
    {
        UIObject.SetActive(true);

        // set position to forward vector of center eye with match height
        UIObject.transform.position = OVRManager.instance.GetComponent<OVRCameraRig>().centerEyeAnchor.position + new Vector3(0, 0, settings.distanceFromCamera);

        if(settings.numberOfKills == 1)
        {
            KillText.text = "you killed " + "<color=yellow>" + settings.numberOfKills + "</color>" + " fly...";
        }
        else
        {
            KillText.text = "you killed " + "<color=yellow>" + settings.numberOfKills + "</color>" + " flies...";
        }
    }
}
