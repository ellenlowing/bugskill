using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaydoSpawner : MonoBehaviour
{
    public GameObject Playdo;
    private GameObject spawned;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (spawned != null)
            {
                Destroy(spawned);
            }
            spawned = Instantiate(Playdo, new Vector3(0, 2f, 0), Quaternion.identity);
        }
    }
}
