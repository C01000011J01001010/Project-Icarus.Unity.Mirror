using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderBase : MonoBehaviour
{
    private Slider _slider;
    public Slider slider
    {
        get
        {
            if(_slider == null) _slider = GetComponent<Slider>();
            return _slider;
        }
    }

    public void SetSliderCallback(UnityAction<float> callback)
    {
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(callback);
    }
}
