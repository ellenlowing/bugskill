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

    public StoreItemSO StoreItemData;
    public float MaxPowerCapacity = 1;
    public float PowerCapacity = 1; // [0-1]: indicate battery power of swatter or liquid capacity of spray
    public float UsePowerRate = 0.001f;
    public float ChargePowerRate = 0.05f;
    public PowerUpState CurrentState;
    public PointableUnityEventWrapper PointableEventWrapper;

    public void Start()
    {
        EnterState(PowerUpState.IDLE);

        PointableEventWrapper.WhenHover.AddListener(OnHover);
        PointableEventWrapper.WhenUnhover.AddListener(OnUnhover);
        PointableEventWrapper.WhenSelect.AddListener(OnSelect);
        PointableEventWrapper.WhenUnselect.AddListener(OnUnselect);
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
        if (!StoreManager.Instance.IsStoreActive)
        {
            if (PowerCapacity >= 0)
            {
                PowerCapacity -= UsePowerRate;
            }
        }
    }

    public void ResetPowerUp()
    {
        PowerCapacity = MaxPowerCapacity;
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
            GameManager.Instance.RightHandRenderer.SetActive(true);
        }
        else
        {
            GameManager.Instance.LeftHandRenderer.SetActive(true);
        }

        if (StoreManager.Instance.IsStoreActive)
        {
            ShowItemData(arg0);
            StoreManager.Instance.ShopItemDataUI.SetActive(true);
        }
    }

    public void OnUnhover(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
        if (handedness == Handedness.Right)
        {
            GameManager.Instance.RightHandRenderer.SetActive(false);
        }
        else
        {
            GameManager.Instance.LeftHandRenderer.SetActive(false);
        }

        if (StoreManager.Instance.IsStoreActive)
        {
            StoreManager.Instance.GlobalDescription.text = "Grab a Power Up Item and Try it On!";
            StoreManager.Instance.GlobalCashAmount.text = "";
            StoreManager.Instance.GlobalName.text = "";
            StoreManager.Instance.ShopItemDataUI.SetActive(false);
        }
    }

    public void OnSelect(PointerEvent arg0)
    {
        if (StoreManager.Instance.IsStoreActive)
        {
            StoreManager.Instance.SetActivePowerUp(this);
        }
    }

    public void OnUnselect(PointerEvent arg0)
    {
        if (StoreManager.Instance.IsStoreActive)
        {
            StoreManager.Instance.SetActivePowerUp(null);
        }
    }

    public void ShowItemData(PointerEvent arg0)
    {
        StoreManager.Instance.GlobalName.text = StoreItemData.Name;
        StoreManager.Instance.GlobalDescription.text = StoreItemData.Description;
        StoreManager.Instance.GlobalCashAmount.text = StoreItemData.Price.ToString();
    }

}