using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class EnvironmentSetup : MonoBehaviour
{
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
}
