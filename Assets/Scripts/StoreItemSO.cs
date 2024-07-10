using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreItem")]
public class StoreItemSO : ScriptableObject
{
    public int Price = 0;
    public string Name = "ItemName";
    public string Description = "Nothing";
    public Vector3 LocalPosition = Vector3.zero;
    public Vector3 LocalEulerAngles = Vector3.zero;
}
