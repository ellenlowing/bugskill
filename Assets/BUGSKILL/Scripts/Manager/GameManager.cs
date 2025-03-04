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
    public Transform MainCameraTransform;

    [Header("Game Settings")]
    public SettingSO settings;
    public bool TestMode;
    public int StartWaveIndex = 0;
    [SerializeField] private SettingSO gameSettings;
    [SerializeField] private SettingSO testSettings;
    public string LandingLayerName = "Landing";
    public string FloorLayerName = "Floor";
    public string WallLayerName = "Wall";

    [Header("Flies")]
    public GameObject FlyPrefab;
    public Transform FlyParentAnchor;
    public List<GameObject> BloodSplatterPrefabs;
    public Transform BloodSplatContainer;
    public MRUKAnchor.SceneLabels SpawnAnchorLabels;
    public LayerMask FlyLayerMask;

    [Header("Power Up")]
    public float DissolveDuration = 1.5f;

    [Header("TNT Stuff")]
    public GameObject TNTFlyPrefab;
    public ParticleSystem TNTExplosion;
    public float TNTEffectTimeout = 5f;
    public float TNTExplosionRadius = 1.5f;
    private Stack<GameObject> TNTFlies = new Stack<GameObject>();

    [Header("MRUK")]
    public List<MRUKAnchor> ValidAnchors;
    public int ValidAnchorIndex = 0;

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
    public HandController LeftHandController;
    public HandController RightHandController;
    public GameObject LeftHandVisuals;
    public GameObject RightHandVisuals;
    public GameObject LeftHandRayInteractor;
    public GameObject RightHandRayInteractor;
    public LayerMask HandsLayerMask;

    [Header("Hand Color")]
    public SkinnedMeshRenderer RightHandMeshRenderer;
    public Color StartHandColor;
    public Color EndHandColor;

    [Header("Finger Gun")]
    public GameObject LeftFingerGun;
    public GameObject RightFingerGun;

    [Header("Finger Slingshot")]
    public GameObject LeftFingerSlingshot;
    public GameObject RightFingerSlingshot;

    [Header("Others")]
    public AudioSource RoundEndAudio;

    // private bool doneOnce = false;

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

        settings.waveIndex = StartWaveIndex;
        settings.totalKills = 0;
        settings.localKills = 0;
        if (!TestMode) settings.Cash = 0;
        settings.flies = new List<GameObject>();
    }

    void Start()
    {
        GameRestartEvent.WhenSelect.AddListener(RestartGameLoop);

        animator.speed = 0;
        GameUIGroup.SetActive(false);
        LevelPanel.SetActive(false);
        UIManager.Instance.GameEndUI.SetActive(false);
        StoreManager.Instance.HideStore();
        SetHandControllersActive(false);
        SetHandVisualsActive(true);
        RightHandRenderer.SetActive(false);
        LeftHandRenderer.SetActive(false); 

// #if UNITY_EDITOR
//         EffectMesh.HideMesh = false;
// #endif

        if (debugObject != null)
        {
            UIManager.Instance.FaceCamera(debugObject, -0.3f);
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            settings.localKills += 1;
        }
#endif
    }

    void OnApplicationQuit()
    {
        settings.waveIndex = StartWaveIndex;
        settings.totalKills = 0;
        settings.localKills = 0;
        if (!TestMode) settings.Cash = 0;
    }

    void InitializeRound()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();

        if (currentRoom != null)
        {
            int spawnedFlies = 0;
            while (spawnedFlies < settings.fliesInWave[settings.waveIndex])
            {
                Vector3? position = currentRoom.GenerateRandomPositionInRoom(0.05f, true);
                if (position.HasValue)
                {
                    GameObject fly = Instantiate(FlyPrefab, (Vector3)position, Quaternion.identity, FlyParentAnchor);
                    fly.name = "Fly " + spawnedFlies.ToString();
                    settings.flies.Add(fly);
                    spawnedFlies++;
                }
            }
        }

        GameUIGroup.SetActive(true);
        LevelPanel.SetActive(true);
        animator.speed = settings.divFactor / settings.durationOfWave[settings.waveIndex];
        animator.Play("Animation", 0, 0);
        // UpdateHandMaterialColor();
        StartCoroutine(SetTimer(settings.durationOfWave[settings.waveIndex]));
    }

    void HandleRoundEnd()
    {
        RoundEndAudio.Play();
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
        StartCoroutine(CheckGoal(settings.waveIndex));

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
        GameEnds.OnEventRaised += AudioManager.Instance.CueEnding;
    }

    public void StartNextWave()
    {
        StoreManager.Instance.HideStore();
        AudioManager.Instance.CueGame();
        Invoke(nameof(InitializeRound), UIManager.Instance.RoundStartUIDuration);
    }

    IEnumerator CheckGoal(int waveI)
    {
        // SetHandVisualsActive(true);
        int powerUpCount = DissolveAllPowerUps();
        yield return null;
        // yield return new WaitForSeconds(powerUpCount > 0 ? DissolveDuration : 0.1f);
        bool goalReached = settings.localKills >= settings.LevelGoals[waveI];
        Debug.Log(goalReached + " for wave index " + waveI + ", Local Kills: " + settings.localKills + " Goal: " + settings.LevelGoals[waveI]);
        settings.totalKills += settings.localKills;
        settings.localKills = 0;

        if (!goalReached)
        {
            UIManager.Instance.ShowGameEndPanel(false);
            GameEnds.RaiseEvent();
        }
        else if (waveI == settings.LevelGoals.Length - 1)
        {
            Debug.Log("Game Ends. You WINNNN!");
            UIManager.Instance.ShowGameEndPanel(true);
            GameEnds.RaiseEvent();
        }
        else
        {
            StoreManager.Instance.ShowStore();
        }
    }

    private void StartGameLoop()
    {
        // SetHandVisualsActive(true);
        SetHandControllersActive(true);
        StoreManager.Instance.HideStore();
        UIManager.Instance.UpdateCashUI();
        AudioManager.Instance.CueGame();
        Invoke(nameof(InitializeRound), UIManager.Instance.RoundStartUIDuration);
    }

    public void RestartGameLoop()
    {
        // SetHandVisualsActive(true);
        settings.flies.Clear();
        settings.waveIndex = StartWaveIndex;
        settings.totalKills = 0;
        settings.localKills = 0;
        settings.Cash = 0;

        UIManager.Instance.GameEndUI.SetActive(false);
        UIManager.Instance.UpdateLevel();
        UIManager.Instance.UpdateCashUI();
        StartGameLoop();
        // StoreManager.Instance.HideAllPowerUps();
    }

    public void TriggerTNT(Vector3 position, GameObject tntFly = null)
    {
        TNTExplosion.transform.position = position;
        TNTExplosion.Stop();
        TNTExplosion.Play();
        TNTExplosion.gameObject.GetComponent<AudioSource>().Play();

        Collider[] hitFlies = Physics.OverlapSphere(position, TNTExplosionRadius, FlyLayerMask);
        foreach (var fly in hitFlies)
        {
            BaseFlyBehavior flyBehavior = fly.GetComponent<BaseFlyBehavior>();
            if (fly.GetComponent<BaseFlyBehavior>().Type == BaseFlyBehavior.FlyType.REGULAR && !flyBehavior.IsKilled)
            {
                UIManager.Instance.IncrementKill(fly.transform.position, (int)SCOREFACTOR.TNT);
                flyBehavior.Kill();
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

    public void PlaceGameObjectOnCeiling(GameObject obj)
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room != null)
        {
            var ceiling = room.CeilingAnchor;
            obj.transform.position = ceiling.transform.position;
            obj.transform.rotation = ceiling.transform.rotation;
        }
    }

    public void PlaceGameUIOnKeyWall()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        var wall = room.GetKeyWall(out Vector2 wallScale);
        GameUIGroup.transform.position = wall.transform.position - new Vector3(0, wallScale.y / 5, 0);
        GameUIGroup.transform.forward = wall.transform.forward;
        Debug.Log("Place on key wall");
    }

    public int DissolveAllPowerUps()
    {
        Debug.Log("StoreManager: Dissolving all powerups");
        var powerup = StoreManager.Instance._selectedPowerUp;
        if (powerup != null)
        {
            Debug.Log("dissolving " + powerup.name);
            powerup.Dissolve();
            return 1;
        }
        else
        {
            return 0;
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

    public void SetHandControllersActive(bool active)
    {
        LeftHandController.enabled = active;
        RightHandController.enabled = active;
    }

    public void SetHandVisualsActive(bool active)
    {
        LeftHandVisuals.SetActive(active);
        RightHandVisuals.SetActive(active);
        LeftHandRayInteractor.SetActive(active);
        RightHandRayInteractor.SetActive(active);
    }

    public void UpdateHandMaterialColor()
    {
        float killPercentage = (float)settings.localKills / (float)settings.LevelGoals[settings.waveIndex];
        Color color = Color.Lerp(StartHandColor, EndHandColor, killPercentage);
        RightHandMeshRenderer.sharedMaterial.SetColor("_ColorTop", color);
    }

    public bool IsOnAnyLandingLayer(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer(LandingLayerName) || obj.layer == LayerMask.NameToLayer(FloorLayerName) || obj.layer == LayerMask.NameToLayer(WallLayerName);
    }

    public int GetAnyLandingLayerMask()
    {
        return (1 << LayerMask.NameToLayer(LandingLayerName)) | (1 << LayerMask.NameToLayer(FloorLayerName)) | (1 << LayerMask.NameToLayer(WallLayerName));
    }

    public int GetWallLayerMask()
    {
        return 1 << LayerMask.NameToLayer(WallLayerName);
    }
}
