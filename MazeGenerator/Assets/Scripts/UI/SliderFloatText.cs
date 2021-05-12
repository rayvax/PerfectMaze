using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderFloatText : SliderText
{
    protected override void OnSliderValueChanged(float value)
    {
        value = (float)System.Math.Round(value, 2);
        base.OnSliderValueChanged(value);
    }
}
