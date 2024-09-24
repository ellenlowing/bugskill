using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using Meta.XR.MRUtilityKit;
using UnityEngine.UIElements;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;
    public bool IsStoreActive = false;

    [Range(0.1f, 10f)]
    public float MinDistanceToEdges = 1.5f;

    [Header("UI")]
    public GameObject StoreUI;
    [SerializeField] private GameObject PopupTextObj;

    [Header("Shared UI")]
    [SerializeField] public TextMeshProUGUI GlobalName;
    [SerializeField] public TextMeshProUGUI GlobalDescription;
    [SerializeField] public TextMeshProUGUI GlobalCashAmount;
    public GameObject ShopItemDataUI;
    public GameObject PurchaseBtnUI;

    [Header("Buttons")]
    [SerializeField] private InteractableUnityEventWrapper PurchaseBtn;
    [SerializeField] private InteractableUnityEventWrapper NextWaveBtn;

    [Header("Events")]
    public VoidEventChannelSO StartNextWaveEvent;

    [Header("Power Up")]
    public List<GameObject> ShopItems;
    public Transform ShopItemsParent;

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
    // public TextMeshProUGUI NotEnoughCashDialogText;

    private BasePowerUpBehavior _selectedPowerUp;
    private SettingSO settings;

    private void OnEnable()
    {
        PurchaseBtn.WhenSelect.AddListener(Purchase);
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
        StoreUI.SetActive(false);
        PlaceStore();
        ThumbsUpEvent.WhenSelected.AddListener(OnThumbsUpSelected);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            CheckoutBasket();
        }
    }

    private void Purchase()
    {
        if (_selectedPowerUp != null)
        {
            var shopItemPrice = _selectedPowerUp.StoreItemData.Price;
            if (settings.Cash < shopItemPrice)
            {
                AddTextPopUp("Not Enough Cash!", _selectedPowerUp.transform.position);
            }
            else
            {
                var shopItemName = _selectedPowerUp.StoreItemData.Name;
                settings.Cash -= _selectedPowerUp.StoreItemData.Price;
                UIManager.Instance.UpdateCashUI();

                GameObject powerupItem = _selectedPowerUp.GetComponentInParent<Grabbable>().gameObject;

                AddTextPopUp(shopItemName + " Purchased!", powerupItem.transform.position);

                _selectedPowerUp = null;
            }

        }
    }

    private void NextWave()
    {
        Debug.Log("[Testing] Next Wave Triggered");
        StartNextWaveEvent.RaiseEvent();
    }

    public void ShowStore()
    {
        Debug.Log("Showing store");

        // Dissolve all powerups
        GameManager.Instance.DissolveAllPowerUps();

        // Dialog
        CheckoutInstructions.SetActive(true);
        ThankyouDialog.SetActive(false);
        NotEnoughCashDialog.SetActive(false);

        // Hide Docking Station
        PowerUpDockingStation.gameObject.SetActive(false);

        // Empty shop displays and shopping basket
        ShoppingBasket.Empty();
        for (int i = 0; i < ShopItemsParent.childCount; i++)
        {
            Destroy(ShopItemsParent.GetChild(i).gameObject);
        }

        // Instantiate shop items
        List<GameObject> powerupInStock = new List<GameObject>();
        foreach (var item in ShopItems)
        {
            var powerup = Instantiate(item, ShopItemsParent);
            powerupInStock.Add(powerup);
        }

        // Organize shop items based on list of shop item transforms
        for (int i = 0; i < powerupInStock.Count; i++)
        {
            var powerup = powerupInStock[i];
            powerup.transform.position = ShopItemPositions[i].position;

            RotatePowerUpDisplay(powerup);
        }

        // Use Store Position Finder to grab spawn location
        PlaceStore();

        UIManager.Instance.UpdateCashUI();
        StoreUI.SetActive(true);
        IsStoreActive = true;
    }

    public void PlaceStore()
    {
        Debug.Log("Placing store " + StorePositionFinder.childCount);
        if (StorePositionFinder.childCount == 0)
        {
            StorePositionFinder.GetComponent<FindLargestSpawnPositions>().StartSpawnCurrentRoom();
        }
        StoreUI.transform.position = StorePositionFinder.GetChild(StorePositionFinder.childCount - 1).position;
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room != null)
        {
            var floor = room.FloorAnchor;
            StoreUI.transform.LookAt(floor.transform);
        }
    }

    public void HideStore()
    {
        IsStoreActive = false;
        StoreUI.SetActive(false);
    }

    public void SetActivePowerUp(BasePowerUpBehavior powerUp)
    {
        _selectedPowerUp = powerUp;
        if (powerUp == null)
        {
            PurchaseBtnUI.SetActive(false);
        }
        else
        {
            PurchaseBtnUI.SetActive(true);
        }
    }

    public void OnThumbsUpSelected()
    {
        Debug.Log("Thumbs up selected");
        if (IsStoreActive)
        {
            CheckoutBasket();
        }
    }

    public void CheckoutBasket()
    {
        if (ShoppingBasket.Items.Count == 0)
        {
            NotEnoughCashDialog.SetActive(false);
            CheckoutInstructions.SetActive(false);
            ThankyouDialog.SetActive(true);
            ThankyouDialogText.text = "Oh, nothing in the basket? A true connoisseur of window shopping, I see. Well then, back to work in 3... 2... 1...";

            Invoke(nameof(NextWave), 2f);
            return;
        };

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

    private void RotatePowerUpDisplay(GameObject powerup)
    {
        Transform rotationTransform = powerup.transform.FindChildRecursive("Rotation");
        if (rotationTransform != null)
        {
            rotationTransform.localEulerAngles = powerup.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
        }
        else
        {
            powerup.transform.localEulerAngles = powerup.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
        }
    }

    public void AddTextPopUp(string text, Vector3 position)
    {
        TextMeshProUGUI tempText = PopupTextObj.GetComponentInChildren<TextMeshProUGUI>();
        tempText.text = text;
        GameObject tempObj = Instantiate(PopupTextObj, position, Quaternion.identity);
        UIManager.Instance.FaceCamera(tempObj);
        Destroy(tempObj, 1f);
    }
}
