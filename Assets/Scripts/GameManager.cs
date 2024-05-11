using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<MRUKAnchor> FlySpawnPositions;
    public GameObject FlyPrefab;
    public SettingSO settings;
    public List<GameObject> BloodSplatterPrefabs;
    public GameObject splatterParticle;
    public Transform FlyParentAnchor;

    // ---
    public GameObject Portal;

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
        GetWindowOrDoorFrames(MRUK.Instance.GetCurrentRoom());
    }

    void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.One))
        {

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
                int randomIndex = Random.Range(0, FlySpawnPositions.Count);
                MRUKAnchor randomAnchor = FlySpawnPositions[randomIndex];
                Vector3 randomPosition = randomAnchor.GetAnchorCenter();
                if (randomAnchor.PlaneRect.HasValue)
                {
                    Vector2 size = randomAnchor.PlaneRect.Value.size;
                    randomPosition += new Vector3(Random.Range(-size.x / 2, size.x / 2), 0, Random.Range(-size.y / 2, size.y / 2));
                }

                // keep reference to all spawned flies
                settings.flies.Add(Instantiate(FlyPrefab, randomPosition, Quaternion.identity,FlyParentAnchor));

                yield return new WaitForSeconds(Random.Range(settings.flySpawnIntervalMin, settings.flySpawnIntervalMax));
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
            if (anchor.HasLabel("WINDOW_FRAME") || anchor.HasLabel("DOOR_FRAME"))
            {
                FlySpawnPositions.Add(anchor);
                if (!doneOnce)
                {
                    Instantiate(Portal, anchor.transform.position, Portal.transform.rotation);
                    doneOnce = true;
                }

            }
        }
    }
}
