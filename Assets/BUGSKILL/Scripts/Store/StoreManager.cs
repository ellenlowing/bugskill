using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using Meta.XR.MRUtilityKit;
using UnityEngine.UIElements;
using System.Collections;

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
    // public TextMeshProUGUI NotEnoughCashDialogText;

    public BasePowerUpBehavior _selectedPowerUp;
    private SettingSO settings;
    public List<BasePowerUpBehavior.PowerUpType> _powerUpTypes = new List<BasePowerUpBehavior.PowerUpType>();

    private void OnEnable()
    {
        // PurchaseBtn.WhenSelect.AddListener(Purchase);
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

    public void Purchase(BasePowerUpBehavior powerup)
    {
        if (powerup != null)
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
                powerup.IsSold = true;
                UIManager.Instance.UpdateCashUI();

                GameObject powerupItem = powerup.GetComponentInParent<Grabbable>().gameObject;

                AddTextPopUp(shopItemName + " Purchased!", powerupItem.transform.position);

                _selectedPowerUp = powerup;

                Invoke(nameof(NextWave), 1f);
            }
        }
    }

    public void NextWave()
    {
        Debug.Log("[Testing] Next Wave Triggered");
        StartNextWaveEvent.RaiseEvent();
    }

    public void ShowStore()
    {
        Debug.Log("Showing store");

        // UNCOMMENT IF SHOPPING BASKET FEATURE IS RE-ENABLED
        // // Dialog
        // CheckoutInstructions.SetActive(true);
        // ThankyouDialog.SetActive(false);
        // NotEnoughCashDialog.SetActive(false);

        // // Hide Docking Station
        // PowerUpDockingStation.gameObject.SetActive(false);

        // Empty shopping basket
        // ShoppingBasket.Empty();

        // ORIGINAL IMPLEMENTATION STARTS HERE
        // Empty store displays
        // for (int i = 0; i < ShopItemsParent.childCount; i++)
        // {
        //     Destroy(ShopItemsParent.GetChild(i).gameObject);
        // }

        // // Instantiate shop items
        // List<GameObject> powerupInStock = new List<GameObject>();
        // foreach (var item in ShopItems)
        // {
        //     var powerup = Instantiate(item, ShopItemsParent);
        //     powerupInStock.Add(powerup);
        // }

        // // Organize shop items based on list of shop item transforms
        // for (int i = 0; i < powerupInStock.Count; i++)
        // {
        //     var powerup = powerupInStock[i];
        //     powerup.transform.position = ShopItemPositions[i].position;
        //     RotatePowerUpDisplay(powerup);
        // }
        // ORIGINAL IMPLEMENTATION ENDS HERE

        for (int i = 0; i < ShopItemsParent.childCount; i++)
        {
            var powerup = ShopItemsParent.GetChild(i).gameObject;
            PlacePowerUpDisplay(powerup);
            RotatePowerUpDisplay(powerup);
        }

        // Use Store Position Finder to grab spawn location
        PlaceStore();
        UIManager.Instance.UpdateCashUI();
        StoreUI.SetActive(true);
        IsStoreActive = true;
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
        // Store looks at user
        StoreUI.transform.LookAt(new Vector3(GameManager.Instance.MainCameraTransform.position.x, 0, GameManager.Instance.MainCameraTransform.position.z));
        StoreUI.transform.eulerAngles = new Vector3(0, StoreUI.transform.eulerAngles.y, 0);
    }

    public void HideStore()
    {
        IsStoreActive = false;

        // StoreUI.transform.position = new Vector3(StoreUI.transform.position.x, -100, StoreUI.transform.position.z);
        StoreUI.SetActive(false);
    }

    // public void SetActivePowerUp(BasePowerUpBehavior powerUp)
    // {
    //     _selectedPowerUp = powerUp;
    //     if (powerUp == null)
    //     {
    //         PurchaseBtnUI.SetActive(false);
    //     }
    //     else
    //     {
    //         PurchaseBtnUI.SetActive(true);
    //     }
    // }

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
        powerup.transform.position = containerTransform.position;
    }

    private void RotatePowerUpDisplay(GameObject powerup)
    {
        Transform rotationTransform = powerup.transform.FindChildRecursive("Rotation");
        if (rotationTransform != null)
        {
            rotationTransform.localEulerAngles = powerup.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
            powerup.transform.localRotation = Quaternion.identity;
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
