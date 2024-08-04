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
        Destroy(gameObject);
        if (other.gameObject.tag == "Fly")
        {
            Destroy(other.gameObject);
        }
    }
}
