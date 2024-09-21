using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogRunner : MonoBehaviour
{
    public List<string> DialogList;
    public TextMeshProUGUI DialogText;
    public float DialogDuration = 5f;
    private int _currentIndex = 0;
    private float _timer = 0;

    void Start()
    {
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DialogDuration)
        {
            DialogText.text = DialogList[_currentIndex];
            _currentIndex = (_currentIndex + 1) % DialogList.Count;
            _timer = 0;
        }
    }
}
