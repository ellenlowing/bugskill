using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class PlayerSettingsManager : MonoBehaviour
{
    public static PlayerSettingsManager Instance;

    [Header("Move Game UI Anchor")]
    public GameObject GameUIGroup;
    public bool IsGameUIMovementEnabled = false;

    [Header("Adjust Depth Bias")]
    public GameObject DepthTestFlyPrefab;
    public int DepthTestFlyCount = 10;
    public float EnvironmentDepthBias = 0;
    public List<GameObject> OccludedPrefabs;

    private List<GameObject> _depthTestFlyList = new List<GameObject>();
    private int _wallLayerMask;

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

    void Start()
    {
        GameUIGroup = GameManager.Instance.GameUIGroup;
        _wallLayerMask = GameManager.Instance.GetWallLayerMask();
    }

    void OnEnable()
    {
        GameUIGroup.SetActive(IsGameUIMovementEnabled);
        CreateDepthTestEnvironment();
    }

    void OnDisable()
    {
        GameUIGroup.SetActive(false);
        DestroyDepthTestEnvironment();
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

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            CreateDepthTestEnvironment();
        }

        if (IsGameUIMovementEnabled)
        {
            MoveGameUI();
        }
    }

    /* SETTING: MOVE GAME UI */
    public void MoveGameUI()
    {
        if (GameManager.Instance.RightHand.GetIndexFingerIsPinching())
        {
            RaycastHit hit;
            GameManager.Instance.RightHand.GetPointerPose(out Pose pointerPose);

            if (Physics.Raycast(pointerPose.position, pointerPose.forward.normalized, out hit, Mathf.Infinity, _wallLayerMask))
            {
                GameUIGroup.transform.position = hit.point;
                GameUIGroup.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
    }

    public void ToggleGameUIMovement()
    {
        IsGameUIMovementEnabled = !IsGameUIMovementEnabled;
        GameUIGroup.SetActive(IsGameUIMovementEnabled);
    }

    /* SETTING: ADJUST DEPTH BIAS */
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
        SamplePointsOnSceneModel();
    }

    public void DestroyDepthTestEnvironment()
    {
        foreach (var depthTestFly in _depthTestFlyList)
        {
            Destroy(depthTestFly);
        }
        _depthTestFlyList.Clear();
    }

    public void SamplePointsOnSceneModel()
    {
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

    public void SamplePointsOnGlobalMesh()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        MRUKAnchor globalMeshAnchor = currentRoom.GlobalMeshAnchor;
        Mesh globalMesh = globalMeshAnchor.GlobalMesh;

        for (int i = 0; i < DepthTestFlyCount; i++)
        {
            (Vector3 point, Vector3 normal) = GetRandomPointOnMesh(globalMesh, globalMeshAnchor.transform);
            GameObject fly = Instantiate(DepthTestFlyPrefab, point, Quaternion.identity, gameObject.transform);
            fly.transform.up = normal;
            fly.transform.rotation = fly.transform.rotation * Quaternion.Euler(0, Random.Range(0, 360f), 0);
            fly.name = "Depth Test Fly " + i.ToString();
            _depthTestFlyList.Add(fly);
            Debug.Log("Depth Test Fly " + i.ToString() + " created at " + point.ToString());
        }
    }

    public static (Vector3 point, Vector3 normal) GetRandomPointOnMesh(Mesh mesh, Transform transform)
    {
        // Get mesh data
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Calculate areas of all triangles
        float[] cumulativeAreas = new float[triangles.Length / 3];
        float totalArea = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            // Calculate area of the triangle
            float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
            totalArea += area;
            cumulativeAreas[i / 3] = totalArea;
        }

        // Select a triangle based on areas
        float randomValue = Random.value * totalArea;
        int triangleIndex = System.Array.BinarySearch(cumulativeAreas, randomValue);
        if (triangleIndex < 0)
            triangleIndex = ~triangleIndex;

        // Get triangle vertices
        Vector3 a = vertices[triangles[triangleIndex * 3]];
        Vector3 b = vertices[triangles[triangleIndex * 3 + 1]];
        Vector3 c = vertices[triangles[triangleIndex * 3 + 2]];

        // Calculate triangle normal dynamically
        Vector3 dynamicNormal = Vector3.Cross(b - a, c - a).normalized;

        // Random barycentric coordinates
        float u = Random.value;
        float v = Random.value;
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }
        float w = 1f - u - v;

        // Convert to world space
        Vector3 localPoint = a * u + b * v + c * w;

        return (transform.TransformPoint(localPoint), transform.TransformDirection(dynamicNormal));
    }
}
