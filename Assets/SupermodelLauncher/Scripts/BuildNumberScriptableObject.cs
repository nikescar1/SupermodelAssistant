using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

[CreateAssetMenu(fileName = "BuildNumber", menuName = "Build Number", order = 1)]
public class BuildNumberScriptableObject : ScriptableObject
{
    public string buildNumber;
    public int buildIncrement = 100;

    public int callbackOrder { get { return 0; } }
    /*
#if UNITY_EDITOR
    public void OnPreprocessBuild(BuildReport report)
    {
        buildIncrement++;
        SetDirty();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
*/
}
