using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
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

    [SerializeField] private GameObject _leftHandRenderer;
    [SerializeField] private GameObject _rightHandRenderer;

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

    public void OnHover(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
        if (handedness == Handedness.Right)
        {
            _rightHandRenderer.SetActive(true);
        }
        else
        {
            _leftHandRenderer.SetActive(true);
        }
    }

    public void OnUnhover(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
        if (handedness == Handedness.Right)
        {
            _rightHandRenderer.SetActive(false);
        }
        else
        {
            _leftHandRenderer.SetActive(false);
        }
    }

}