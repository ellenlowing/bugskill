using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoppingBasket : MonoBehaviour
{
    public List<GameObject> Items = new List<GameObject>();

    public void Empty()
    {
        // Remove items from basket
        foreach (var item in Items)
        {
            Destroy(item);
        }
        Items.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        BasePowerUpBehavior powerUp = other.gameObject.GetComponentInParent<BasePowerUpBehavior>();
        if (powerUp != null && !powerUp.IsSold && !Items.Contains(powerUp.gameObject))
        {
            Items.Add(powerUp.gameObject);
            powerUp.transform.SetParent(transform);
            Debug.Log("Added " + powerUp.gameObject.name + " to basket");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BasePowerUpBehavior powerUp = other.gameObject.GetComponentInParent<BasePowerUpBehavior>();
        if (powerUp != null && !powerUp.IsSold && Items.Contains(powerUp.gameObject))
        {
            Items.Remove(powerUp.gameObject);
            powerUp.transform.SetParent(StoreManager.Instance.ShopItemsParent);
            Debug.Log("Removed " + powerUp.gameObject.name + " from basket");
        }
    }

    void SetParentAndKeepScale(Transform obj, Transform parent)
    {
        Vector3 originalScale = obj.lossyScale;

        obj.SetParent(parent);

        obj.localScale = new Vector3(
            originalScale.x / parent.lossyScale.x,
            originalScale.y / parent.lossyScale.y,
            originalScale.z / parent.lossyScale.z
        );
    }
}
