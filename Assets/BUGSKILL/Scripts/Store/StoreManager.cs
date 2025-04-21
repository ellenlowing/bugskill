using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Oculus.Interaction;
using System.Collections;
using UnityEngine.InputSystem;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;
    public bool IsStoreActive = false;

    [Range(0.1f, 10f)]
    public float MinDistanceToEdges = 1.5f;

    public int NumPowerUpToStart = 2;

    [Header("UI")]
    public GameObject StoreUI;
    [SerializeField] private GameObject PopupTextObj;

    [Header("Buttons")]
    public GameObject StoreButtons;
    [SerializeField] private InteractableUnityEventWrapper PurchaseBtn;
    [SerializeField] private InteractableUnityEventWrapper NextWaveBtn;

    [Header("Events")]
    public VoidEventChannelSO StartNextWaveEvent;

    [Header("Power Up")]
    public List<GameObject> ShopItems;
    public Transform ShopItemsParent;
    public Transform ShopItemContainersParent;

    [Header("Shop Props")]
    public GameObject ShopMaster;
    public ShoppingBasket ShoppingBasket;
    public List<Transform> ShopItemPositions;
    public Transform StorePositionFinder;
    public AudioSource CheckoutSound;

    [Header("Dialog")]
    public GameObject CheckoutInstructions;
    public GameObject ThankyouDialog;
    public TextMeshProUGUI ThankyouDialogText;
    public GameObject NotEnoughCashDialog;

    [Header("Audio")]
    public MCFlyBehavior StoreOwner;
    public AudioSource KaChingSFX;
    public List<AudioClip> WelcomeClips;
    public AudioClip BuyInstructionClip;
    public List<AudioClip> CheckoutClips;

    [Header("Misc")]
    public BasePowerUpBehavior SelectedPowerUp;
    public Grabbable RepositionGrabbable;
    public GrabFreeTransformer GrabFreeTransformer;
    public bool HasGrabbedAnyItem = false;
    private bool _hasShownOnce = false;
    private SettingSO settings;

    private void OnEnable()
    {
        PurchaseBtn.WhenSelect.AddListener(delegate { Purchase(SelectedPowerUp); });
        NextWaveBtn.WhenSelect.AddListener(NextWave);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        settings = GameManager.Instance.settings;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
        {
            SelectedPowerUp = ShopItemsParent.GetChild(0).GetComponentInChildren<BasePowerUpBehavior>();
            SelectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(SelectedPowerUp);
        }
        else if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
        {
            SelectedPowerUp = ShopItemsParent.GetChild(1).GetComponentInChildren<BasePowerUpBehavior>();
            SelectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(SelectedPowerUp);
        }
        else if (Keyboard.current[Key.Digit3].wasPressedThisFrame)
        {
            SelectedPowerUp = ShopItemsParent.GetChild(2).GetComponentInChildren<BasePowerUpBehavior>();
            SelectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(SelectedPowerUp);
        }
        else if (Keyboard.current[Key.Digit4].wasPressedThisFrame)
        {
            SelectedPowerUp = ShopItemsParent.GetChild(3).GetComponentInChildren<BasePowerUpBehavior>();
            SelectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(SelectedPowerUp);
        }
#endif

    }

    public void Purchase(BasePowerUpBehavior powerup)
    {
        if (powerup != null && !powerup.IsSold)
        {
            var shopItemPrice = powerup.StoreItemData.Price;
            if (settings.Cash < shopItemPrice)
            {
                AddTextPopUp("Not Enough Cash!", powerup.transform.position);
            }
            else
            {
                var shopItemName = powerup.StoreItemData.Name;
                settings.Cash -= powerup.StoreItemData.Price;
                powerup.HandlePurchase();
                UIManager.Instance.UpdateCashUI();

                GameObject powerupItem = powerup.GetComponentInParent<Grabbable>().gameObject;
                AddTextPopUp(shopItemName + " Purchased!", powerupItem.transform.position);
                KaChingSFX.Play();
                // StartCoroutine(CueNextWave(CheckoutClips[settings.waveIndex % CheckoutClips.Count], 2f));
                StartCoroutine(CueNextWave(endClip: null, delay: 1f));
            }
        }
    }

    IEnumerator CueNextWave(AudioClip endClip = null, float delay = 0f)
    {
        float totalDuration = delay;
        if (endClip != null)
        {
            totalDuration = StoreOwner.PlayClip(clip: endClip, delay: delay) + 1f;
        }
        StoreButtons.SetActive(false);
        yield return new WaitForSeconds(totalDuration);
        NextWave();
    }

    public void NextWave()
    {
        Debug.Log("[Testing] Next Wave Triggered");
        StartNextWaveEvent.RaiseEvent();
    }

    public void ShowStore()
    {
        Debug.Log("Showing store");

        if (GameManager.Instance.TestMode)
        {
            NumPowerUpToStart = 5;
        }

        for (int i = 0; i < ShopItems.Count; i++)
        {
            var powerup = ShopItems[i];
            if ((i - (NumPowerUpToStart - 1)) < settings.waveIndex)
            {
                PlacePowerUpDisplay(powerup);
                RotatePowerUpDisplay(powerup);
            }
            else
            {
                ShowTeaser(powerup);
            }
        }

        // Place store in front of player
        StoreButtons.SetActive(true);
        HasGrabbedAnyItem = false;
        PlaceStore();
        UIManager.Instance.UpdateCashUI();
        StoreUI.SetActive(true);
        IsStoreActive = true;
        BGMManager.Instance.CueStore();
        StoreOwner.PlayClip(clip: WelcomeClips[(settings.waveIndex + 1) % WelcomeClips.Count], delay: 3f);
    }

    public void RestockPowerup(BasePowerUpBehavior powerup)
    {
        var shopItem = ShopItems.Find(item => item.GetComponent<BasePowerUpBehavior>().Type == powerup.Type);
        var powerupObj = Instantiate(shopItem, ShopItemsParent);
        PlacePowerUpDisplay(powerupObj);
        RotatePowerUpDisplay(powerupObj);
    }

    public void PlaceStore()
    {
        if (!_hasShownOnce)
        {
            UIManager.Instance.FaceCamera(StoreUI, placeOnFloor: true, flipForwardVector: true, distanceFromCamera: settings.farDistanceFromCamera);
            _hasShownOnce = true;
        }

        TransformerUtils.PositionConstraints positionConstraints = new TransformerUtils.PositionConstraints();
        TransformerUtils.ConstrainedAxis yAxis = new TransformerUtils.ConstrainedAxis();
        yAxis.ConstrainAxis = true;
        TransformerUtils.FloatRange yAxisRange = new TransformerUtils.FloatRange();
        yAxisRange.Min = StoreUI.transform.position.y;
        yAxisRange.Max = StoreUI.transform.position.y;
        yAxis.AxisRange = yAxisRange;
        positionConstraints.YAxis = yAxis;
        GrabFreeTransformer.InjectOptionalPositionConstraints(positionConstraints);
        RepositionGrabbable.InjectOptionalOneGrabTransformer(GrabFreeTransformer);
    }

    public void HideStore()
    {
        StoreUI.SetActive(false);
    }

    public void SetActivePowerUp(BasePowerUpBehavior powerUp)
    {
        SelectedPowerUp = powerUp;
        if (powerUp != null)
        {
            Debug.Log("Active Power Up: " + powerUp.gameObject.name);
        }
        else
        {
            Debug.Log("Removed Active Power Up");
        }
    }

    private void PlacePowerUpDisplay(GameObject powerup)
    {
        string containerName = powerup.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.ItemContainerName;
        Transform containerTransform = ShopItemContainersParent.Find(containerName);
        Transform teaserTransform = containerTransform.Find("Teaser");
        if (teaserTransform != null)
        {
            teaserTransform.gameObject.SetActive(false);
        }
        GameObject powerupInStock = FindChildWithCondition(ShopItemsParent, item => item.GetComponent<BasePowerUpBehavior>().Type == powerup.GetComponent<BasePowerUpBehavior>().Type).gameObject;
        powerupInStock.SetActive(true);
        powerupInStock.GetComponent<BasePowerUpBehavior>().SetGrabbableActive(true);
        powerupInStock.transform.position = containerTransform.position;
    }

    private void RotatePowerUpDisplay(GameObject powerup)
    {
        GameObject powerupInStock = FindChildWithCondition(ShopItemsParent, item => item.GetComponent<BasePowerUpBehavior>().Type == powerup.GetComponent<BasePowerUpBehavior>().Type).gameObject;
        Transform rotationTransform = powerupInStock.transform.FindChildRecursive("Rotation");

        if (rotationTransform != null)
        {
            rotationTransform.localEulerAngles = powerupInStock.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
            powerupInStock.transform.localRotation = Quaternion.identity;
        }
        else
        {
            powerupInStock.transform.localEulerAngles = powerupInStock.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
        }
    }

    private void ShowTeaser(GameObject powerup)
    {
        string containerName = powerup.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.ItemContainerName;
        Transform containerTransform = ShopItemContainersParent.Find(containerName);
        Transform teaserTransform = containerTransform.Find("Teaser");
        if (teaserTransform != null)
        {
            teaserTransform.gameObject.SetActive(true);
        }
        GameObject powerupInStock = FindChildWithCondition(ShopItemsParent, item => item.GetComponent<BasePowerUpBehavior>().Type == powerup.GetComponent<BasePowerUpBehavior>().Type).gameObject;
        powerupInStock.SetActive(false);
    }

    public void DisableAllGrabbables()
    {
        foreach (Transform child in ShopItemsParent)
        {
            child.GetComponent<BasePowerUpBehavior>().SetGrabbableActive(false);
        }
    }

    public void ShufflePowerups()
    {
        for (int i = ShopItems.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (ShopItems[i], ShopItems[j]) = (ShopItems[j], ShopItems[i]); // Swap elements
        }
    }

    public Transform FindChildWithCondition(Transform parent, System.Func<Transform, bool> condition)
    {
        foreach (Transform child in parent)
        {
            if (condition(child))
            {
                return child; // Return the first matching child
            }
        }
        return null; // No match found
    }

    public void AddTextPopUp(string text, Vector3 position)
    {
        TextMeshProUGUI tempText = PopupTextObj.GetComponentInChildren<TextMeshProUGUI>();
        tempText.text = text;
        GameObject tempObj = Instantiate(PopupTextObj, position, Quaternion.identity);
        UIManager.Instance.FaceCamera(tempObj);
        Destroy(tempObj, 1f);
    }

    public void CueBuyInstruction()
    {
        StoreOwner.PlayClip(clip: BuyInstructionClip, delay: 2f);
    }
}
