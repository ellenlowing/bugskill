using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class EnvironmentSetup : MonoBehaviour
{
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
            wall.GetComponentInChildren<Collider>().gameObject.layer = landingSurfaceLayerNum;
        }
        room.CeilingAnchor.GetComponentInChildren<Collider>().gameObject.layer = landingSurfaceLayerNum;
        room.FloorAnchor.GetComponentInChildren<Collider>().gameObject.layer = landingSurfaceLayerNum;
    }
}
