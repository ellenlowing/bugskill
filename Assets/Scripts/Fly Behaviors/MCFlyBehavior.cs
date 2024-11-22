using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCFlyBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hands")
        {
            if (UIManager.Instance.HowToPlayUI.activeInHierarchy)
            {
                UIManager.Instance.StartGameLoopTrigger();
            }
        }
    }
}
