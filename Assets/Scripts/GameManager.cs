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
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using Unity.VisualScripting;
using Oculus.Interaction.Input.Filter;

public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public EffectMesh EffectMesh;

    public GameObject debugObject;

    [Header("Scene Objects")]
    public Animator animator;
    public GameObject GameUIGroup;
    public GameObject LevelPanel;
    public TutorialVideoPlayer TutorialVideoPlayer;

    [Header("Game Settings")]
    public SettingSO settings;
    public bool TestMode;
    [SerializeField] private SettingSO gameSettings;
    [SerializeField] private SettingSO testSettings;
    public string LandingLayerName = "Landing";
    public string FloorLayerName = "Floor";

    [Header("Flies")]
    public GameObject FlyPrefab;
    public Transform FlyParentAnchor;
    public List<GameObject> BloodSplatterPrefabs;
    public Transform BloodSplatContainer;
    public MRUKAnchor.SceneLabels SpawnAnchorLabels;
    public LayerMask FlyLayerMask;

    [Header("TNT Stuff")]
    public GameObject TNTFlyPrefab;
    public ParticleSystem TNTExplosion;
    public float TNTEffectTimeout = 5f;
    public float TNTExplosionRadius = 1.5f;
    private Stack<GameObject> TNTFlies = new Stack<GameObject>();

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
    public OVRHand LeftOVRHand;
    public OVRHand RightOVRHand;
    public GameObject LeftHandRenderer;
    public GameObject RightHandRenderer;
    public Hand LeftHand;
    public Hand RightHand;

    [Header("Finger Gun")]
    public GameObject LeftFingerGun;
    public GameObject RightFingerGun;

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
        settings.totalKills = 0;
        settings.localKills = 0;
        if (!TestMode) settings.Cash = 0;
        settings.flies = new List<GameObject>();
    }

    void Start()
    {
        GameRestartEvent.WhenSelect.AddListener(RestartGameLoop);

        GameUIGroup.SetActive(false);
        LevelPanel.SetActive(false);
        UIManager.Instance.FailedPanel(false);

#if UNITY_EDITOR
        EffectMesh.HideMesh = false;
#endif

        UIManager.Instance.FaceCamera(debugObject, -0.3f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            UIManager.Instance.FaceCamera(debugObject);
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
                    fly.name = "Fly " + i.ToString();
                    settings.flies.Add(fly);
                }
            }

            TNTFlies.Clear();
            for (int i = 0; i < settings.tntFliesInWave[settings.waveIndex]; i++)
            {
                if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL, 0.01f, labelFilter, out Vector3 position, out Vector3 normal))
                {
                    GameObject fly = Instantiate(TNTFlyPrefab, position, Quaternion.identity, FlyParentAnchor);
                    fly.transform.up = normal;
                    fly.transform.rotation = fly.transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    fly.name = "TNT Fly " + i.ToString();
                    settings.flies.Add(fly);

                    TNTFlies.Push(fly);
                }
            }
        }

        GameUIGroup.SetActive(true);
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

        for (int i = BloodSplatContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(BloodSplatContainer.GetChild(i).gameObject);
        }

        GameUIGroup.SetActive(false);
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
        GameEnds.OnEventRaised += DissolveAllPowerUps;
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
        bool goalReached = settings.localKills >= settings.LevelGoals[waveI];
        Debug.Log(goalReached + " for wave index " + waveI + ", Local Kills: " + settings.localKills + " Goal: " + settings.LevelGoals[waveI]);
        settings.totalKills += settings.localKills;
        settings.localKills = 0;

        if (!goalReached)
        {
            UIManager.Instance.FailedPanel(true);
            GameEnds.RaiseEvent();
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
        settings.totalKills = 0;
        settings.localKills = 0;
        settings.Cash = 0;
        UIManager.Instance.FailedPanel(false);
        UIManager.Instance.UpdateLevel();
        UIManager.Instance.UpdateCashUI();
        // StoreManager.Instance.HideAllPowerUps();
    }

    public void TriggerTNT(Vector3 position, GameObject tntFly)
    {
        TNTExplosion.transform.position = position;
        TNTExplosion.Stop();
        TNTExplosion.Play();
        TNTExplosion.gameObject.GetComponent<AudioSource>().Play();
        Destroy(tntFly);

        Collider[] hitFlies = Physics.OverlapSphere(position, TNTExplosionRadius, FlyLayerMask);
        foreach (var fly in hitFlies)
        {
            if (fly.GetComponent<BaseFlyBehavior>().Type == BaseFlyBehavior.FlyType.REGULAR)
            {
                UIManager.Instance.IncrementKill(fly.transform.position, (int)SCOREFACTOR.TNT);
                Destroy(fly.gameObject);
            }
        }
        UIManager.Instance.UpdateCashUI();
    }

    public void PlaceGameObjectOnFloor(GameObject obj)
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room != null)
        {
            var floor = room.FloorAnchor;
            obj.transform.position = floor.transform.position;
            obj.transform.up = floor.transform.forward;
        }
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

    public void PlaceGameUIGroup()
    {
        Debug.Log("starting to place gameUIgroup");
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        foreach (var anchor in room.Anchors)
        {
            // place hourglass on table
            if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.TABLE))
            {
                if (!doneOnce)
                {
                    GameUIGroup.transform.position = anchor.transform.position;
                    doneOnce = true;
                    Debug.Log("Place on table");
                    break;
                }
            }
            else
            {
                if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
                {
                    if (!doneOnce)
                    {
                        GameUIGroup.transform.position = anchor.transform.position;
                        GameUIGroup.transform.forward = anchor.transform.up;
                        doneOnce = true;
                        Debug.Log("Place on floor");
                        break;
                    }
                }
            }
        }

        if (!doneOnce)
        {
            var wall = room.GetKeyWall(out Vector2 wallScale);
            GameUIGroup.transform.position = wall.transform.position - new Vector3(0, wallScale.y / 2, 0);
            GameUIGroup.transform.forward = -wall.transform.forward;
            Debug.Log("Place on key wall");
        }

        GameUIGroup.transform.LookAt(room.FloorAnchor.transform);
        GameUIGroup.transform.eulerAngles = new Vector3(0, GameUIGroup.transform.eulerAngles.y, 0);
    }

    public void DissolveAllPowerUps()
    {
        Debug.Log("StoreManager: Dissolving all powerups");
        var powerUps = FindObjectsOfType<BasePowerUpBehavior>();
        foreach (var powerUp in powerUps)
        {
            powerUp.Dissolve();
        }
    }

    void OnDrawGizmos()
    {
        foreach (var fly in TNTFlies)
        {
            if (fly != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(fly.transform.position, TNTExplosionRadius);
            }
        }
    }
}
