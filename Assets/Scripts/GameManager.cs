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
using Unity.VisualScripting;

public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public EffectMesh EffectMesh;

    [Header("Scene Objects")]
    public Animator animator;
    public GameObject HourGlass;
    public GameObject LevelPanel;

    [Header("Game Settings")]
    public SettingSO settings;
    public bool TestMode;
    [SerializeField] private SettingSO gameSettings;
    [SerializeField] private SettingSO testSettings;

    [Header("Flies")]
    public GameObject FlyPrefab;
    public Transform FlyParentAnchor;
    public List<GameObject> BloodSplatterPrefabs;
    public Transform BloodSplatContainer;
    public MRUKAnchor.SceneLabels SpawnAnchorLabels;

    [Header("TNT Stuff")]
    public GameObject TNTFlyPrefab;
    public ParticleSystem TNTExplosion;
    public bool IsTNTTriggered = false;
    public float TNTEffectTimeout = 5f;

    [Header("Game Events")]
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

    private int LocalKills = 0;
    private int LocalCash = 0;
    private bool doneOnce = false;

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

        if (TestMode)
        {
            settings = testSettings;
        }
        else
        {
            settings = gameSettings;
        }

        settings.waveIndex = 0;
        settings.flies = new List<GameObject>();
    }

    void Start()
    {
        settings.numberOfKills = 0;
        GameRestartEvent.WhenSelect.AddListener(RestartGameLoop);

        HourGlass.SetActive(false);
        LevelPanel.SetActive(false);

#if UNITY_EDITOR
        EffectMesh.HideMesh = false;
#endif
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceLevelPanel();
        }
    }

    void InitializeRound()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        var labelFilter = LabelFilter.Included(SpawnAnchorLabels);

        if (currentRoom != null)
        {
            for (int i = 0; i < settings.fliesInWave[settings.waveIndex]; i++)
            {
                if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 0.01f, labelFilter, out Vector3 position, out Vector3 normal))
                {
                    GameObject fly = Instantiate(FlyPrefab, position, Quaternion.identity, FlyParentAnchor);
                    fly.transform.up = normal;
                    fly.transform.rotation = fly.transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    settings.flies.Add(fly);
                }
            }

            for (int i = 0; i < settings.tntFliesInWave[settings.waveIndex]; i++)
            {
                if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 0.01f, labelFilter, out Vector3 position, out Vector3 normal))
                {
                    GameObject fly = Instantiate(TNTFlyPrefab, position, Quaternion.identity, FlyParentAnchor);
                    fly.transform.up = normal;
                    fly.transform.rotation = fly.transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    settings.flies.Add(fly);
                }
            }
        }

        HourGlass.SetActive(true);
        LevelPanel.SetActive(true);
        animator.speed = settings.divFactor / settings.durationOfWave[settings.waveIndex];

        StartCoroutine(SetTimer(settings.durationOfWave[settings.waveIndex]));
    }

    void HandleRoundEnd()
    {
        foreach (var obj in settings.flies)
        {
            Destroy(obj);
        }
        settings.flies.Clear();
        settings.flies = new List<GameObject>();

        LocalKills = settings.numberOfKills - LocalKills;
        LocalCash = settings.Cash - LocalCash;

        for (int i = BloodSplatContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(BloodSplatContainer.GetChild(i).gameObject);
        }

        HourGlass.SetActive(false);
        LevelPanel.SetActive(false);
        CheckGoal(settings.waveIndex);

        settings.waveIndex = settings.waveIndex + 1;

        animator.speed = 1f;

        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            animator.speed = 0;
        }
    }

    IEnumerator SetTimer(float time)
    {
        yield return new WaitForSeconds(time);
        HandleRoundEnd();
    }

    private void OnEnable()
    {
        StartNextWaveEvent.OnEventRaised += StartNextWave;
        GameBegins.OnEventRaised += StartGameLoop;
    }

    public void StartNextWave()
    {
        animator.speed = settings.divFactor / settings.durationOfWave[settings.waveIndex];
        animator.Play("Animation", 0, 0);
        InitializeRound();
        StoreManager.Instance.HideStore();
    }

    private void CheckGoal(int waveI)
    {
        Debug.Log("CHECK: " + settings.Cash + " " + settings.LevelGoals[waveI] + " " + waveI);
        if (!(settings.Cash >= settings.LevelGoals[waveI]))
        {
            UIManager.Instance.FailedPanel(true, LocalCash, settings.waveIndex);
            settings.waveIndex = 0;
        }
        else
        {
            StoreManager.Instance.ShowStore();
        }
    }

    private void StartGameLoop()
    {
        InitializeRound();
    }

    public void RestartGameLoop()
    {
        settings.flies.Clear();
        animator.Play("Animation", 0, 0);

        InitializeRound();
        settings.waveIndex = 0;
        settings.numberOfKills = 0;
        settings.Cash = 0;
        UIManager.Instance.FailedPanel(false, 0, 0);
        UIManager.Instance.UpdateLevel();
        UIManager.Instance.UpdateCashUI();
        StoreManager.Instance.HideAllPowerUps();
    }

    public void TriggerTNT(Vector3 position, GameObject itemToDestroy)
    {
        StartCoroutine(TriggerTNTCoroutine(position, itemToDestroy));

        foreach (GameObject fly in settings.flies)
        {
            fly.GetComponent<FlyMovement>().GoInsane();
        }
    }

    IEnumerator TriggerTNTCoroutine(Vector3 position, GameObject itemToDestroy)
    {
        IsTNTTriggered = true;
        TNTExplosion.transform.position = position;
        TNTExplosion.Stop();
        TNTExplosion.Play();
        TNTExplosion.gameObject.GetComponent<AudioSource>().Play();
        Destroy(itemToDestroy);

        yield return new WaitForSeconds(TNTEffectTimeout);
        IsTNTTriggered = false;
    }

    public void PlaceLevelPanel()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room != null)
        {
            var wall = room.GetKeyWall(out Vector2 wallScale);
            LevelPanel.transform.position = wall.transform.position + new Vector3(0, wallScale.y / 2, 0);
            LevelPanel.transform.forward = -wall.transform.forward;
        }
    }

    public void PlaceHourglass()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        foreach (var anchor in room.Anchors)
        {
            // place hourglass on table
            if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE))
            {
                if (!doneOnce)
                {
                    HourGlass.transform.position = anchor.transform.position;
                    HourGlass.transform.forward = -anchor.transform.right;
                    doneOnce = true;
                }
            }
            else
            {
                if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
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

        if (!doneOnce)
        {
            var wall = room.GetKeyWall(out Vector2 wallScale);
            HourGlass.transform.position = wall.transform.position - new Vector3(0, wallScale.y / 2, 0);
            HourGlass.transform.forward = -wall.transform.forward;
        }
    }
}
