using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

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
    public List<GameObject> BoughtItems = new List<GameObject>();

    public bool IsStoreActive = false;
    public Transform ShopItemsParent;

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
                // powerupItem.SetActive(false);

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
        foreach (var item in BoughtItems)
        {
            Destroy(item);
        }
        BoughtItems = new List<GameObject>();

        IsStoreActive = true;

        for (int i = 0; i < ShopItemsParent.childCount; i++)
        {
            Destroy(ShopItemsParent.GetChild(i).gameObject);
        }

        StoreUI.SetActive(true);
        UIManager.Instance.FaceCamera(StoreUI, -0.3f);
        foreach (var item in ShopItems)
        {
            var powerup = Instantiate(item, ShopItemsParent);
            powerup.transform.localPosition = item.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalPosition;
            powerup.transform.localEulerAngles = item.GetComponentInChildren<BasePowerUpBehavior>().StoreItemData.LocalEulerAngles;
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

    public void HideAllPowerUps()
    {
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
