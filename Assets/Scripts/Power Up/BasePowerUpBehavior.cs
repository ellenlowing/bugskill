using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePowerUpBehavior : MonoBehaviour
{
    public enum PowerUpState
    {
        IDLE, // not in use + not grabbed
        INACTIVE, // not in use + grabbed
        ACTIVE // in use + grabbed
    }

    public float MaxPowerCapacity = 1;
    public float PowerCapacity = 1; // [0-1]: indicate battery power of swatter or liquid capacity of spray
    public float UsePowerRate = 0.001f;
    public float ChargePowerRate = 0.05f;
    public PowerUpState CurrentState;

    [SerializeField] private Renderer _rightHandRenderer;
    [SerializeField] private Material _handMaterial;

    public void Start()
    {
        EnterState(PowerUpState.IDLE);
    }

    public void Update()
    {
        UpdateState();
    }

    public void EnterState(PowerUpState state)
    {
        switch (state)
        {
            case PowerUpState.IDLE:
                EnterIdleState();
                break;

            case PowerUpState.INACTIVE:
                EnterInactiveState();
                break;

            case PowerUpState.ACTIVE:
                EnterActiveState();
                break;
        }

        CurrentState = state;
    }

    public void UpdateState()
    {
        switch (CurrentState)
        {
            case PowerUpState.IDLE:
                UpdateIdleState();
                break;

            case PowerUpState.INACTIVE:
                UpdateInactiveState();
                break;

            case PowerUpState.ACTIVE:
                UpdateActiveState();
                break;
        }
    }

    public virtual void EnterIdleState() { }
    public virtual void UpdateIdleState() { }
    public virtual void EnterInactiveState() { }
    public virtual void UpdateInactiveState() { }
    public virtual void EnterActiveState() { }
    public virtual void UpdateActiveState()
    {
        if (PowerCapacity > 0)
        {
            PowerCapacity -= UsePowerRate;
        }
    }

    public void Charge()
    {
        if (PowerCapacity < MaxPowerCapacity)
        {
            PowerCapacity += ChargePowerRate;
        }
        else
        {
            PowerCapacity = MaxPowerCapacity;
        }
    }

    public void OnHover()
    {
        _rightHandRenderer.material = _handMaterial;
    }

    public void OnUnhover()
    {
        _rightHandRenderer.material = null;
        _rightHandRenderer.materials = new Material[0];
    }

}