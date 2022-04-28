using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlAxisTuningElementUI : MonoBehaviour
{
    public SupermodelInputs.SupermodelInputAxes inputAxis;
    public Slider sensitivitySlider;
    public Text sensitivityValueLabel;
    public Slider deadzoneSlider;
    public Text deadzoneValueLabel;

    private int saturationIndex;
    private int deadzoneIndex;
    private List<int> saturationValues = new List<int>() { 200, 180, 160, 140, 120, 100, 90, 80, 70, 60, 50 };
    private List<int> deadzoneValues = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

    private NewInputTesting newInputTesting;
    private bool shouldSendSaturationInputToSave = true;
    private bool shouldSendDeadzoneInputToSave = true;

    public void Setup(int saturation, int deadzone, NewInputTesting newInputTesting)
    {
        SetSensitivityValue(saturation);
        sensitivitySlider.value = saturationIndex;

        SetDeadzoneValue(deadzone);
        deadzoneSlider.value = deadzoneIndex;

        this.newInputTesting = newInputTesting;
        this.shouldSendSaturationInputToSave = false;
        this.shouldSendDeadzoneInputToSave = false;

        sensitivitySlider.onValueChanged.AddListener(delegate { SaturationValueChanged(); });
        deadzoneSlider.onValueChanged.AddListener(delegate { DeadzoneValueChanged(); });
    }

    private void SaturationValueChanged()
    {
        SetSensitivityValue(GetSaturation());
        if (shouldSendSaturationInputToSave)
        {
            newInputTesting.SetSensitivity(inputAxis, this);
        }
        else
        {
            shouldSendSaturationInputToSave = true;
        }
    }

    private void DeadzoneValueChanged()
    {
        SetDeadzoneValue(GetDeadzone());
        if (shouldSendDeadzoneInputToSave)
        {
            newInputTesting.SetDeadzone(inputAxis, this);
        }
        else
        {
            shouldSendDeadzoneInputToSave = true;
        }
    }

    private void SetSensitivityValue(int saturation)
    {
        sensitivityValueLabel.text = $"x{(100 / (float)saturation).ToString("0.00")}";
        saturationIndex = GetSensitivitySliderValue(saturation);
    }

    private void SetDeadzoneValue(int deadzone)
    {
        deadzoneValueLabel.text = $"{deadzone}%";
        deadzoneIndex = GetDeadzoneSliderValue(deadzone);
    }

    public int GetMinVal()
    {
        return (int)(-32768 * (100 / (float)saturationValues[saturationIndex]));
    }

    public int GetMaxVal()
    {
        return (int)(32767 * (100 / (float)saturationValues[saturationIndex]));
    }

    public int GetSaturation()
    {
        saturationIndex = (int)sensitivitySlider.value;
        return saturationValues[saturationIndex];
    }

    public int GetDeadzone()
    {
        deadzoneIndex = (int)deadzoneSlider.value;
        return deadzoneValues[deadzoneIndex];
    }

    private int GetSensitivitySliderValue(int saturation)
    {
        return saturationValues.IndexOf(saturation);
    }

    private int GetDeadzoneSliderValue(int deadzone)
    {
        return deadzoneValues.IndexOf(deadzone);
    }
}
