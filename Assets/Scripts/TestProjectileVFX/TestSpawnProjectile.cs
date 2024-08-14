using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnProjectile : MonoBehaviour
{
    public Transform firePoint;
    public GameObject effectToSpawn;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject effect;

            if (firePoint != null)
            {

                effect = Instantiate(effectToSpawn, firePoint.position, firePoint.rotation);
            }

        }
    }
}
