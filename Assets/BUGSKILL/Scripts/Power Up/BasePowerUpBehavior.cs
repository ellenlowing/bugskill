using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class BasePowerUpBehavior : MonoBehaviour
{
    public enum PowerUpState
    {
        IDLE, // not in use + not grabbed
        INACTIVE, // not in use + grabbed
        ACTIVE // in use + grabbed
    }

    public enum PowerUpType
    {
        SWATTER,
        SPRAY,
        FROGGY,
        FINGER_SLINGSHOT,
        FINGER_GUN
    }

    [System.Serializable]
    public class MeshMatPair
    {
        public MeshRenderer MeshRenderer;
        public Material Material;
    }

    public OVRHand ActiveOVRHand = null;
    public PowerUpType Type;
    public PowerUpState CurrentState;
    public PointableUnityEventWrapper PointableEventWrapper;
    public ParticleSystem GlowEffect;
    public bool AnchoredToHandPose = true;

    [Header("Power Capacity Settings")]
    public float MaxPowerCapacity = 1;
    public float PowerCapacity = 1; // [0-1]: indicate battery power of swatter or liquid capacity of spray
    public float UsePowerRate = 0.001f;
    public float DissolveDuration = 1f;
    public List<MeshMatPair> DissolvePairs;
    public Slider PowerCapacitySlider;

    [Header("Settings Data")]
    protected SettingSO settings;

    [Header("Shop Item Things")]
    public bool IsSold = false;
    public StoreItemSO StoreItemData;
    public GameObject LeftUIContainer;
    public GameObject RightUIContainer;
    public TextMeshPro LeftPriceTagText;
    public TextMeshPro RightPriceTagText;
    public TextMeshPro DisplayPriceTagText;
    public GameObject DisplayPriceTag;
    public VideoPlayer Teaser;

    protected bool _isEquipped = false;

    public void Start()
    {
        settings = GameManager.Instance.settings;
        DissolveDuration = GameManager.Instance.DissolveDuration;
        ResetPowerUp();
        EnterState(PowerUpState.IDLE);

        if (PointableEventWrapper != null)
        {
            // PointableEventWrapper.WhenHover.AddListener(OnHover);
            // PointableEventWrapper.WhenUnhover.AddListener(OnUnhover);
            PointableEventWrapper.WhenSelect.AddListener(OnGrabbableSelect);
            PointableEventWrapper.WhenUnselect.AddListener(OnGrabbableUnselect);
        }

        HandleUI(showDetails: false, showPriceTag: true, handedness: Handedness.Right);
        if (Teaser != null) Teaser.Prepare();

        GlowEffect.gameObject.SetActive(false);

        LeftPriceTagText.text = StoreItemData.Price.ToString();
        RightPriceTagText.text = StoreItemData.Price.ToString();
        DisplayPriceTagText.text = StoreItemData.Price.ToString();
    }

    public void Update()
    {
        UpdateState();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dissolve();
        }
#endif
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
    public virtual void EnterActiveState()
    {
        if (IsSold)
        {
            DisplayPriceTag.SetActive(false);
        }
    }
    public virtual void UpdateActiveState()
    {
        if (PowerCapacitySlider != null)
        {
            if (!PowerCapacitySlider.gameObject.activeSelf && !StoreManager.Instance.IsStoreActive)
            {
                PowerCapacitySlider.gameObject.SetActive(true);
            }
        }

        if (!StoreManager.Instance.IsStoreActive && PowerCapacity <= 0)
        {
            PowerCapacity = 0;
            ActiveOVRHand = null;
            transform.parent = null;
            Dissolve();
        }
    }

    public virtual void ResetPowerUp()
    {
        PowerCapacity = MaxPowerCapacity;
        if (PowerCapacitySlider != null)
        {
            PowerCapacitySlider.value = PowerCapacity;
            PowerCapacitySlider.gameObject.SetActive(false);
        }
    }

    public virtual void Dissolve()
    {
        StoreManager.Instance.RestockPowerup(this);

        EnterState(PowerUpState.INACTIVE);

        if (PowerCapacitySlider != null)
        {
            PowerCapacitySlider.gameObject.SetActive(false);
        }

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
    }

    public void OnUnhover(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
    }

    public virtual void OnGrabbableSelect(PointerEvent arg0)
    {
        HandRef handData = (HandRef)arg0.Data;
        Handedness handedness = handData.Handedness;
        if (handedness == Handedness.Right)
        {
            ActiveOVRHand = GameManager.Instance.RightOVRHand;
        }
        else
        {
            ActiveOVRHand = GameManager.Instance.LeftOVRHand;
        }

        if (StoreManager.Instance.IsStoreActive && !IsSold)  
        {
            StoreManager.Instance.SetActivePowerUp(this);
            HandleUI(showDetails: true, showPriceTag: false, handedness: handedness);
            if (Teaser != null) Teaser.Play();
        }

        if(IsSold)
        {
            if (AnchoredToHandPose)
            {
                transform.parent = ActiveOVRHand.gameObject.transform;
            }
        }

        EnterState(PowerUpState.ACTIVE);
    }

    public virtual void OnGrabbableUnselect(PointerEvent arg0)
    {
        if (!IsSold && StoreManager.Instance.IsStoreActive)
        {
            StoreManager.Instance.SetActivePowerUp(null);
            HandleUI(showDetails: false, showPriceTag: true, handedness: Handedness.Right);
            if (Teaser != null) Teaser.Stop();
        }

        if (!_isEquipped)
        {
            ActiveOVRHand = null;
            EnterState(PowerUpState.IDLE);
        }
    }

    public void HandleUI(bool showDetails, bool showPriceTag, Handedness handedness)
    {
        DisplayPriceTag.SetActive(showPriceTag);
        LeftUIContainer.SetActive(showDetails && handedness == Handedness.Left);
        RightUIContainer.SetActive(showDetails && handedness == Handedness.Right);
    }

    public void HandlePurchase()
    {
        IsSold = true;
        _isEquipped = true;
        StoreManager.Instance.SetActivePowerUp(this);
        HandleUI(showDetails: false, showPriceTag: false, handedness: Handedness.Right);
        if (AnchoredToHandPose)
        {
            transform.parent = ActiveOVRHand.gameObject.transform;
        }
    }

    public void ShowItemData(PointerEvent arg0)
    {
        StoreManager.Instance.GlobalName.text = StoreItemData.Name;
        StoreManager.Instance.GlobalDescription.text = StoreItemData.Description;
        StoreManager.Instance.GlobalCashAmount.text = StoreItemData.Price.ToString();
    }

    public void UsePowerCapacity()
    {
        if (!StoreManager.Instance.IsStoreActive)
        {
            if (PowerCapacity > 0)
            {
                PowerCapacity -= UsePowerRate;
                PowerCapacitySlider.value = PowerCapacity;
            }
        }
    }

}