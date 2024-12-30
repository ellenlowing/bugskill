using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class PokeButtonHandler : MonoBehaviour
{
    public InteractableUnityEventWrapper PokeButtonEvent;

    void Awake()
    {
        PokeButtonEvent = GetComponent<InteractableUnityEventWrapper>();
    }

    void Start()
    {
        PokeButtonEvent.WhenHover.AddListener(() =>
        {
            GameManager.Instance.SetHandVisualsActive(true);
        });

        PokeButtonEvent.WhenUnhover.AddListener(() =>
        {
            GameManager.Instance.SetHandVisualsActive(false);
        });
    }

    void Update()
    {

    }
}
