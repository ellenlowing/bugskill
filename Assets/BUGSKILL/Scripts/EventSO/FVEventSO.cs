using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/FVEvent")]
public class FVEventSO : ScriptableObject
{
    public FVEvent OnEventRaised;
    public void RaiseEvent(float value, Vector3 position)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(value, position);
        }
    }
}

[System.Serializable]
public class FVEvent : UnityEvent<float, Vector3> { }