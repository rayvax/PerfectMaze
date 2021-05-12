using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class SliderText : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    protected TMP_Text ValueText;

    private void Awake()
    {
        ValueText = GetComponent<TMP_Text>();
        ValueText.text = _slider.value.ToString();
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDisable()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    protected virtual void OnSliderValueChanged(float value)
    {
        ValueText.text = value.ToString();
    }
}
