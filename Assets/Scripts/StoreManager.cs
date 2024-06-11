using Oculus.Interaction;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private List<StoreItemSO> items;
    [SerializeField] private SettingSO settings;

    [Header("UI ")]
    [SerializeField] private GameObject StoreUI;
    [SerializeField] private GameObject ItemTemplate;
    [SerializeField] private Transform GridAnchor;

    [Header("Shared UI")]
    [SerializeField] private TextMeshProUGUI GlobalDescription;
    [SerializeField] private TextMeshProUGUI GlobalCashAmount;


    [Header("Buttons")]
    [SerializeField] private InteractableUnityEventWrapper PurchaseBtn;
    [SerializeField] private InteractableUnityEventWrapper NextWaveBtn;

    private GameObject template;
    private TextMeshProUGUI[] TextMeshes;
    private Transform ObjectColid;

    private void OnEnable()
    {
        PurchaseBtn.WhenSelect.AddListener(Purchase);
        NextWaveBtn.WhenSelect.AddListener(NextWave);
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
    }

    private void ShowStore(int itemCount)
    {
        StoreUI.SetActive(true);

        GlobalDescription.text = items[0].Description;
        GlobalCashAmount.text = settings.Cash.ToString();

        for(int i = 0; i < itemCount; i++)
        {
            template = Instantiate(ItemTemplate, GridAnchor);
            TextMeshes = template.GetComponentsInChildren<TextMeshProUGUI>();

            TextMeshes[0].text = items[i].Price.ToString();
            TextMeshes[1].text = items[i].Name;

            ObjectColid = template.transform.GetChild(1);
            Instantiate(items[i].ItemPrefab, ObjectColid);
        }
    }

    private void HideStore()
    {
        StoreUI.SetActive(false); 
    }
}
