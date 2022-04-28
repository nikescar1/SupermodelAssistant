using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlCategoryElementUI : MonoBehaviour
{
    public Text label;

    public void Setup(string labelName)
    {
        label.text = labelName;
    }
}
