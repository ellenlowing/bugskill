using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;
    [SerializeField] private List<StoreItemSO> items;
    [SerializeField] private SettingSO settings;

    [Header("UI")]
    [SerializeField] private GameObject StoreUI;
    [SerializeField] private GameObject ItemTemplate;
    [SerializeField] private Transform GridAnchor;

    [Header("Shared UI")]
    [SerializeField] private TextMeshProUGUI GlobalDescription;
    [SerializeField] private TextMeshProUGUI GlobalCashAmount;

    [Header("Buttons")]
    [SerializeField] private InteractableUnityEventWrapper PurchaseBtn;
    [SerializeField] private InteractableUnityEventWrapper NextWaveBtn;

    [Header("Events")]
    public VoidEventChannelSO StartNextWaveEvent;

    private GameObject template;
    private TextMeshProUGUI[] TextMeshes;
    private Transform ObjectColid;
    private List<GameObject> shopItems = new List<GameObject>();

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
        ShowStore(3);
    }

    private void Purchase()
    {
        Debug.Log("[Testing] Purchase Triggered");
    }

    private void NextWave()
    {
        Debug.Log("[Testing] Next Wave Triggered");
        StartNextWaveEvent.RaiseEvent();
    }

    public void ShowStore(int itemCount)
    {
        StoreUI.SetActive(true);
        UIManager.Instance.FaceCamera(StoreUI);

        GlobalDescription.text = items[0].Description;
        GlobalCashAmount.text = settings.Cash.ToString();

        for (int i = 0; i < itemCount; i++)
        {
            template = Instantiate(ItemTemplate, GridAnchor);
            TextMeshes = template.GetComponentsInChildren<TextMeshProUGUI>();

            TextMeshes[0].text = items[i].Price.ToString();
            TextMeshes[1].text = items[i].Name;

            ObjectColid = template.transform.GetChild(1);
            GameObject item = Instantiate(items[i].ItemPrefab, ObjectColid);
            shopItems.Add(item);
        }
    }

    public void HideStore()
    {
        foreach (GameObject item in shopItems)
        {
            Destroy(item);
        }
        shopItems = new List<GameObject>();
        StoreUI.SetActive(false);
    }
}
