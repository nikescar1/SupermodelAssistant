using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlElementUI : MonoBehaviour
{
    public Text inputNameLabel;
    public Dropdown axisSideDropdown;
    public Button mapButton;
    public Button addMappingButton;
    public Text assignedInputLabel;
    public Button clearMappingButton;
    public Toggle toggle;
    public Toggle invertToggle;
    public SupermodelInputs.SupermodelInputType inputType;
    public bool isAnalog;
    public bool isToggle;

    public void Setup(string inputName, bool isAnalog, bool isInverted, int axisSide, string mappedName, SupermodelInputs.SupermodelInputType inputType, NewInputTesting newInputTesting)
    {
        this.isAnalog = isAnalog;
        inputNameLabel.text = inputName;
        if (isAnalog)
        {
            axisSideDropdown.gameObject.SetActive(true);
            invertToggle.gameObject.SetActive(true);

            if (isInverted)
            {
                invertToggle.isOn = true;
            }

            axisSideDropdown.value = axisSide;
        }
        else
        {
            axisSideDropdown.gameObject.SetActive(false);
            invertToggle.gameObject.SetActive(false);
        }
        assignedInputLabel.text = mappedName;
        this.inputType = inputType;

        mapButton.onClick.AddListener(delegate { StartMappingPressed(newInputTesting); });
        addMappingButton.onClick.AddListener(delegate { StartAddMappingPressed(newInputTesting); });
        clearMappingButton.onClick.AddListener(delegate { newInputTesting.ClearMapping(inputType, this); });
        invertToggle.onValueChanged.AddListener(delegate { newInputTesting.SetAxisInverted(inputType, this); });
        axisSideDropdown.onValueChanged.AddListener(delegate { newInputTesting.SetAxisInverted(inputType, this); });
    }

    public void Setup(string inputName, bool turnOn, SupermodelInputs.SupermodelInputType inputType, NewInputTesting newInputTesting)
    {
        this.inputType = inputType;
        isToggle = true;
        mapButton.gameObject.SetActive(false);
        addMappingButton.gameObject.SetActive(false);
        clearMappingButton.gameObject.SetActive(false);
        axisSideDropdown.gameObject.SetActive(false);
        invertToggle.gameObject.SetActive(false);
        toggle.gameObject.SetActive(true);
        toggle.isOn = turnOn;
        inputNameLabel.text = inputName;
        toggle.onValueChanged.AddListener(delegate { newInputTesting.SetToggle(inputType, this); });
    }

    public void StartMappingPressed(NewInputTesting newInputTesting)
    {
        StartCoroutine(DelayStartMapping(newInputTesting));
    }

    IEnumerator DelayStartMapping(NewInputTesting newInputTesting)
    {
        yield return null;
        newInputTesting.StartMapping(inputType, this);
    }

    public void StartAddMappingPressed(NewInputTesting newInputTesting)
    {
        StartCoroutine(DelayStartAddMapping(newInputTesting));
    }

    IEnumerator DelayStartAddMapping(NewInputTesting newInputTesting)
    {
        yield return null;
        newInputTesting.StartAddMapping(inputType, this);
    }
}
