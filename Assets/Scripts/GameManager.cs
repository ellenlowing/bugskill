using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor;
using System.Dynamic;
using Oculus.Interaction;

// kills set to zero on start
// cash still running , will reset somewhere safe

public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<MRUKAnchor> FlySpawnPositions;
    public GameObject FlyPrefab;
    public SettingSO settings;
    public List<GameObject> BloodSplatterPrefabs;
    public GameObject splatterParticle;
    public Transform FlyParentAnchor;
    public UIManager UIM;
    public Animator animator;
    public GameObject HourGlass;
    public GameObject Portal;

    [Header("Game Events")]
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
    [Tooltip("Subscribe to activate during boss fight begin")]
    public VoidEventChannelSO BossFightEvent;
    [Tooltip("Subscribe to activate when game ends")]
    public VoidEventChannelSO GameEnds;
    [Tooltip("Starts the Next Wave Event")]
    public VoidEventChannelSO StartNextWaveEvent;
    [Tooltip("Failed Level Event")]
    public InteractableUnityEventWrapper GameRestartEvent;

    [Header("Hands")]
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject LeftHandRenderer;
    public GameObject RightHandRenderer;

    private int waveIndex = 0;
    private bool canSpawn = true;
    private bool moveToNextWave = false;
    private float initialTime = 0;
    private GameObject fly;
    private Coroutine GameLoopRoutine;

    private int runningIndex = 0;
    private int LocalKills = 0;
    private int LocalCash = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        settings.waveIndex = 0;
        settings.flies = new List<GameObject>();
        FlySpawnPositions = new List<MRUKAnchor>();
    }

    void Start()
    {
        // check references
        UIM = GetComponent<UIManager>();
        settings.numberOfKills = 0;
        GameRestartEvent.WhenSelect.AddListener(RestartGameLoop);

        Assert.IsNotNull(UIM, "UIManager Reference Missing");

        HourGlass.SetActive(false);

        // GetWindowOrDoorFrames(MRUK.Instance.GetCurrentRoom());
    }

    void Update()
    {
        TrackTimer();

        // restarting for quick testing on deployment
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }


    private void TrackTimer()
    {
        if (moveToNextWave)
        {
            if (initialTime > settings.durationOfWave[waveIndex])
            {
                // empty all fly list
                foreach (var obj in settings.flies)
                {
                    Destroy(obj);
                }
                settings.flies.Clear();
                settings.flies = new List<GameObject>();

                // waveIndex++;
                // update wave index across gameplay


                // moveToNextWave = false;
                // canSpawn = true;
                // enable hour glass here
                // set the animation speed to scale with div factor
                // HourGlass.SetActive(true);

                // animator.speed = settings.divFactor / settings.waveWaitTime;
                // animator.speed = 0.02f;
                // HourGlass.SetActive(false);

                initialTime = 0;
            }
            initialTime += Time.deltaTime;
        }
    }

    IEnumerator SpawnFlyAtRandomPosition()
    {
        if (FlySpawnPositions.Count == 0)
        {
            Debug.LogWarning("Fly Spawn Positions Were Zero");
            yield break;
        }

        while (true)
        {
            if (FlySpawnPositions.Count > 0)
            {

                if (waveIndex == settings.fliesInWave.Length)
                {
                    // call completion here with ui score update
                    Debug.LogWarning("Wave Index Same as Number of Flies in Wave");
                    GameEnds.RaiseEvent();
                    yield break;
                }

                // loop here with wave count which changes
                // destroy all current flies before next wave
                // before next wave, wait for certain amount of time
                if (canSpawn)
                {
                    for (int i = runningIndex; i < settings.fliesInWave[waveIndex]; i++)
                    {
                        int randomIndex = Random.Range(0, FlySpawnPositions.Count);
                        MRUKAnchor randomAnchor = FlySpawnPositions[randomIndex];
                        Vector3 randomPosition = randomAnchor.GetAnchorCenter();
                        if (randomAnchor.PlaneRect.HasValue)
                        {
                            Vector2 size = randomAnchor.PlaneRect.Value.size;
                            randomPosition += new Vector3(Random.Range(-size.x / 2, size.x / 2), size.y / 2, 0);
                        }

                        if (settings.fliesInWave[waveIndex] == 1)
                        {
                            fly = Instantiate(FlyPrefab, randomPosition, Quaternion.identity, FlyParentAnchor);
                            fly.transform.localScale = new Vector3(4, 4, 4);
                        }
                        else
                        {
                            fly = Instantiate(FlyPrefab, randomPosition, Quaternion.identity, FlyParentAnchor);
                        }


                        fly.transform.up = randomAnchor.transform.forward;
                        fly.transform.rotation = fly.transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);

                        // keep reference to all spawned flies
                        // spawn wave number through loop which uses settings factor
                        settings.flies.Add(fly);

                    }
                    canSpawn = false;
                    moveToNextWave = true;

                    // enable and set timescale for loading based on time anticapated per wave
                    HourGlass.SetActive(true);
                    animator.speed = settings.divFactor / settings.durationOfWave[waveIndex];
                }

                // check if all flies are killed
                // move to next wave count
                // play theme wave wait sound
                if (settings.flies.Count == 0)
                {
                    waveIndex++;
                    settings.waveIndex = waveIndex;
                    moveToNextWave = false;
                    canSpawn = true;

                    LocalKills = settings.numberOfKills - LocalKills;
                    LocalCash = settings.Cash - LocalCash;

                    CanProgress(waveIndex);
                    WhatPowerUp(waveIndex);
                    animator.speed = 1f;

                    if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    {
                        animator.speed = 0;
                    }
                    // enable hour glass here
                    // set the animation speed to scale with div factor
                    // HourGlass.SetActive(true);
                    // animator.speed = settings.divFactor / settings.waveWaitTime;
                    // animator.speed = 0.02f;
                    // animator.speed = 0;
                    yield return new WaitForSeconds(settings.waveWaitTime);
                    // HourGlass.SetActive(false);
                }

                // 1 second frame checks
                yield return null;
                // yield return new WaitForSeconds(Random.Range(settings.flySpawnIntervalMin, settings.flySpawnIntervalMax));
            }
        }
    }

    private void OnEnable()
    {
        StartNextWaveEvent.OnEventRaised += StartNextWave;
        GameBegins.OnEventRaised += StartGameLoop;
    }

    // force next wave to start
    public void StartNextWave()
    {
        StopCoroutine(GameLoopRoutine);
        runningIndex = waveIndex;
        canSpawn = true;
        animator.speed = settings.divFactor / settings.durationOfWave[waveIndex];
        animator.Play("Animation", 0, 0);
        GameLoopRoutine = StartCoroutine(SpawnFlyAtRandomPosition());
        StoreManager.Instance.HideStore();
    }


    // check state of progression 
    private void CanProgress(int waveIndex)
    {
        switch (waveIndex)
        {
            case 0:
                CheckGoal(waveIndex);
                break;
            case 1:
                CheckGoal(waveIndex);
                break;
            case 2:
                CheckGoal(waveIndex);
                break;
            case 3:
                CheckGoal(waveIndex);
                break;
            case 4:
                CheckGoal(waveIndex);
                break;
        }
    }



    private void CheckGoal(int waveI)
    {
        //if (!(LocalKills >= settings.LevelGoals[waveI]))
        //{
        //    UIManager.Instance.FailedPanel(true, LocalKills, waveIndex);
        //    canSpawn = false;
        //    waveIndex = 0;
        //    runningIndex = waveIndex;     

        //    StopCoroutine(GameLoopRoutine);
        //}

        if (!(LocalCash >= settings.LevelGoals[waveI]))
        {
            UIManager.Instance.FailedPanel(true, LocalCash, waveIndex);
            canSpawn = false;
            waveIndex = 0;
            runningIndex = waveIndex;

            StopCoroutine(GameLoopRoutine);
        }
    }

    private void StartGameLoop()
    {
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        GameLoopRoutine = StartCoroutine(SpawnFlyAtRandomPosition());
    }

    // check game start to determine powerup
    private void WhatPowerUp(int waveIndex)
    {
        StoreManager.Instance.ShowStore();
        // switch (waveIndex)
        // {
        //     case (int)POWERUP.FROG:
        //         FrogPowerUp.RaiseEvent();
        //         break;
        //     case (int)POWERUP.SPRAY:
        //         SprayPowerUp.RaiseEvent();
        //         break;
        //     case (int)POWERUP.SWATTER:
        //         ElectricSwatterPowerUp.RaiseEvent();
        //         break;
        //     case (int)POWERUP.UPGRADE:
        //         UpgradePowerUps.RaiseEvent();
        //         break;
        //     case (int)POWERUP.BOSS:
        //         BossFightEvent.RaiseEvent();
        //         break;
        // }
    }


    public void RestartGameLoop()
    {
        settings.flies.Clear();
        canSpawn = true;
        animator.Play("Animation", 0, 0);
        //animator.speed = settings.divFactor / settings.durationOfWave[0];

        GameLoopRoutine = StartCoroutine(SpawnFlyAtRandomPosition());
        waveIndex = 0;
        settings.numberOfKills = 0;
        settings.Cash = 0;
        //initialTime = 0;
        UIManager.Instance.FailedPanel(false, 0, 0);
        UIManager.Instance.UpdateLevel();

    }

    // this event has been removed from the MRUK event call 
    public void StartGame()
    {
        GetWindowOrDoorFrames(MRUK.Instance.GetCurrentRoom());
    }

    // 
    bool doneOnce = false;

    public void GetWindowOrDoorFrames(MRUKRoom room)
    {
        foreach (var anchor in room.Anchors)
        {
            // handling only door and window points
            if (anchor.HasLabel("WINDOW_FRAME") || anchor.HasLabel("DOOR_FRAME"))
            {
                FlySpawnPositions.Add(anchor);
                /*if (!doneOnce)
                 {
                     Instantiate(Portal, anchor.transform.position, Portal.transform.rotation);
                     doneOnce = true;
                 } */

            }
            else
            {
                if (anchor.HasLabel("CEILING") || anchor.HasLabel("FLOOR") || anchor.HasLabel("WALL_FACE"))
                {
                    FlySpawnPositions.Add(anchor);
                }
            }

            // place hourglass on table
            if (anchor.HasLabel("TABLE"))
            {
                if (!doneOnce)
                {
                    HourGlass.transform.position = anchor.transform.position;
                    HourGlass.transform.forward = anchor.transform.right;
                    doneOnce = true;
                }
            }
            else
            {
                if (anchor.HasLabel("FLOOR"))
                {
                    if (!doneOnce)
                    {
                        HourGlass.transform.position = anchor.transform.position;
                        HourGlass.transform.forward = anchor.transform.up;

                        doneOnce = true;
                    }
                }
            }
        }
    }
}
