using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif 
public class BuildNumberUI : MonoBehaviour
{
    public Text buildNumber;
    public Text buildIncrement;
    public BuildNumberScriptableObject buildNumberScriptableObject;

#if !UNITY_EDITOR
    void Start()
    {
        if (buildNumber != null)
        {
            buildNumber.text = $"v{Application.version}";
        }
        if (buildIncrement != null)
        {
            buildIncrement.text = buildNumberScriptableObject.buildIncrement.ToString();
        }
    }
#endif

#if UNITY_EDITOR
    private void Update()
    {
        if (!PlayerSettings.bundleVersion.Equals(buildNumberScriptableObject.buildNumber))
        {
            buildNumberScriptableObject.buildNumber = PlayerSettings.bundleVersion;
            if (buildNumber != null)
            {
                buildNumber.text = $"v{buildNumberScriptableObject.buildNumber}";
            }
            if (buildIncrement != null)
            {
                buildIncrement.text = buildNumberScriptableObject.buildIncrement.ToString();
            }
        }
    }
#endif
}
