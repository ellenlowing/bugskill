using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spray : BasePowerUpBehavior
{
    private SettingSO settings;

    new void Start()
    {
        settings = GameManager.Instance.settings;
        base.Start();
    }

    new void Update()
    {
        base.Update();
    }

    public override void EnterIdleState()
    {
    }

    public override void UpdateIdleState()
    {
    }

    public override void EnterInactiveState()
    {
    }

    public override void UpdateInactiveState()
    {
    }

    public override void EnterActiveState()
    {
    }

    public override void UpdateActiveState()
    {
        base.UpdateActiveState();
    }
}
