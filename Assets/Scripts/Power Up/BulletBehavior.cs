using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {

    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.name);
        Destroy(gameObject);
        if (other.gameObject.tag == "Fly")
        {
            Destroy(other.gameObject);
        }
    }
}