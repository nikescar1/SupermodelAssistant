using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.UI;

public class LauncherSettings : MonoBehaviour
{
    public Settings settings = new Settings();

    public Toggle rotateGameFlyersToggle;
    public Toggle showGameTitlesOnSelectionScreenToggle;
    public Toggle coverFlowModeToggle;
    public Dropdown resolutionDropdown;

    private List<Resolution> resolutions = new List<Resolution>();

    public class Settings
    {
        public bool rotateGameFlyers = true;
        public bool showGameTitlesOnSelectionScreen = true;
        public bool useCoverFlowMode = false;
        public int resolutionIndex = 0;
    }
    
    private void Awake()
    {
        GetResolutions();
        Messaging.OnGetLauncherSettings += GetLauncherSettings;
    }
    
    private void OnEnable()
    {
        /*
        bool hasGetLauncherSettingsMethod = false;
        foreach (var item in Messaging.OnGetLauncherSettings.GetInvocationList())
        {
            if (item.Equals(GetLauncherSettings()))
            {
                hasGetLauncherSettingsMethod = true;
            }
        }

        if (!hasGetLauncherSettingsMethod)
        {
            Messaging.OnGetLauncherSettings += GetLauncherSettings;
        }
        */
        //Messaging.OnGetLauncherSettings += GetLauncherSettings;

        rotateGameFlyersToggle.onValueChanged.AddListener(ChangeRotateGameFlyers);
        showGameTitlesOnSelectionScreenToggle.onValueChanged.AddListener(ChangeShowGameTitlesOnSelectionScreen);
        coverFlowModeToggle.onValueChanged.AddListener(ChangeUseCoverFlowMode);
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
    }

    private void OnDisable()
    {
        Messaging.OnGetLauncherSettings -= GetLauncherSettings;
        rotateGameFlyersToggle.onValueChanged.RemoveListener(ChangeRotateGameFlyers);
        showGameTitlesOnSelectionScreenToggle.onValueChanged.RemoveListener(ChangeShowGameTitlesOnSelectionScreen);
        resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
    }

    private void Start()
    {
        ReadJsonFile();
    }

    private Settings GetLauncherSettings()
    {
        return settings;
    }

    public void ChangeRotateGameFlyers(bool value)
    {
        settings.rotateGameFlyers = value;
        WriteJsonFile();
        Messaging.OnRotateGameFlyers(value);
    }

    public void SetRotateGameFlyers(bool value)
    {
        rotateGameFlyersToggle.isOn = value;
        Messaging.OnRotateGameFlyers(value);
    }

    public void ChangeShowGameTitlesOnSelectionScreen(bool value)
    {
        settings.showGameTitlesOnSelectionScreen = value;
        WriteJsonFile();
        Messaging.OnShowGameTitlesOnSelectionScreen(value);
    }

    public void SetShowGameTitlesOnSelectionScreen(bool value)
    {
        showGameTitlesOnSelectionScreenToggle.isOn = value;
        Messaging.OnShowGameTitlesOnSelectionScreen(value);
    }

    public void ChangeUseCoverFlowMode(bool value)
    {
        settings.useCoverFlowMode = value;
        WriteJsonFile();
        Messaging.OnUseCoverFlowMode(value);
    }

    public void SetUseCoverFlowMode(bool value)
    {
        coverFlowModeToggle.isOn = value;
        Messaging.OnUseCoverFlowMode(value);
    }

    public void ChangeResolution(int value)
    {
        settings.resolutionIndex = value;
        WriteJsonFile();
        ApplyResolution(value);
    }

    public void SetResolution(int value)
    {
        resolutionDropdown.value = value;
        ApplyResolution(value);
    }

    public void ApplyResolution(int index)
    {
        Resolution resolution = resolutions[index];

        int width = resolution.width;
        int height = resolution.height;
        FullScreenMode fullScreenMode = Screen.fullScreenMode;
        int preferredRefreshRate = Screen.currentResolution.refreshRate;//Screen.resolutions[Screen.resolutions.Length - 1].refreshRate;
        int startingFrameRate = Application.targetFrameRate;
        Messaging.OnResolutionChanged(width, height, fullScreenMode, preferredRefreshRate);
        Screen.SetResolution(width, height, fullScreenMode, preferredRefreshRate);
    }

    #region Screen Resolutions
    [DllImport("user32.dll")]
    public static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);
    const int ENUM_CURRENT_SETTINGS = -1;

    const int ENUM_REGISTRY_SETTINGS = -2;

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        private const int CCHDEVICENAME = 0x20;
        private const int CCHFORMNAME = 0x20;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public ScreenOrientation dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;

    }

    public void GetResolutions()
    {
        resolutions.Add(new Resolution() { width = 496, height = 384 });

        DEVMODE vDevMode = new DEVMODE();
        int i = 0;
        while (EnumDisplaySettings(null, i, ref vDevMode))
        {
            Resolution resolution = new Resolution() { width = vDevMode.dmPelsWidth, height = vDevMode.dmPelsHeight };
            if (resolutions.Find(x => x.width == resolution.width && x.height == resolution.height) == null)
            {
                resolutions.Add(resolution);
            }
            //Debug.Log($"Width:{vDevMode.dmPelsWidth} Height:{vDevMode.dmPelsHeight} Color:{1 << vDevMode.dmBitsPerPel} Frequency:{vDevMode.dmDisplayFrequency}");
            i++;
        }

        resolutions = resolutions.OrderBy(x => x.width).ThenBy(x => x.height).ToList();

        SetupResolutions();
    }

    private void SetupResolutions()
    {
        resolutionDropdown.options.Clear();
        for (int h = 0; h < resolutions.Count; h++)
        {
            //Debug.Log(resolutions[h].name);
            resolutionDropdown.options.Add(new Dropdown.OptionData($"{resolutions[h].width}x{resolutions[h].height}"));
        }

        resolutionDropdown.value = resolutions.Count - 1;
        ApplyResolution(resolutionDropdown.value);
    }


    [System.Serializable]
    public class Resolution
    {
        public int width;
        public int height;
    }
    #endregion

    public void SetArgumentsFromJson(string json)
    {
        settings = JsonUtility.FromJson<Settings>(json);

        SetRotateGameFlyers(settings.rotateGameFlyers);
        SetShowGameTitlesOnSelectionScreen(settings.showGameTitlesOnSelectionScreen);
        SetUseCoverFlowMode(settings.useCoverFlowMode);
        SetResolution(settings.resolutionIndex);
    }

    public string GetJson()
    {
        string json = JsonUtility.ToJson(settings, true); 
        return json;
    }

    public void WriteJsonFile()
    {
#if !UNITY_EDITOR        
        string jsonPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\";
#else
        string jsonPath = @"C:\SupermodelsAssistant\";
#endif

        string filePath = $@"{jsonPath}LauncherSettings.json";

        using (StreamWriter sw = new StreamWriter(filePath))
        {
            sw.Write(GetJson());
        }
    }

    public void ReadJsonFile()
    {
#if !UNITY_EDITOR        
        string jsonPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\";
#else
        string jsonPath = @"C:\SupermodelsAssistant\";
#endif

        string filePath = $@"{jsonPath}LauncherSettings.json";

        FileInfo fi = new FileInfo(filePath);
        if (fi.Exists)
        {
            string json = "";
            using (StreamReader sr = new StreamReader(filePath))
            {
                json = sr.ReadToEnd();
            }

            if (!string.IsNullOrEmpty(json))
            {
                SetArgumentsFromJson(json);
            }
        }
        else
        {
            WriteJsonFile();
        }
    }    
}
