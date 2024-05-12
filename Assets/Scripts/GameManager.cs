using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
    

    // ---
    public GameObject Portal;
    private int waveIndex = 0;
    private bool canSpawn = true;


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

        FlySpawnPositions = new List<MRUKAnchor>();
    }

    void Start()
    {
        // check references
        UIM = GetComponent<UIManager>();
        Assert.IsNotNull(UIM, "UIManager Reference Missing");

        GetWindowOrDoorFrames(MRUK.Instance.GetCurrentRoom());
    }

    private bool moveToNextWave = false;
    private float initialTime = 0;

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
            if (initialTime > settings.maxWaitTime[waveIndex])
            {
                // empty all fly list
                foreach (var obj in settings.flies)
                {
                    Destroy(obj);
                }
                settings.flies.Clear();
                initialTime = 0;

                // hide hourglass before next countdown
                HourGlass.SetActive(false);
            }
            initialTime += Time.deltaTime;
        }
    }

    IEnumerator SpawnFlyAtRandomPosition()
    {
        if (FlySpawnPositions.Count == 0)
        {
            yield break;
        }

        while (true)
        {
            if (FlySpawnPositions.Count > 0)
            {

                // loop here with wave count which changes
                // destroy all current flies before next wave
                // before next wave, wait for certain amount of time
                if (canSpawn)
                {
                    for (int i = 0; i < settings.Waves[waveIndex]; i++)
                    {
                        int randomIndex = Random.Range(0, FlySpawnPositions.Count);
                        MRUKAnchor randomAnchor = FlySpawnPositions[randomIndex];
                        Vector3 randomPosition = randomAnchor.GetAnchorCenter();
                        if (randomAnchor.PlaneRect.HasValue)
                        {
                            Vector2 size = randomAnchor.PlaneRect.Value.size;
                            randomPosition += new Vector3(Random.Range(-size.x / 2, size.x / 2), 0, Random.Range(-size.y / 2, size.y / 2));
                        }

                        // keep reference to all spawned flies
                        // spawn wave number through loop which uses settings factor
                        settings.flies.Add(Instantiate(FlyPrefab, randomPosition, Quaternion.identity, FlyParentAnchor));

                    }
                    canSpawn = false;
                    moveToNextWave = true;   
                    

                    // enable and set timescale for loading based on time anticapated per wave
                    HourGlass.SetActive(true);
                    animator.speed = settings.divFactor / settings.maxWaitTime[waveIndex];
                }
                

                // check if all flies are killed
                // move to next wave count
                // play theme wave wait sound
                if(settings.flies.Count == 0)
                {
                    waveIndex++;
                    moveToNextWave = false;
                    canSpawn = true;
                    // enable hour glass here
                    // set the animation speed to scale with div factor
                    HourGlass.SetActive(true);

                    animator.speed = settings.divFactor / settings.WaveWaitTime;
                   // animator.speed = 0.02f;
                    yield return new WaitForSeconds(settings.WaveWaitTime);
                    HourGlass.SetActive(false);
                }

                if(waveIndex >= settings.Waves.Length)
                {
                    // call completion here with ui score update
                    UIM.KillUpdate();
                    yield break;
                }

                // 2 second frame checks
                yield return new WaitForSeconds(2.0f);
                // yield return new WaitForSeconds(Random.Range(settings.flySpawnIntervalMin, settings.flySpawnIntervalMax));
            }
        }
    }

    public void StartGame()
    {
        GetWindowOrDoorFrames(MRUK.Instance.GetCurrentRoom());
        StartCoroutine(SpawnFlyAtRandomPosition());
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
               // if (!doneOnce)
               // {
               //     Instantiate(Portal, anchor.transform.position, Portal.transform.rotation);
               //     doneOnce = true;
               // }

            }

            // place hourglass on table
            if (anchor.HasLabel("TABLE"))
            {
                if (!doneOnce)
                {
                    HourGlass.transform.position = anchor.transform.position + new Vector3(0f, 0.3f, 0f);
                    doneOnce = true;
                }
            }
        }
    }
}
