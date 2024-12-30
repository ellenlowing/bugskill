using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTestFlyBehavior : BaseFlyBehavior
{
    new void Start()
    {
        base.Start();
        EnterState(FlyState.IDLE);
        TakeoffChance = 0;
    }
}
