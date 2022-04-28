using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
//using Newtonsoft.Json;
using System.Security.AccessControl;

public class GameArguments : MonoBehaviour
{
    public Arguments arguments = new Arguments();

    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;
    public Dropdown cpuClockDropdown;
    public Dropdown graphicsEngineDropdown;
    public Dropdown multithreadingDropdown;
    public Toggle widescreenToggle;
    public Toggle vsyncToggle;
    public Toggle stretchToggle;
    public Toggle showFpsToggle;
    public Toggle throttleToggle;
    public Toggle quadRenderingToggle;
    public Toggle multiTexturingToggle;

    public Slider soundVolumeSlider;
    public Slider musicVolumeSlider;
    public Toggle enableSoundToggle;
    public Toggle enableMusicToggle;
    public Toggle flipStereoToggle;
    public Slider frontRearBalanceSlider;

    public Toggle enableForceFeedbackToggle;
    public Toggle loadSaveStateOnLaunchToggle;
    public Dropdown loadSaveStateOnLaunchDropdown;

    public Text soundVolumeValue;
    public Text musicVolumeValue;
    private List<Resolution> resolutions = new List<Resolution>();

    private bool isSettingArgumentsFromJson = false;

    public GameView gameView;

    private void OnEnable()
    {
        Messaging.OnGameLaunched += WriteBatchFile;
        Messaging.OnGameLaunched += WriteJsonFile;
    }

    private void OnDisable()
    {
        Messaging.OnGameLaunched -= WriteBatchFile;
        Messaging.OnGameLaunched -= WriteJsonFile;
    }

    public void SetArgumentsFromJson(string json)
    {
        arguments = JsonUtility.FromJson<Arguments>(json);//JsonConvert.DeserializeObject<Arguments>(json);

        isSettingArgumentsFromJson = true;

        fullscreenToggle.isOn = arguments.fullscreen;
        resolutionDropdown.value = resolutions.IndexOf(resolutions.Find(x => x.width == arguments.resolution.width && x.height == arguments.resolution.height));
        cpuClockDropdown.value = Mathf.Clamp((Mathf.RoundToInt(arguments.cpuClock) / 10) - 2, 0, 8);
        graphicsEngineDropdown.value = (int)arguments.graphicsEngine;
        SetGraphicsEngineUI();
        multithreadingDropdown.value = (int)arguments.multithreading;
        widescreenToggle.isOn = arguments.widescreen;
        vsyncToggle.isOn = arguments.vsync;
        stretchToggle.isOn = arguments.stretch;
        showFpsToggle.isOn = arguments.showFramerate;
        throttleToggle.isOn = arguments.throttle;
        quadRenderingToggle.isOn = arguments.quadRendering;
        multiTexturingToggle.isOn = arguments.multiTexture;

        soundVolumeSlider.value = arguments.soundVolume;
        musicVolumeSlider.value = arguments.musicVolume;
        frontRearBalanceSlider.value = arguments.frontRearBalance;
        SetEnableSoundUI();
        enableSoundToggle.isOn = arguments.enableSound;
        SetEnableMusicUI();
        enableMusicToggle.isOn = arguments.enableMusic;
        flipStereoToggle.isOn = arguments.flipStereo;

        enableForceFeedbackToggle.isOn = arguments.forceFeedback;
        SetLoadSaveStateOnLaunchSlotUI();
        loadSaveStateOnLaunchToggle.isOn = arguments.loadSaveStateOnLaunch;
        loadSaveStateOnLaunchDropdown.value = arguments.loadSaveStateSlot;

        isSettingArgumentsFromJson = false;

        //gameVerisonDropdown.value = resolutions.IndexOf(resolutions.Find(x => x.width == arguments.resolution.width && x.height == arguments.resolution.height));
    }

