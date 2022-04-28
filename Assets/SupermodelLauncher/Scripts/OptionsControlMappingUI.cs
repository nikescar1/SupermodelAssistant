using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OptionsControlMappingUI : MonoBehaviour
{
    public GameObject controlElementPrefab;
    public GameObject controlCategoryElementPrefab;
    public GameObject controlSpacerElementPrefab;
    public NewInputTesting newInputTesting;
    public Transform contentTransform;
    public List<ControlAxisTuningElementUI> axisTuningElements = new List<ControlAxisTuningElementUI>();
    
    public Toggle toggleJoystick1;
    public Toggle toggleJoystick2;
    public Toggle toggleJoystick3;
    public Toggle toggleJoystick4;

    void Start()
    {
        SetupAxisTuning();
        SpawnControlElements();
        SetupJoystickToggles();
    }

    private void SetupJoystickToggles()
    {
        toggleJoystick1.onValueChanged.AddListener(delegate { ToggleJoystickChanged(1); });
        toggleJoystick2.onValueChanged.AddListener(delegate { ToggleJoystickChanged(2); });
        toggleJoystick3.onValueChanged.AddListener(delegate { ToggleJoystickChanged(3); });
        toggleJoystick4.onValueChanged.AddListener(delegate { ToggleJoystickChanged(4); });
    }

    private void ToggleJoystickChanged(int index)
    {
        switch (index)
        {
            case 1:
                axisTuningElements[0].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy1X;
                axisTuningElements[1].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy1Y;
                axisTuningElements[2].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy1RX;
                axisTuningElements[3].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy1RY;
                axisTuningElements[4].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy1Z;
                break;
            case 2:
                axisTuningElements[0].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy2X;
                axisTuningElements[1].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy2Y;
                axisTuningElements[2].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy2RX;
                axisTuningElements[3].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy2RY;
                axisTuningElements[4].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy2Z;
                break;
            case 3:
                axisTuningElements[0].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy3X;
                axisTuningElements[1].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy3Y;
                axisTuningElements[2].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy3RX;
                axisTuningElements[3].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy3RY;
                axisTuningElements[4].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy3Z;
                break;
            case 4:
                axisTuningElements[0].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy4X;
                axisTuningElements[1].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy4Y;
                axisTuningElements[2].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy4RX;
                axisTuningElements[3].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy4RY;
                axisTuningElements[4].inputAxis = SupermodelInputs.SupermodelInputAxes.InputJoy4Z;
                break;
        }

        SetupAxisTuning();
    }

    private void SetupAxisTuning()
    {
        foreach (var element in axisTuningElements)
        {
            int saturation = 100;
            int deadzone = 2;

            foreach (var inputString in newInputTesting.supermodelIniStrings)
            {
                string saturationString = $"{element.inputAxis.ToString()}Saturation = ";
                if (inputString.Contains(saturationString))
                {
                    saturation = int.Parse(inputString.Replace(saturationString, ""));
                }

                string deadzoneString = $"{element.inputAxis.ToString()}DeadZone = ";
                if (inputString.Contains(deadzoneString))
                {
                    deadzone = int.Parse(inputString.Replace(deadzoneString, ""));
                }
            }

            element.Setup(saturation, deadzone, newInputTesting);
        }
    }

    private void SpawnControlElements()
    {
        bool hasReachedControls = false;
        bool isFirstEntry = true;
        
        foreach (var inputString in newInputTesting.supermodelIniStrings)
        {
            if (inputString.Contains("; Common") || inputString.Contains(";Common"))
            {
                hasReachedControls = true;
            }

            if (hasReachedControls)
            {
                //Debug.Log(inputString);
                if (inputString.Length >= 1 && inputString.Substring(0, 1).Equals(";"))
                {
                    if (!isFirstEntry)
                    {
                        Instantiate(controlSpacerElementPrefab, contentTransform);
                    }
                    GameObject controlCategoryElement = Instantiate(controlCategoryElementPrefab, contentTransform);
                    ControlCategoryElementUI controlCategoryElementUI = controlCategoryElement.GetComponent<ControlCategoryElementUI>();
                    controlCategoryElementUI.Setup(inputString.Substring(1, inputString.Length - 1).Trim());
                }
                else if (inputString.Length >= 5)// && inputString.Substring(0,5).Equals("Input"))
                {
                    GameObject controlElement = Instantiate(controlElementPrefab, contentTransform);
                    ControlElementUI controlElementUI = controlElement.GetComponent<ControlElementUI>();

                    bool isControlMapping = false;
                    string inputName = "";
                    bool isAnalog = IsInputAnalog(inputString);
                    string mappedName = "";
                    string[] inputStringsSplitEquals = inputString.Split('=');
                    controlElement.name = inputStringsSplitEquals[0];
                    inputName = AddSpacesToSentence(inputStringsSplitEquals[0]);
                    SupermodelInputs.SupermodelInputType inputType = (SupermodelInputs.SupermodelInputType)Enum.Parse(typeof(SupermodelInputs.SupermodelInputType), inputStringsSplitEquals[0]);
                    if (inputStringsSplitEquals[1].Contains("\""))
                    {
                        mappedName = GetMappedControl(inputStringsSplitEquals[0]);
                        bool isInverted = false;
                        if (mappedName.Contains("_INV"))
                        {
                            isInverted = true;
                        }
                        int axisSide = 0;
                        if (mappedName.Contains("_POS"))
                        {
                            axisSide = 1;
                        }
                        if (mappedName.Contains("_NEG"))
                        {
                            axisSide = 2;
                        }
                        controlElementUI.Setup(inputName, isAnalog, isInverted, axisSide, mappedName, inputType, newInputTesting);
                    }
                    else
                    {
                        string[] boolValueStrings = inputStringsSplitEquals[1].Split(';');
                        bool isTurnedOn = int.Parse(boolValueStrings[0]) != 0;
                        controlElementUI.Setup(inputName, isTurnedOn, inputType, newInputTesting);
                    }
                }

                isFirstEntry = false;
            }
        }
    }

    private string GetMappedControl(string inputString)
    {        
        foreach (var inputSetString in newInputTesting.supermodelIniStrings)
        {
            if (inputSetString.Contains("="))
            {
                string[] inputStringsSplitEquals = inputSetString.Split('=');
                if (inputStringsSplitEquals[0].Equals(inputString))
                {
                    if (inputStringsSplitEquals[1].Contains("\""))
                    {
                        string[] inputStringsSplitQuote = inputStringsSplitEquals[1].Split('"');
                        string[] assignedInputs = inputStringsSplitQuote[1].Split(',');
                        return assignedInputs[0];
                    }
                }
            }
        }

        return "";
    }

    private bool IsInputAnalog(string inputString)
    {
        if (inputString.Contains("analog") || inputString.Contains("AXIS"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    string AddSpacesToSentence(string text, bool preserveAcronyms = true)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }
}
