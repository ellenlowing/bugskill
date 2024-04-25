using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<MRUKAnchor> FlySpawnPositions;
    public GameObject FlyPrefab;
    public float FlySpawnIntervalMin = 5.0f;
    public float FlySpawnIntervalMax = 15.0f;
    public List<GameObject> BloodSplatterPrefabs;

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

    }

    void Update()
    {

    }

    IEnumerator SpawnFlyAtRandomPosition()
    {
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

                Instantiate(FlyPrefab, randomPosition, Quaternion.identity);

                yield return new WaitForSeconds(Random.Range(FlySpawnIntervalMin, FlySpawnIntervalMax));
            }
        }
    }

    public void StartGame()
    {
        foreach (var room in MRUK.Instance.Rooms)
        {
            GetWindowOrDoorFrames(room);
        }

        StartCoroutine(SpawnFlyAtRandomPosition());
    }

    public void GetWindowOrDoorFrames(MRUKRoom room)
    {
        foreach (var anchor in room.Anchors)
        {
            if (anchor.HasLabel("WINDOW_FRAME") || anchor.HasLabel("DOOR_FRAME"))
            {
                FlySpawnPositions.Add(anchor);
            }
        }
    }
}
