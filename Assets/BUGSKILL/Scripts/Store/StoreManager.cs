using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Meta.XR.MRUtilityKit;
using UnityEngine.UIElements;
using System.Collections;

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

    [Header("Shared UI")]
    [SerializeField] public TextMeshProUGUI GlobalName;
    [SerializeField] public TextMeshProUGUI GlobalDescription;
    [SerializeField] public TextMeshProUGUI GlobalCashAmount;
    public GameObject ShopItemDataUI;

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
    public SelectorUnityEventWrapper ThumbsUpEvent;
    public Transform PowerUpDockingStation;
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
    public BasePowerUpBehavior _selectedPowerUp;
    public Grabbable RepositionGrabbable;
    public GrabFreeTransformer GrabFreeTransformer;
    public bool HasGrabbedAnyItem = false;
    private bool _hasShownOnce = false;
    private SettingSO settings;
    public List<BasePowerUpBehavior.PowerUpType> _powerUpTypes = new List<BasePowerUpBehavior.PowerUpType>();

    private void OnEnable()
    {
        PurchaseBtn.WhenSelect.AddListener(delegate { Purchase(_selectedPowerUp); });
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
        InitializeStorePosition();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _selectedPowerUp = ShopItemsParent.GetChild(0).GetComponentInChildren<BasePowerUpBehavior>();
            _selectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(_selectedPowerUp);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _selectedPowerUp = ShopItemsParent.GetChild(1).GetComponentInChildren<BasePowerUpBehavior>();
            _selectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(_selectedPowerUp);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _selectedPowerUp = ShopItemsParent.GetChild(2).GetComponentInChildren<BasePowerUpBehavior>();
            _selectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(_selectedPowerUp);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _selectedPowerUp = ShopItemsParent.GetChild(3).GetComponentInChildren<BasePowerUpBehavior>();
            _selectedPowerUp.ActiveOVRHand = GameManager.Instance.LeftOVRHand;
            Purchase(_selectedPowerUp);
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
                StartCoroutine(CueNextWave(CheckoutClips[settings.waveIndex % CheckoutClips.Count], 2f));
            }
        }
    }

    IEnumerator CueNextWave(AudioClip endClip, float delay)
    {
        float totalDuration = StoreOwner.PlayClip(clip: endClip, delay: delay) + 1f;
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

        // Use Store Position Finder to grab spawn location]
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

    public void InitializeStorePosition()
    {
        Debug.Log("Placing store " + StorePositionFinder.childCount);
        if (StorePositionFinder.childCount == 0)
        {
            StorePositionFinder.GetComponent<FindLargestSpawnPositions>().StartSpawnCurrentRoom();
        }
        else
        {
            StoreUI.transform.position = StorePositionFinder.GetChild(StorePositionFinder.childCount - 1).position;
        }
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
        // IsStoreActive = false;
        StoreUI.SetActive(false);
    }

    public void SetActivePowerUp(BasePowerUpBehavior powerUp)
    {
        _selectedPowerUp = powerUp;
        if (powerUp != null)
        {
            Debug.Log("Active Power Up: " + powerUp.gameObject.name);
        }
        else
        {
            Debug.Log("Removed Active Power Up");
        }
    }

    public void OnThumbsUpSelected()
    {
        Debug.Log("Thumbs up selected");
        if (IsStoreActive) // Change isstoreactive
        {
            CheckoutBasket();
        }
    }

    public void CheckoutBasket()
    {
        IsStoreActive = false;
        if (ShoppingBasket.Items.Count == 0)
        {
            NotEnoughCashDialog.SetActive(false);
            CheckoutInstructions.SetActive(false);
            ThankyouDialog.SetActive(true);
            ThankyouDialogText.text = "Oh, nothing in the basket? A true connoisseur of window shopping, I see. Well then, back to work in 3... 2... 1...";

            Invoke(nameof(NextWave), 2f);
            return;
        }
        ;

        var totalSum = 0;
        foreach (GameObject item in ShoppingBasket.Items)
        {
            var price = item.GetComponent<BasePowerUpBehavior>().StoreItemData.Price;
            totalSum += price;
        }

        if (settings.Cash < totalSum)
        {
            NotEnoughCashDialog.SetActive(true);
            CheckoutInstructions.SetActive(false);
            ThankyouDialog.SetActive(false);
        }
        else
        {
            Debug.Log("Checking out basket with enough cash");
            settings.Cash -= totalSum;
            UIManager.Instance.UpdateCashUI();
            // AddTextPopUp("Purchased " + ShoppingBasket.Items.Count + " items!", ShoppingBasket.transform.position);
            NotEnoughCashDialog.SetActive(false);
            CheckoutInstructions.SetActive(false);
            ThankyouDialog.SetActive(true);
            ThankyouDialogText.text = "You've checked out " + ShoppingBasket.Items.Count + " items! Thank you for supporting my small business.\nBack to work in 3... 2... 1...";
            CheckoutSound.Play();

            // Add to docking station
            int index = 0;
            foreach (var item in ShoppingBasket.Items)
            {
                var powerUp = item.GetComponent<BasePowerUpBehavior>();
                powerUp.IsSold = true;
                powerUp.gameObject.transform.SetParent(PowerUpDockingStation, true);
                powerUp.gameObject.transform.localPosition = new Vector3(index * 0.3f, 0, 0);
                powerUp.GlowEffect.gameObject.SetActive(true);
                powerUp.GlowEffect.Stop();
                powerUp.GlowEffect.Play();
                RotatePowerUpDisplay(powerUp.gameObject);

                index++;
                Debug.Log("Purchased " + powerUp.StoreItemData.Name);
            }

            // Place docking station behind store
            PowerUpDockingStation.transform.position = StoreUI.transform.position - StoreUI.transform.forward * 0.3f + StoreUI.transform.up * 0.8f;
            PowerUpDockingStation.gameObject.SetActive(true);

            // Start Next Wave
            Invoke(nameof(NextWave), 2f);
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
