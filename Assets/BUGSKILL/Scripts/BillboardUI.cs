using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{

    void Start()
    {

    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - GameManager.Instance.MainCameraTransform.position);

    }
}
