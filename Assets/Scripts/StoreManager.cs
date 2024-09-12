using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;
    public bool IsStoreActive = false;

    [Header("UI")]
    [SerializeField] private GameObject StoreUI;
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
        ThumbsUpEvent.WhenSelected.AddListener(OnThumbsUpSelected);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnThumbsUpSelected();
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
        var powerUps = FindObjectsOfType<BasePowerUpBehavior>();
        foreach (var powerUp in powerUps)
        {
            powerUp.Dissolve();
        }

        // Empty shop displays and shopping basket
        ShoppingBasket.Empty();
        for (int i = 0; i < ShopItemsParent.childCount; i++)
        {
            Destroy(ShopItemsParent.GetChild(i).gameObject);
        }

        // Instantiate shop items
        foreach (var item in ShopItems)
        {
            var powerup = Instantiate(item, ShopItemsParent);
            powerup.transform.localPosition = item.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalPosition;
            powerup.transform.localEulerAngles = item.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
        }

        UIManager.Instance.UpdateCashUI();
        StoreUI.SetActive(true);
        IsStoreActive = true;
        UIManager.Instance.FaceCamera(StoreUI, -0.3f);
    }

    public void HideStore()
    {
        IsStoreActive = false;
        StoreUI.SetActive(false);
        UIManager.Instance.FaceCamera(PowerUpDockingStation.gameObject, -0.3f);
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
        if (ShoppingBasket.Items.Count == 0) return;

        var totalSum = 0;
        foreach (GameObject item in ShoppingBasket.Items)
        {
            var price = item.GetComponent<BasePowerUpBehavior>().StoreItemData.Price;
            totalSum += price;
        }

        if (settings.Cash < totalSum)
        {
            AddTextPopUp("Not Enough Cash!", ShoppingBasket.transform.position);
        }
        else
        {
            Debug.Log("Checking out basket with enough cash");
            settings.Cash -= totalSum;
            UIManager.Instance.UpdateCashUI();
            AddTextPopUp("Purchased!", ShoppingBasket.transform.position);

            // Add to docking station
            int index = 0;
            foreach (var item in ShoppingBasket.Items)
            {
                var powerUp = item.GetComponent<BasePowerUpBehavior>();
                powerUp.IsSold = true;
                powerUp.transform.SetParent(PowerUpDockingStation, true);
                powerUp.transform.localPosition = new Vector3(index * 0.1f, 0, 0);
                powerUp.transform.localEulerAngles = Vector3.zero;
                index++;
                Debug.Log("Purchased " + powerUp.StoreItemData.Name);
            }

            ShoppingBasket.Empty();
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
