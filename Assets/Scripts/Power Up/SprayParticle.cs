using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayParticle : MonoBehaviour
{
    public ParticleSystem particles;

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent<SlowDown>(out SlowDown slowDown))
        {
            if (!slowDown.IsSlowed)
            {
                slowDown.SlowDownFly();
            }
            else
            {
                slowDown.DropFly();
            }

        };
    }
}
