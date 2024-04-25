using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class EnvironmentSetup : MonoBehaviour
{
    public List<MeshCollider> landingSurfaceMeshColliders = new List<MeshCollider>();
    public int landingSurfaceLayerNum = 6;

    void Start()
    {

    }

    void Update()
    {

    }

    public void AddLandingSurfaceLayer()
    {
        foreach (var room in MRUK.Instance.Rooms)
        {
            AddLandingSurfaceLayer(room);
        }
    }

    public void AddLandingSurfaceLayer(MRUKRoom room)
    {
        var walls = room.WallAnchors;
        foreach (var wall in walls)
        {
            AddLandingSurfaceColliders(wall);
        }

        AddLandingSurfaceColliders(room.CeilingAnchor);
        AddLandingSurfaceColliders(room.FloorAnchor);

    }

    public void AddLandingSurfaceColliders(MRUKAnchor anchor)
    {
        var colliders = anchor.GetComponentsInChildren<MeshCollider>();
        foreach (var collider in colliders)
        {
            landingSurfaceMeshColliders.Add(collider);
            collider.gameObject.layer = landingSurfaceLayerNum;
            collider.convex = true;
        }
    }
}
