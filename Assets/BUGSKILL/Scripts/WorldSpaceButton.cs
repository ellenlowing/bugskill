using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldSpaceButton : MonoBehaviour
{
    public float TouchDuration = 0.5f;
    public UnityEvent OnButtonPressed;
    public float RotationSpeed = 15f;
    private bool _isTouching = false;
    private float _firstTouchTime = Mathf.Infinity;
    private float _rotationY = 0;

    void Start()
    {

    }

    void Update()
    {
        if (!_isTouching)
        {
            transform.localEulerAngles = new Vector3(0, _rotationY, 0);
            _rotationY += Time.deltaTime * RotationSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hands")
        {
            _firstTouchTime = Time.time;
            _isTouching = true;
            transform.localEulerAngles = new Vector3(0, 0, 0);
            _rotationY = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Hands")
        {
            if (Time.time - _firstTouchTime > TouchDuration)
            {
                Debug.Log("Button Held");
                OnButtonPressed.Invoke();
                _firstTouchTime = Mathf.Infinity;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hands")
        {
            _firstTouchTime = Mathf.Infinity;
            _isTouching = false;
        }
    }
}
