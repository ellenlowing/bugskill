using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class PlayerSettingsManager : MonoBehaviour
{
    public static PlayerSettingsManager Instance;
    public GameObject DepthTestFlyPrefab;
    public int DepthTestFlyCount = 10;
    public float EnvironmentDepthBias = 0;
    private List<GameObject> _depthTestFlyList = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y > 0.5f)
        {
            AdjustDepthBias(0.01f);
        }
        else if (OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y < -0.5f)
        {
            AdjustDepthBias(-0.01f);
        }
    }

    public void AdjustDepthBias(float value)
    {
        EnvironmentDepthBias += value;
        foreach (var depthTestFly in _depthTestFlyList)
        {
            depthTestFly.GetComponent<DepthTestFlyBehavior>().SetDepthBias(EnvironmentDepthBias);
        }
        Debug.Log("New depth bias: " + EnvironmentDepthBias);
    }

    public void CreateDepthTestEnvironment()
    {
        GameManager.Instance.SetHandControllersActive(false);

        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        var labelFilter = LabelFilter.Included(GameManager.Instance.SpawnAnchorLabels);

        for (int i = 0; i < DepthTestFlyCount; i++)
        {
            if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.VERTICAL | MRUK.SurfaceType.FACING_UP, 0.01f, labelFilter, out Vector3 position, out Vector3 normal))
            {
                GameObject fly = Instantiate(DepthTestFlyPrefab, position, Quaternion.identity, gameObject.transform);
                fly.transform.up = normal;
                fly.transform.rotation = fly.transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
                fly.name = "Depth Test Fly " + i.ToString();
                _depthTestFlyList.Add(fly);
                Debug.Log("Depth Test Fly " + i.ToString() + " created at " + position.ToString());
            }
        }
    }

    public void DestroyDepthTestEnvironment()
    {
        foreach (var depthTestFly in _depthTestFlyList)
        {
            Destroy(depthTestFly);
        }
        _depthTestFlyList.Clear();
    }
}
