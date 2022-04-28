
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class NewInputTesting : MonoBehaviour
{
    //public List<Gamepad> gamepads = new List<Gamepad>();
    //public List<Joystick> joysticks = new List<Joystick>();

    public List<InputDevice> joysticksAndGamepads = new List<InputDevice>();
    public List<Mouse> mice = new List<Mouse>();

    private List<string> systemControls = new List<string>() { "escape", "f5", "f7", "f6", "f9", "f10", "f11", "f12" };

    public bool checkInputs = false;
    public bool addingInput = false;

    public static string supermodelIniPath = @"Config\Supermodel.ini";

    public List<string> supermodelIniStrings = new List<string>();
    public List<string> supermodelBackupIniStrings = new List<string>();

    public SupermodelInputs.SupermodelInputType currentInputType;
    public ControlElementUI currentControlElementUI;
    public SupermodelInputs.SupermodelInputAxes currentInputAxis;
    public ControlAxisTuningElementUI currentControlAxisTuningElementUI;

    private int startingIndexForCommonControls = 0;

    private const float defaultThreshold = 0.25f;
    private const float defaultMouseThreshold = 2f;

    private List<DirectInputDevices> directInputDevices = new List<DirectInputDevices>();
    private List<SharpDX.DirectInput.Joystick> sharpDxJoysticks = new List<SharpDX.DirectInput.Joystick>();

    [System.Serializable]
    public class DirectInputDevices
    {
        public string name;
        public string type;
    }

    private void PopulateAllDevices()
    {
        SharpDX.DirectInput.DirectInput directInput = new SharpDX.DirectInput.DirectInput();
        IList<SharpDX.DirectInput.DeviceInstance> devList = directInput.GetDevices(SharpDX.DirectInput.DeviceClass.GameControl, SharpDX.DirectInput.DeviceEnumerationFlags.AttachedOnly);
        for (int i = 0; i < devList.Count; i++)
        {
            Debug.Log($"{devList[i].ProductName}");//  {devList[i].ProductName}  {devList[i].Type.ToString()} {devList[i].InstanceGuid} {devList[i].ProductGuid}");
                                                   //SharpDX.DirectInput.Joystick stick = new SharpDX.DirectInput.Joystick(directInput, devList[i].InstanceGuid);

            
            directInputDevices.Add(new DirectInputDevices() { name = devList[i].ProductName, type = devList[i].Type.ToString() });
            SharpDX.DirectInput.Joystick stick = new SharpDX.DirectInput.Joystick(directInput, devList[i].InstanceGuid);
            stick.Properties.BufferSize = 128;
            stick.Properties.DeadZone = (int)(defaultThreshold * 10000);
            stick.Acquire();
            sharpDxJoysticks.Add(stick);
            /*
            Debug.Log(stick.Information.InstanceName);
            Debug.Log(stick.Information.ProductName);
            Debug.Log(stick.Information.InstanceGuid);
            Debug.Log(stick.Information.ProductGuid);
            Debug.Log(stick.Information.Type.ToString());
            */
        }

        List<InputDevice> tempDevices = new List<InputDevice>();

        for (int i = 0; i < directInputDevices.Count; i++)
        {
            foreach (var item in joysticksAndGamepads)
            {
                if (directInputDevices[i].name.Contains("XBOX") && item.name.Contains("XInputControllerWindows"))
                {
                    tempDevices.Add(item);
                }
                else if (directInputDevices[i].name.Contains("USB Gamepad") && item.name.Contains("USB Gamepad 1"))
                {
                    tempDevices.Add(item);
                }
                else if (directInputDevices[i].name.Contains("USB Gamepad") && item.name.Contains("USB Gamepad 2"))
                {
                    tempDevices.Add(item);
                }
                else if (directInputDevices[i].name.Contains("USB Gamepad") && item.name.Contains("USB Gamepad 3"))
                {
                    tempDevices.Add(item);
                }
                else if (directInputDevices[i].name.Contains("USB Gamepad") && item.name.Contains("USB Gamepad 4"))
                {
                    tempDevices.Add(item);
                }
            }
        }

    }

    void Awake()
    {
        Debug.Log("");
        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            Gamepad gamepad = InputSystem.devices[i] as Gamepad;
            Mouse mouse = InputSystem.devices[i] as Mouse;

            if (mouse == null && (gamepad != null || InputSystem.devices[i].GetType() == typeof(Joystick)))
            {
                joysticksAndGamepads.Add(InputSystem.devices[i]);
                Debug.Log($"{i}: {InputSystem.devices[i].name}  {InputSystem.devices[i].description}  {InputSystem.devices[i].deviceId}  {InputSystem.devices[i].GetType().ToString()}   {InputSystem.devices[i].path}");


                Debug.Log($"{InputSystem.devices[i].description.capabilities}");
                Debug.Log($"{InputSystem.devices[i].description.deviceClass}");
                Debug.Log($"{InputSystem.devices[i].description.interfaceName}");
                Debug.Log($"{InputSystem.devices[i].description.manufacturer}");
                Debug.Log($"{InputSystem.devices[i].description.product}");
                Debug.Log($"{InputSystem.devices[i].description.serial}");
                Debug.Log($"{InputSystem.devices[i].description.version}");
            }
            else if (mouse != null)
            {
                mice.Add(mouse);
            }
        }

        PopulateAllDevices();

        BackupSupermodelIni();
        LoadSupermodelIni();
    }

    void Update()
    {
        if (checkInputs)
        {
            for (int i = 0; i < mice.Count; i++)
            {
                Mouse mouse = mice[i];

                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    EditSupermodelIni(currentInputType, $"MOUSE_LEFT_BUTTON");
                    checkInputs = false;
                    return;
                }
                if (mouse.middleButton.wasReleasedThisFrame)
                {
                    EditSupermodelIni(currentInputType, $"MOUSE_MIDDLE_BUTTON");
                    checkInputs = false;
                    return;
                }
                if (mouse.rightButton.wasReleasedThisFrame)
                {
                    EditSupermodelIni(currentInputType, $"MOUSE_RIGHT_BUTTON");
                    checkInputs = false;
                    return;
                }

                Vector2 valueDeltaMouse = mouse.delta.ReadValue();

                //Debug.Log($"Mouse: {valueDeltaMouse}");
                if (!IsCurrentInputTypeDigital(currentInputType)) //currentControlElementUI.isAnalog)// 
                {
                    if (((valueDeltaMouse.x > 0 && valueDeltaMouse.x > valueDeltaMouse.y) || (valueDeltaMouse.x < 0 && valueDeltaMouse.x < valueDeltaMouse.y)) && (valueDeltaMouse.x > defaultMouseThreshold || valueDeltaMouse.x < -defaultMouseThreshold))
                    {
                        EditSupermodelIni(currentInputType, $"MOUSE_XAXIS");
                        checkInputs = false;
                        Debug.Log($"MouseX: {valueDeltaMouse.x}");
                        return;
                    }
                    else if (((valueDeltaMouse.y > 0 && valueDeltaMouse.y > valueDeltaMouse.x) || (valueDeltaMouse.y < 0 && valueDeltaMouse.y < valueDeltaMouse.x)) && (valueDeltaMouse.y > defaultMouseThreshold || valueDeltaMouse.y < -defaultMouseThreshold))
                    {
                        EditSupermodelIni(currentInputType, $"MOUSE_YAXIS");
                        checkInputs = false;
                        Debug.Log($"MouseY: {valueDeltaMouse.y}");
                        return;
                    }
                }
            }

            //int gamepadNumberPressed = GetGamepadNumber(-1);
            for (int i = 0; i < joysticksAndGamepads.Count; i++)
            {
                Gamepad gamepad = joysticksAndGamepads[i] as Gamepad;

                if (gamepad != null)
                {
                    //int gamepadNumber = i + 1;

                    if (gamepad.aButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON1");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.bButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON2");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.xButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON3");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.yButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON4");
                        checkInputs = false;
                        return;
                    }

                    if (gamepad.crossButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON1");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.circleButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON2");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.squareButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON3");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.triangleButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON4");
                        checkInputs = false;
                        return;
                    }

                    if (gamepad.buttonSouth.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON1");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.buttonEast.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON2");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.buttonWest.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON3");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.buttonNorth.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON4");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.leftShoulder.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON5");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.rightShoulder.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON6");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.selectButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON7");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.startButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON8");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.leftStickButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON9");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.rightStickButton.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON10");
                        checkInputs = false;
                        return;
                    }

                    if (gamepad.dpad.down.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_POV1_DOWN");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.dpad.up.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_POV1_UP");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.dpad.left.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_POV1_LEFT");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.dpad.right.wasPressedThisFrame)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_POV1_RIGHT");
                        checkInputs = false;
                        return;
                    }

                    if (gamepad.leftStick.ReadValue().x > defaultThreshold || gamepad.leftStick.ReadValue().x < -defaultThreshold)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_XAXIS");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.leftStick.ReadValue().y > defaultThreshold || gamepad.leftStick.ReadValue().y < -defaultThreshold)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_YAXIS");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.rightStick.ReadValue().x > defaultThreshold || gamepad.rightStick.ReadValue().x < -defaultThreshold)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_RXAXIS");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.rightStick.ReadValue().y > defaultThreshold || gamepad.rightStick.ReadValue().y < -defaultThreshold)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_RYAXIS");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.leftTrigger.ReadValue() > defaultThreshold)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_ZAXIS_POS");
                        checkInputs = false;
                        return;
                    }
                    if (gamepad.rightTrigger.ReadValue() > defaultThreshold)
                    {
                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_ZAXIS_NEG");
                        checkInputs = false;
                        return;
                    }
                }
                else if (joysticksAndGamepads[i].GetType() == typeof(Joystick))
                {
                    //int GetGamepadNumber() = i + 1;
                    Joystick joystick = (Joystick)joysticksAndGamepads[i];

                    for (int h = 0; h < joystick.allControls.Count; h++)
                    {
                        switch (joystick.allControls[h].name)
                        {
                            case "stick":
                                Vector2 valueStick = (Vector2)joystick.allControls[h].ReadValueAsObject();

                                //Debug.Log($"StickY: {valueStick}");
                                if (IsCurrentInputTypeDigital(currentInputType)) // !currentControlElementUI.isAnalog)// 
                                {
                                    if (valueStick.x > defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_RIGHT");
                                        checkInputs = false;
                                        return;
                                    }
                                    else if (valueStick.x < -defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_LEFT");
                                        checkInputs = false;
                                        return;
                                    }
                                    else if (valueStick.y > defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_UP");
                                        checkInputs = false;
                                        return;
                                    }
                                    else if (valueStick.y < -defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_DOWN");
                                        checkInputs = false;
                                        return;
                                    }
                                }
                                else if (((valueStick.x > 0 && valueStick.x > valueStick.y) || (valueStick.x < 0 && valueStick.x < valueStick.y)) && (valueStick.x > defaultThreshold || valueStick.x < -defaultThreshold))
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_XAXIS");
                                    checkInputs = false;
                                    //Debug.Log($"StickX: {valueStick}");
                                    return;
                                }
                                else if (((valueStick.y > 0 && valueStick.y > valueStick.x) || (valueStick.y < 0 && valueStick.y < valueStick.x)) && (valueStick.y > defaultThreshold || valueStick.y < -defaultThreshold))
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_YAXIS");
                                    checkInputs = false;
                                    //Debug.Log($"StickY: {valueStick}");
                                    return;
                                }
                                break;
                            case "x":
                                float valueX = (float)joystick.allControls[h].ReadValueAsObject();
                                if (IsCurrentInputTypeDigital(currentInputType)) // !currentControlElementUI.isAnalog)// 
                                {
                                    if (valueX > defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_RIGHT");
                                        checkInputs = false;
                                        return;
                                    }
                                    else if (valueX < -defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_LEFT");
                                        checkInputs = false;
                                        return;
                                    }
                                }
                                else if (valueX > defaultThreshold || valueX < -defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_XAXIS");
                                    checkInputs = false;
                                    //Debug.Log($"Y: {valueX}");
                                    return;
                                }
                                break;
                            case "y":
                                float valueY = (float)joystick.allControls[h].ReadValueAsObject();
                                if (IsCurrentInputTypeDigital(currentInputType)) // !currentControlElementUI.isAnalog)// 
                                {
                                    if (valueY > defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_UP");
                                        checkInputs = false;
                                        return;
                                    }
                                    else if (valueY < -defaultThreshold)
                                    {
                                        EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_DOWN");
                                        checkInputs = false;
                                        return;
                                    }
                                }
                                if (valueY > defaultThreshold || valueY < -defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_YAXIS");
                                    checkInputs = false;
                                    //Debug.Log($"Y: {valueY}");
                                    return;
                                }
                                break;
                            case "up":
                                float valueUp = (float)joystick.allControls[h].ReadValueAsObject();
                                if (valueUp > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_UP");
                                    checkInputs = false;
                                    //Debug.Log($"up: {valueUp}");
                                    return;
                                }
                                break;
                            case "down":
                                float valueDown = (float)joystick.allControls[h].ReadValueAsObject();
                                if (valueDown > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_DOWN");
                                    checkInputs = false;
                                    //Debug.Log($"down: {valueDown}");
                                    return;
                                }
                                break;
                            case "left":
                                float valueLeft = (float)joystick.allControls[h].ReadValueAsObject();
                                if (valueLeft > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_LEFT");
                                    checkInputs = false;
                                    //Debug.Log($"left: {valueLeft}");
                                    return;
                                }
                                break;
                            case "right":
                                float valueRight = (float)joystick.allControls[h].ReadValueAsObject();
                                if (valueRight > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_RIGHT");
                                    checkInputs = false;
                                    //Debug.Log($"right: {valueRight}");
                                    return;
                                }
                                break;
                            case "trigger":
                                float valueTrigger = (float)joystick.allControls[h].ReadValueAsObject();
                                if (valueTrigger > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON1");
                                    checkInputs = false;
                                    //Debug.Log($"trigger: {valueTrigger}");
                                    return;
                                }
                                break;
                            case "button1":
                                float value1 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value1 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON1");
                                    checkInputs = false;
                                    //Debug.Log($"1: {value1}");
                                    return;
                                }
                                break;
                            case "button2":
                                float value2 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value2 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON2");
                                    checkInputs = false;
                                    //Debug.Log($"2: {value2}");
                                    return;
                                }
                                break;
                            case "button3":
                                float value3 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value3 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON3");
                                    checkInputs = false;
                                    //Debug.Log($"3: {value3}");
                                    return;
                                }
                                break;
                            case "button4":
                                float value4 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value4 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON4");
                                    checkInputs = false;
                                    //Debug.Log($"4: {value4}");
                                    return;
                                }
                                break;
                            case "button5":
                                float value5 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value5 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON5");
                                    checkInputs = false;
                                    //Debug.Log($"5: {value5}");
                                    return;
                                }
                                break;
                            case "button6":
                                float value6 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value6 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON6");
                                    checkInputs = false;
                                    //Debug.Log($"6: {value6}");
                                    return;
                                }
                                break;
                            case "button7":
                                float value7 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value7 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON7");
                                    checkInputs = false;
                                    //Debug.Log($"7: {value7}");
                                    return;
                                }
                                break;
                            case "button8":
                                float value8 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value8 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON8");
                                    checkInputs = false;
                                    //Debug.Log($"8: {value8}");
                                    return;
                                }
                                break;
                            case "button9":
                                float value9 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value9 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON9");
                                    checkInputs = false;
                                    //Debug.Log($"9: {value9}");
                                    return;
                                }
                                break;
                            case "button10":
                                float value10 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value10 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON10");
                                    checkInputs = false;
                                    //Debug.Log($"10: {value10}");
                                    return;
                                }
                                break;
                            case "button11":
                                float value11 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value11 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON11");
                                    checkInputs = false;
                                    //Debug.Log($"11: {value11}");
                                    return;
                                }
                                break;
                            case "button12":
                                float value12 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value12 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON12");
                                    checkInputs = false;
                                    //Debug.Log($"12: {value12}");
                                    return;
                                }
                                break;
                            case "button13":
                                float value13 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value13 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON13");
                                    checkInputs = false;
                                    //Debug.Log($"13: {value13}");
                                    return;
                                }
                                break;
                            case "button14":
                                float value14 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value14 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON14");
                                    checkInputs = false;
                                    //Debug.Log($"14: {value14}");
                                    return;
                                }
                                break;
                            case "button15":
                                float value15 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value15 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON15");
                                    checkInputs = false;
                                    //Debug.Log($"15: {value15}");
                                    return;
                                }
                                break;
                            case "button16":
                                float value16 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value16 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON16");
                                    checkInputs = false;
                                    //Debug.Log($"16: {value16}");
                                    return;
                                }
                                break;
                            case "button17":
                                float value17 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value17 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON17");
                                    checkInputs = false;
                                    //Debug.Log($"17: {value17}");
                                    return;
                                }
                                break;
                            case "button18":
                                float value18 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value18 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON18");
                                    checkInputs = false;
                                    //Debug.Log($"18: {value18}");
                                    return;
                                }
                                break;
                            case "button19":
                                float value19 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value19 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON19");
                                    checkInputs = false;
                                    //Debug.Log($"19: {value19}");
                                    return;
                                }
                                break;
                            case "button20":
                                float value20 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value20 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON20");
                                    checkInputs = false;
                                    //Debug.Log($"20: {value20}");
                                    return;
                                }
                                break;
                            case "button21":
                                float value21 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value21 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON21");
                                    checkInputs = false;
                                    //Debug.Log($"21: {value21}");
                                    return;
                                }
                                break;
                            case "button22":
                                float value22 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value22 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON22");
                                    checkInputs = false;
                                    //Debug.Log($"22: {value22}");
                                    return;
                                }
                                break;
                            case "button23":
                                float value23 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value23 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON23");
                                    checkInputs = false;
                                    //Debug.Log($"23: {value23}");
                                    return;
                                }
                                break;
                            case "button24":
                                float value24 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value24 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON24");
                                    checkInputs = false;
                                    //Debug.Log($"24: {value24}");
                                    return;
                                }
                                break;
                            case "button25":
                                float value25 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value25 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON25");
                                    checkInputs = false;
                                    //Debug.Log($"25: {value25}");
                                    return;
                                }
                                break;
                            case "button26":
                                float value26 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value26 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON26");
                                    checkInputs = false;
                                    //Debug.Log($"26: {value26}");
                                    return;
                                }
                                break;
                            case "button27":
                                float value27 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value27 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON27");
                                    checkInputs = false;
                                    //Debug.Log($"27: {value27}");
                                    return;
                                }
                                break;
                            case "button28":
                                float value28 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value28 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON28");
                                    checkInputs = false;
                                    //Debug.Log($"28: {value28}");
                                    return;
                                }
                                break;
                            case "button29":
                                float value29 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value29 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON29");
                                    checkInputs = false;
                                    //Debug.Log($"29: {value29}");
                                    return;
                                }
                                break;
                            case "button30":
                                float value30 = (float)joystick.allControls[h].ReadValueAsObject();
                                if (value30 > defaultThreshold)
                                {
                                    EditSupermodelIni(currentInputType, $"JOY{GetGamepadNumber()}_BUTTON30");
                                    checkInputs = false;
                                    //Debug.Log($"30: {value30}");
                                    return;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    checkInputs = false;
                    foreach (var systemControl in systemControls)
                    {
                        if (key.name.Equals(systemControl))
                        {
                            return;
                        }
                    }

                    string keyName = key.name.ToUpper();
                    if (keyName.Contains("ARROW"))
                    {
                        keyName = keyName.Replace("ARROW", "");
                    }
                    EditSupermodelIni(currentInputType, $"KEY_{keyName}");
                    return;
                }
            }
        }
    }

    private int GetGamepadNumber()
    {
        //if (number == -1)
        {
            int gamepadNumberPressed = -1;
            for (int h = 0; h < sharpDxJoysticks.Count; h++)
            {
                sharpDxJoysticks[h].Poll();
                var data = sharpDxJoysticks[h].GetBufferedData();
                string dataString = $"Gamepad #{h + 1} - {sharpDxJoysticks[h].Information.InstanceName}: \n";
                bool hasData = false;
                foreach (var state in data)
                {
                    //state.
                    hasData = true;
                    String OffsetName = Enum.GetName(typeof(SharpDX.DirectInput.JoystickOffset), state.Offset);
                    dataString += $"{OffsetName} State: {state.Value}\n";
                }
                if (hasData)
                {
                    gamepadNumberPressed = h+1;
                    Debug.Log(dataString);
                }
            }
            return gamepadNumberPressed;
        }
        //else
        {
            //return number;
        }
    }

    public bool IsCurrentInputTypeDigital(SupermodelInputs.SupermodelInputType inputType)
    {
        if (currentInputType == SupermodelInputs.SupermodelInputType.InputJoyDown
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyDown2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyDown1
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyDown2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyUp
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyUp2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyUp1
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyUp2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyLeft
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyLeft2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyLeft1
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyLeft2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyRight
            || currentInputType == SupermodelInputs.SupermodelInputType.InputJoyRight2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyRight1
            || currentInputType == SupermodelInputs.SupermodelInputType.InputTwinJoyRight2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunLeft
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunRight
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunUp
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunDown
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunLeft2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunRight2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunUp2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputAnalogGunDown2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputSkiLeft
            || currentInputType == SupermodelInputs.SupermodelInputType.InputSkiRight
            || currentInputType == SupermodelInputs.SupermodelInputType.InputSkiUp
            || currentInputType == SupermodelInputs.SupermodelInputType.InputSkiDown
            || currentInputType == SupermodelInputs.SupermodelInputType.InputMagicalLeverUp1
            || currentInputType == SupermodelInputs.SupermodelInputType.InputMagicalLeverDown1
            || currentInputType == SupermodelInputs.SupermodelInputType.InputMagicalLeverUp2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputMagicalLeverDown2
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingRodLeft
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingRodRight
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingRodUp
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingRodDown
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingStickLeft
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingStickRight
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingStickUp
            || currentInputType == SupermodelInputs.SupermodelInputType.InputFishingStickDown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void StartMapping(SupermodelInputs.SupermodelInputType inputType, ControlElementUI controlElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentInputType = inputType;
        currentControlElementUI = controlElementUI;
        controlElementUI.assignedInputLabel.text = "Scanning Inputs...";
        currentControlElementUI.mapButton.interactable = false;
        checkInputs = true;
        addingInput = false;
    }

    public void StartAddMapping(SupermodelInputs.SupermodelInputType inputType, ControlElementUI controlElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentInputType = inputType;
        currentControlElementUI = controlElementUI;
        controlElementUI.assignedInputLabel.text = "Scanning Inputs...";
        currentControlElementUI.mapButton.interactable = false;
        checkInputs = true;
        addingInput = true;
    }

    public void ClearMapping(SupermodelInputs.SupermodelInputType inputType, ControlElementUI controlElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentInputType = inputType;
        currentControlElementUI = controlElementUI;
        currentControlElementUI.mapButton.interactable = true;
        checkInputs = false;
        EditSupermodelIni(currentInputType, $"NONE");
    }

    public void SetAxisInverted(SupermodelInputs.SupermodelInputType inputType, ControlElementUI controlElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentInputType = inputType;
        currentControlElementUI = controlElementUI;
        checkInputs = false;

        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputType.ToString()} = "))
            {
                string value = supermodelIniStrings[i].Split(new char[] { '=', ';' })[1].Replace("\"", "").Trim();
                if (!value.Contains("NONE"))
                {
                    if (currentControlElementUI.invertToggle.isOn && !value.Contains("_INV"))
                    {
                        value = $"{value}_INV";
                    }
                    else if (!currentControlElementUI.invertToggle.isOn && value.Contains("_INV"))
                    {
                        value = value.Replace("_INV", "");
                    }
                    EditSupermodelIni(currentInputType, value);
                }
                return;
            }
        }
    }

    public void SetToggle(SupermodelInputs.SupermodelInputType inputType, ControlElementUI controlElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentInputType = inputType;
        currentControlElementUI = controlElementUI;
        checkInputs = false;

        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputType.ToString()} = "))
            {
                string value = supermodelIniStrings[i].Split(new char[] { '=', ';' })[1].Replace("\"", "").Trim();
                if (value.Equals("0") || value.Equals("1"))
                {
                    if (currentControlElementUI.toggle.isOn)
                    {
                        value = "1";
                    }
                    else
                    {
                        value = "0";
                    }
                    EditSupermodelIni(currentInputType, value);
                }
                return;
            }
        }
    }

    public void SetAxisSide(SupermodelInputs.SupermodelInputType inputType, ControlElementUI controlElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentInputType = inputType;
        currentControlElementUI = controlElementUI;
        checkInputs = false;

        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputType.ToString()} = "))
            {
                string value = supermodelIniStrings[i].Split(new char[] { '=', ';' })[1].Replace("\"", "").Trim();
                if (!value.Contains("NONE"))
                {
                    value = value.Replace("_POS", "").Replace("_NEG", "");

                    switch (currentControlElementUI.axisSideDropdown.value)
                    {
                        case 0:
                            break;
                        case 1:
                            if (!value.Contains("_POS"))
                            {
                                value = $"{value}_POS";
                            }
                            break;
                        case 2:
                            if (!value.Contains("_NEG"))
                            {
                                value = $"{value}_NEG";
                            }
                            break;
                    }

                    EditSupermodelIni(currentInputType, value);
                }
                return;
            }
        }
    }

    public void SetSensitivity(SupermodelInputs.SupermodelInputAxes inputAxis, ControlAxisTuningElementUI controlAxisTuningElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentControlAxisTuningElementUI = controlAxisTuningElementUI;
        currentInputAxis = inputAxis;
        string saturationString = $"{inputAxis.ToString()}Saturation = {controlAxisTuningElementUI.GetSaturation()}";
        string minValueString = $"{inputAxis.ToString()}MinVal = {controlAxisTuningElementUI.GetMinVal()}";
        string maxValueString = $"{inputAxis.ToString()}MaxVal = {controlAxisTuningElementUI.GetMaxVal()}";
        bool hasSaturationEntry = false;
        bool hasMinValueEntry = false;
        bool hasMaxValueEntry = false;
        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputAxis.ToString()}Saturation"))
            {
                supermodelIniStrings[i] = saturationString;
                hasSaturationEntry = true;
            }
            if (supermodelIniStrings[i].Contains($"{inputAxis.ToString()}MinVal"))
            {
                supermodelIniStrings[i] = minValueString;
                hasMinValueEntry = true;
            }
            if (supermodelIniStrings[i].Contains($"{inputAxis.ToString()}MaxVal"))
            {
                supermodelIniStrings[i] = maxValueString;
                hasMaxValueEntry = true;
            }
        }

        if (!hasSaturationEntry)
        {
            supermodelIniStrings.Insert(startingIndexForCommonControls, saturationString);
        }
        if (!hasMinValueEntry)
        {
            supermodelIniStrings.Insert(startingIndexForCommonControls, minValueString);
        }
        if (!hasMaxValueEntry)
        {
            supermodelIniStrings.Insert(startingIndexForCommonControls, maxValueString);
        }

        SaveToSupermodelIni();
    }

    public void SetDeadzone(SupermodelInputs.SupermodelInputAxes inputAxis, ControlAxisTuningElementUI controlAxisTuningElementUI)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        currentControlAxisTuningElementUI = controlAxisTuningElementUI;
        currentInputAxis = inputAxis;
        string deadzoneString = $"{inputAxis.ToString()}DeadZone = {controlAxisTuningElementUI.GetDeadzone()}";
        bool hasDeadzoneEntry = false;
        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputAxis.ToString()}DeadZone"))
            {
                supermodelIniStrings[i] = deadzoneString;
                hasDeadzoneEntry = true;
            }
        }

        if (!hasDeadzoneEntry)
        {
            supermodelIniStrings.Insert(startingIndexForCommonControls, deadzoneString);
        }

        SaveToSupermodelIni();
    }

    private void BackupSupermodelIni()
    {
        string sourcePath = SupermodelLauncher.supermodelPath + supermodelIniPath;
        if (!File.Exists(sourcePath.Replace(".ini", "_backup.ini")))
        {
            File.Copy(sourcePath, sourcePath.Replace(".ini", "_backup.ini"));
        }
    }

    private void EditSupermodelIni(SupermodelInputs.SupermodelInputAxes inputAxis, string value)
    {
        bool hasInputAxisEntry = false;
        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputAxis.ToString()} = "))
            {
            }
        }
    }

    private void EditSupermodelIni(SupermodelInputs.SupermodelInputType inputType, string value)
    {
        if (currentControlElementUI)
        {
            currentControlElementUI.mapButton.interactable = true;
        }
        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains($"{inputType.ToString()} = "))
            {
                string comment = "";
                if (supermodelIniStrings[i].Contains(";"))
                {
                    comment = $"; {supermodelIniStrings[i].Split(';')[1]}";
                }

                if (currentControlElementUI.isToggle)
                {
                    supermodelIniStrings[i] = $"{inputType.ToString()} = {value} {comment}";
                }
                else
                {
                    if (currentControlElementUI.isAnalog && !value.Contains("NONE"))
                    {
                        value = value.Replace("_POS", "").Replace("_NEG", "");
                        switch (currentControlElementUI.axisSideDropdown.value)
                        {
                            case 0:
                                break;
                            case 1:
                                if (!value.Contains("_POS"))
                                {
                                    value = $"{value}_POS";
                                }
                                break;
                            case 2:
                                if (!value.Contains("_NEG"))
                                {
                                    value = $"{value}_NEG";
                                }
                                break;
                        }

                        if (currentControlElementUI.invertToggle.isOn && !value.Contains("_INV"))
                        {
                            value = $"{value}_INV";
                        }
                        else if (!currentControlElementUI.invertToggle.isOn && value.Contains("_INV"))
                        {
                            value = value.Replace("_INV", "");
                        }
                    }

                    currentControlElementUI.assignedInputLabel.text = value;
                    supermodelIniStrings[i] = $"{inputType.ToString()} = \"{value}\" {comment}";
                }
            }
        }
        SaveToSupermodelIni();
    }

    private void SaveToSupermodelIni()
    {
        string path = SupermodelLauncher.supermodelPath + supermodelIniPath;
        StreamWriter writer = new StreamWriter(path);

        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            writer.WriteLine(supermodelIniStrings[i]);
        }
        writer.Close();
    }

    private void LoadSupermodelIni()
    {
        string path = SupermodelLauncher.supermodelPath + supermodelIniPath;
        StreamReader reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            supermodelIniStrings.Add(reader.ReadLine());
        }
        reader.Close();

        StreamReader readerBackup = new StreamReader(path.Replace(".ini", "_backup.ini"));
        while (!readerBackup.EndOfStream)
        {
            supermodelBackupIniStrings.Add(readerBackup.ReadLine());
        }
        readerBackup.Close();

        for (int i = 0; i < supermodelIniStrings.Count; i++)
        {
            if (supermodelIniStrings[i].Contains("; Common") || supermodelIniStrings[i].Contains(";Common"))
            {
                startingIndexForCommonControls = i - 1;
            }
        }

        bool hasAxisTuning = false;
        foreach (var entry in supermodelIniStrings)
        {
            if (entry.Contains("; Axis Tuning"))
            {
                hasAxisTuning = true;
                break;
            }
        }

        if (!hasAxisTuning)
        {
            supermodelIniStrings.Insert(startingIndexForCommonControls, "\n; Axis Tuning");
            startingIndexForCommonControls += 1;
        }
        else
        {
            startingIndexForCommonControls -= 1;
        }
    }
}