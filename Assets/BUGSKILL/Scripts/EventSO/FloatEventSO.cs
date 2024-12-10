using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/FloatEvent")]
public class FloatEventSO : ScriptableObject
{
    public FloatEvent OnEventRaised;

    public void RaiseEvent(float value)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(value);
        }
    }
}


public class FloatEvent : UnityEvent<float> { }


