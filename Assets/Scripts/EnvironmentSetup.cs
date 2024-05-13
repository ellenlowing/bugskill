using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class EnvironmentSetup : MonoBehaviour
{
    public MRUKAnchor.SceneLabels canLand = MRUKAnchor.SceneLabels.FLOOR | MRUKAnchor.SceneLabels.CEILING;

    void Start()
    {
        SetAllAnchorsToConvex();
    }

    private void SetAllAnchorsToConvex()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            foreach (var anchor in currentRoom.Anchors)
            {
                MeshCollider[] meshColliders = anchor.GetComponentsInChildren<MeshCollider>();
                foreach (MeshCollider meshCollider in meshColliders)
                {
                    meshCollider.convex = true;
                }
            }
        }
    }

    public void AssignLayerToLandingSurfaces()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            foreach (var anchor in currentRoom.Anchors)
            {
                var labelFilter = LabelFilter.FromEnum(canLand);
                if (labelFilter.PassesFilter(anchor.AnchorLabels))
                {
                    anchor.gameObject.layer = 6;
                    Debug.Log("Assigned layer to " + anchor.name);
                }
            }
        }
    }
}
