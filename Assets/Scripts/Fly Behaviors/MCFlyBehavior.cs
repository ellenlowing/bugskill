using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCFlyBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.tag == "Hands")
        {
            Debug.Log("Collided with hands");

            if (UIManager.Instance.HowToPlayUI.activeInHierarchy)
            {
                UIManager.Instance.StartGameLoopTrigger();
            }
            else if (StoreManager.Instance.IsStoreActive)
            {
                StoreManager.Instance.OnThumbsUpSelected();
            }
        }
    }
}
