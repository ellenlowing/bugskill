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

    [System.Serializable]
    public class MeshMatPair
    {
        public MeshRenderer MeshRenderer;
        public Material Material;
    }

    public OVRHand ActiveHand = null;
    public StoreItemSO StoreItemData;
    public PowerUpState CurrentState;
    public PointableUnityEventWrapper PointableEventWrapper;

    [Header("Power Capacity Settings")]
    public float MaxPowerCapacity = 1;
    public float PowerCapacity = 1; // [0-1]: indicate battery power of swatter or liquid capacity of spray
    public float UsePowerRate = 0.001f;
    public float ChargePowerRate = 0.05f;
    public float DissolveDuration = 1f;
    public List<MeshMatPair> DissolvePairs;

    public void Start()
    {
        EnterState(PowerUpState.IDLE);

        PointableEventWrapper.WhenHover.AddListener(OnHover);
        PointableEventWrapper.WhenUnhover.AddListener(OnUnhover);
        PointableEventWrapper.WhenSelect.AddListener(OnSelect);
        PointableEventWrapper.WhenUnselect.AddListener(OnUnselect);
        PointableEventWrapper.WhenSelect.AddListener(OnGrabbableSelect);
        PointableEventWrapper.WhenUnselect.AddListener(OnGrabbableUnselect);
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
        if (!StoreManager.Instance.IsStoreActive && PowerCapacity <= 0)
        {
            PowerCapacity = 0;
            ActiveHand = null;
            transform.parent = null;
            Dissolve();
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

    public virtual void Dissolve()
    {
        GameManager.Instance.RightHandRenderer.SetActive(false);
        GameManager.Instance.LeftHandRenderer.SetActive(false);

        foreach (var pair in DissolvePairs)
        {
            if (pair.Material != null)
            {
                StartCoroutine(DissolveSingleMesh(pair.MeshRenderer, pair.Material, DissolveDuration));
            }
            else
            {
                pair.MeshRenderer.gameObject.SetActive(false);
            }
        }

        Destroy(gameObject, DissolveDuration);
    }

    IEnumerator DissolveSingleMesh(MeshRenderer mesh, Material mat, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            mat.SetFloat("_Cutoff", Mathf.Lerp(0, 1, t / duration));
            mesh.material = mat;
            t += Time.deltaTime;
            yield return null;
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

    // TODO change selection method
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

    private void OnGrabbableSelect(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
        if (handedness == Handedness.Right)
        {
            ActiveHand = GameManager.Instance.RightHand.GetComponent<OVRHand>();
        }
        else
        {
            ActiveHand = GameManager.Instance.LeftHand.GetComponent<OVRHand>();
        }
        transform.parent = ActiveHand.gameObject.transform;
        EnterState(PowerUpState.ACTIVE);
    }

    private void OnGrabbableUnselect(PointerEvent arg0)
    {
        // ActiveHand = null;
        // EnterState(PowerUpState.IDLE);
    }

    public void ShowItemData(PointerEvent arg0)
    {
        StoreManager.Instance.GlobalName.text = StoreItemData.Name;
        StoreManager.Instance.GlobalDescription.text = StoreItemData.Description;
        StoreManager.Instance.GlobalCashAmount.text = StoreItemData.Price.ToString();
    }

}