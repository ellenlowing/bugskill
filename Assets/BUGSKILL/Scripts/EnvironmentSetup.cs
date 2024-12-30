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

    public void AssignAnchorsToLayers()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom != null)
        {
            foreach (var anchor in currentRoom.Anchors)
            {
                var labelFilter = LabelFilter.Included(canLand);

                if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR)
                {
                    anchor.gameObject.layer = LayerMask.NameToLayer(GameManager.Instance.FloorLayerName);
                    foreach (Transform child in anchor.transform)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer(GameManager.Instance.FloorLayerName);
                    }
                }
                else if (anchor.Label == MRUKAnchor.SceneLabels.WALL_FACE)
                {
                    anchor.gameObject.layer = LayerMask.NameToLayer(GameManager.Instance.WallLayerName);
                    foreach (Transform child in anchor.transform)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer(GameManager.Instance.WallLayerName);
                    }
                }
                else if (labelFilter.PassesFilter(anchor.Label))
                {
                    anchor.gameObject.layer = LayerMask.NameToLayer(GameManager.Instance.LandingLayerName);
                    foreach (Transform child in anchor.transform)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer(GameManager.Instance.LandingLayerName);
                    }
                }
            }
        }
    }
}