    private void SetLoadSaveStateOnLaunchSlotUI()
    {
        if (arguments.loadSaveStateOnLaunch)
        {
            loadSaveStateOnLaunchDropdown.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            loadSaveStateOnLaunchDropdown.transform.parent.gameObject.SetActive(false);
        }

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    private void SetGraphicsEngineUI()
    {
        switch (arguments.graphicsEngine)
        {
            case GraphicsEngine.new3d:
                multiTexturingToggle.gameObject.SetActive(false);
                break;
            case GraphicsEngine.legacy3d:
                multiTexturingToggle.gameObject.SetActive(true);
                break;
            default:
                break;
        }

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    private void SetEnableSoundUI()
    {
        if (arguments.enableSound)
        {
            soundVolumeSlider.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            soundVolumeSlider.transform.parent.gameObject.SetActive(false);
        }

        if (!arguments.enableSound && !arguments.enableMusic)
        {
            flipStereoToggle.gameObject.SetActive(false);
            frontRearBalanceSlider.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            flipStereoToggle.gameObject.SetActive(true);
            frontRearBalanceSlider.transform.parent.gameObject.SetActive(true);
        }

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    private void SetEnableMusicUI()
    {
        if (arguments.enableMusic)
        {
            musicVolumeSlider.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            musicVolumeSlider.transform.parent.gameObject.SetActive(false);
        }

        if (!arguments.enableSound && !arguments.enableMusic)
        {
            flipStereoToggle.gameObject.SetActive(false);
            frontRearBalanceSlider.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            flipStereoToggle.gameObject.SetActive(true);
            frontRearBalanceSlider.transform.parent.gameObject.SetActive(true);
        }

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    private void Awake()
    {
        GetResolutions();
    }

    public string GetArgumentString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (arguments.fullscreen)
        {
            stringBuilder.Append(" -fullscreen");
        }

        //stringBuilder.Append(" -print-gl-info");

        stringBuilder.Append(" -res=");
        stringBuilder.Append(arguments.resolution.width);
        stringBuilder.Append(",");
        stringBuilder.Append(arguments.resolution.height);
        
        stringBuilder.Append(" -ppc-frequency=");
        stringBuilder.Append(arguments.cpuClock);

        stringBuilder.Append(" -");
        stringBuilder.Append(arguments.graphicsEngine.ToString());

        stringBuilder.Append(" -");
        stringBuilder.Append(arguments.multithreading.ToString().Replace('_', '-'));

        if (arguments.vsync)
        {
            stringBuilder.Append(" -vsync");
        }
        else
        {
            stringBuilder.Append(" -no-vsync");
        }

        if (arguments.widescreen)
        {
            stringBuilder.Append(" -wide-screen");
        }

        if (arguments.stretch)
        {
            stringBuilder.Append(" -stretch");
        }

        if (arguments.showFramerate)
        {
            stringBuilder.Append(" -show-fps");
        }

        if (!arguments.throttle)
        {
            stringBuilder.Append(" -no-throttle");
        }

        if (arguments.quadRendering)
        {
            stringBuilder.Append(" -quad-rendering");
        }

        if (arguments.multiTexture)
        {
            stringBuilder.Append(" -multi-texture");
        }
        else
        {
            stringBuilder.Append(" -no-multi-texture");
        }

        stringBuilder.Append(" -balance=");
        stringBuilder.Append(arguments.frontRearBalance);

        if (!arguments.enableSound)
        {
            stringBuilder.Append(" -no-sound");
        }

        if (!arguments.enableMusic)
        {
            stringBuilder.Append(" -no-dsb");
        }

        if (!arguments.flipStereo)
        {
            stringBuilder.Append(" -flip-stereo");
        }

        if (!arguments.forceFeedback)
        {
            stringBuilder.Append(" -force-feedback");
        }

        if (arguments.loadSaveStateOnLaunch)
        {
            stringBuilder.Append(" -load-state=");
            stringBuilder.Append(SupermodelLauncher.supermodelSavesPath);
            stringBuilder.Append(GetComponent<GameView>().currentGameVersion.fileName.Replace("zip", $"st{arguments.loadSaveStateSlot}")); 
        }

        string args = stringBuilder.ToString();

        return args;
    }

    public void SetFullscreen(bool value)
    {
        arguments.fullscreen = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetVsync(bool value)
    {
        arguments.vsync = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetWidescreen(bool value)
    {
        arguments.widescreen = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetStretch(bool value)
    {
        arguments.stretch = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetShowFramerate(bool value)
    {
        arguments.showFramerate = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetThrottle(bool value)
    {
        arguments.throttle = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetQuadRendering(bool value)
    {
        arguments.quadRendering = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetMultiTexture(bool value)
    {
        arguments.multiTexture = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetFlipStereo(bool value)
    {
        arguments.flipStereo = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetEnableSound(bool value)
    {
        arguments.enableSound = value;

        SetEnableSoundUI();
    }

    public void SetEnableMusic(bool value)
    {
        arguments.enableMusic = value;

        SetEnableMusicUI();
    }

    public void SetForceFeedback(bool value)
    {
        arguments.forceFeedback = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetSoundVolume(float value)
    {
        arguments.soundVolume = Mathf.RoundToInt(value);
        soundVolumeValue.text = arguments.soundVolume.ToString() + "%";

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }
       
    public void SetMusicVolume(float value)
    {
        arguments.musicVolume = Mathf.RoundToInt(value);
        musicVolumeValue.text = arguments.musicVolume.ToString() + "%";

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetFrontRearBalance(float value)
    {
        arguments.frontRearBalance = Mathf.RoundToInt(value);

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetCpuClock(int value)
    {
        switch (value)
        {
            case 0:
                arguments.cpuClock = 20;
                break;
            case 1:
                arguments.cpuClock = 30;
                break;
            case 2:
                arguments.cpuClock = 40;
                break;
            case 3:
                arguments.cpuClock = 50;
                break;
            case 4:
                arguments.cpuClock = 60;
                break;
            case 5:
                arguments.cpuClock = 70;
                break;
            case 6:
                arguments.cpuClock = 80;
                break;
            case 7:
                arguments.cpuClock = 90;
                break;
            case 8:
                arguments.cpuClock = 100;
                break;
        }

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetLoadStateOnLaunch(bool value)
    {
        arguments.loadSaveStateOnLaunch = value;

        SetLoadSaveStateOnLaunchSlotUI();
    }

    public void SetLoadStateOnLaunchSlot(int value)
    {
        arguments.loadSaveStateSlot = value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetGraphicsEngine(int value)
    {
        arguments.graphicsEngine = (GraphicsEngine)value;

        SetGraphicsEngineUI();
    }

    public void SetMultithreading(int value)
    {
        Debug.Log("Multithread: " + value);
        arguments.multithreading = (Multithreading)value;

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public void SetResolution(int value)
    {
        arguments.resolution = resolutions[value];

        WriteJsonFile(gameView);
        WriteBatchFile(gameView);
    }

    public string GetArgumentsJson()
    {
        string json = JsonUtility.ToJson(arguments, true); //JsonConvert.SerializeObject(arguments, Formatting.Indented);
        //string json = JsonUtility.ToJson(arguments);
        return json;
    }

    public void WriteJsonFile(GameView gameView)
    {
        if (!isSettingArgumentsFromJson)
        {
#if !UNITY_EDITOR
            string jsonPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\GameConfigurationsJson";
#else
            string jsonPath = $@"C:\SupermodelsAssistant\GameConfigurationsJson";
#endif
            FileInfo fi = new FileInfo(jsonPath);
            if (!fi.Directory.Exists)
            {
                DirectorySecurity securityRules = new DirectorySecurity();
                securityRules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));

                Directory.CreateDirectory(fi.DirectoryName, securityRules);
            }

            string filePath = $@"{jsonPath}\{gameView.currentGameVersion.fileName.Replace("zip", "json")}";

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(GetArgumentsJson());

                //Debug.Log($"{gameView.gameGroup.gameName}: \n{GetArgumentsJson()}");
            }
        }
    }

    public void ReadJsonFile(GameView gameView)
    {
#if !UNITY_EDITOR
        string jsonPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\GameConfigurationsJson";
#else
        string jsonPath = $@"C:\SupermodelsAssistant\GameConfigurationsJson";
#endif
        string filePath = $@"{jsonPath}\{gameView.currentGameVersion.fileName.Replace("zip", "json")}";

        FileInfo fi = new FileInfo(filePath);
        if (fi.Exists)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string json = sr.ReadToEnd();
                if (!string.IsNullOrEmpty(json))
                {
                    //Debug.Log($"{gameView.gameGroup.gameName}");
                    SetArgumentsFromJson(json);
                }
            }
        }
        else
        {
            WriteJsonFile(gameView);
            WriteBatchFile(gameView);
        }
    }

    public void WriteBatchFile(GameView gameView)
    {
        if (!isSettingArgumentsFromJson)
        {
#if !UNITY_EDITOR
            string batchPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\GameConfigurationsBatch";
#else
            string batchPath = $@"C:\SupermodelsAssistant\GameConfigurationsBatch";
#endif
            FileInfo fi = new FileInfo(batchPath);
            if (!fi.Directory.Exists)
            {
                DirectorySecurity securityRules = new DirectorySecurity();
                securityRules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));

                Directory.CreateDirectory(fi.DirectoryName, securityRules);
            }

            string filePath = $@"{batchPath}\{gameView.currentGameVersion.fileName.Replace("zip", "bat")}";
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine($@"cd {batchPath}\");
                sw.WriteLine($"supermodel {gameView.currentGameVersion.fileName} {GetArgumentString()}");
            }
        }
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
    }
    #endregion

    [System.Serializable]
    public class Arguments
    {
        //public int gameVersionIndex = 0;
        public Resolution resolution = new Resolution() { width = 496, height = 384 };
        public int cpuClock = 100;
        public GraphicsEngine graphicsEngine = GraphicsEngine.new3d;
        public Multithreading multithreading = Multithreading.gpu_multi_threaded;
        public bool fullscreen = true;
        public bool vsync = true;
        public bool widescreen = true;
        public bool stretch = false;
        public bool showFramerate = true;
        public bool throttle = false;
        public bool quadRendering = true;
        public bool multiTexture = false;

        public int soundVolume = 100;
        public int musicVolume = 100;
        public bool enableSound = true;
        public bool enableMusic = true;
        public bool flipStereo = false;
        public int frontRearBalance = 0;

        public bool forceFeedback = false;
        public bool loadSaveStateOnLaunch = false;
        public int loadSaveStateSlot = 0;
    }

    [System.Serializable]
    public class Resolution
    {
        public int width;
        public int height;
    }

    public enum GraphicsEngine
    {
        new3d,
        legacy3d
    }

    public enum Multithreading
    {
        no_threads,
        no_gpu_thread,
        gpu_multi_threaded
    }
}