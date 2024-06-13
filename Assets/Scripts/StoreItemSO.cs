using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreItem")]
public class StoreItemSO : ScriptableObject
{
    public GameObject ItemPrefab;
    public int Price = 0;
    public string Name = "ItemName";
    public string Description = "Nothing";
    public Vector3 LocalPosition = Vector3.zero;
    public Quaternion LocalRotation = Quaternion.identity;
}
