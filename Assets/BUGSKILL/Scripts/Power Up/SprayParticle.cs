using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayParticle : MonoBehaviour
{
    public ParticleSystem particles;

    void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent<BaseFlyBehavior>(out BaseFlyBehavior fly))
        {
            if (other.tag == "Fly")
            {
                if (!fly.IsSlowed)
                {
                    fly.SlowDown();
                }
                else
                {
                    fly.EnterState(BaseFlyBehavior.FlyState.DYING);
                }
            }
        }
    }
}
