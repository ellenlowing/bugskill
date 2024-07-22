using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayParticle : MonoBehaviour
{
    public ParticleSystem particles;

    void Start()
    {

    }

    void Update()
    {

    }

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent<SlowDown>(out SlowDown slowDown))
        {
            slowDown.SlowDownFly();
        };
    }
}
