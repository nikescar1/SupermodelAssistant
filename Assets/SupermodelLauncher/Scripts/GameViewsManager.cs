using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.UI;

public class GameViewsManager : MonoBehaviour
{
    public List<GameGroupSO> gameGroupSOs = new List<GameGroupSO>();
    public Transform selectGameButtonsParent;
    public GameObject selectGameButtonPrefab;
    public List<GameView> gameViews = new List<GameView>();
    public Transform gameViewsParent;
    public GameObject gameViewPrefab;
    public GameObject main;
    public GameObject blackScreen;
    public GameObject launchingText;
    public GameObject closingText;
    public GameObject badRom;
    public GameObject startPatching;
    public Text startPatchingFileName;
    public GameObject patchingSuccess;
    public Text patchingSuccessFileName;
    public GameObject patchingFailure;
    public Text patchingFailureFileName;
    public InputField missingRomfilesField;
    public Button patchingFailureButton;
    public CoverFlowManager coverFlowManager;

    public GameView currentGameView;

    private bool didGameExit = false;

    private int width;
    private int height;
    private FullScreenMode fullScreenMode;
    private int preferredRefreshRate;

    private int startingFrameRate;

    private SelectedGameVersions selectedGameVersions = new SelectedGameVersions();

    public class SelectedGameVersions
    {
        public List<int> games;
        /*
public int DaytonaUSA2 = 0;
public int DirtDevils = 0;
public int EmergencyCallAmbulance = 0;
public int FightingVipers2 = 0;
public int HarleyDavidson = 0;
public int LAMachineguns = 0;
public int LeMans24 = 0;
public int MagicalTruckAdventure = 0;
public int ScudRace = 0;
public int SegaBassFishing = 0;
public int SegaRally2 = 0;
public int SkiChamp = 0;
public int Spikeout = 0;
public int StarWarsTrilogy = 0;
public int TheLostWorld = 0;
        public int TheOceanHunter = 0;
        public int VirtuaFighter3 = 0;
        public int VirtualOnOT = 0;
        public int VirtuaStriker2 = 0;
        */
    }

    private void OnEnable()
    {
        Messaging.OnGameSelected += OnGameSelected;
        Messaging.OnGameLaunched += OnGameLaunched;
        Messaging.OnGameExited += OnGameExited;
        Messaging.OnBackPressed += OnBackPressed;

        Messaging.OnGameVersionSelected += SetSelectedGameVersion;

        Messaging.OnPatchRomFile += ShowBadRom;
        Messaging.OnPatchRomFileSuccess += ShowPatchingSuccess;
        Messaging.OnPatchRomFileFailure += ShowPatchingFailure;
        patchingFailureButton.onClick.AddListener(HideBadRomConfirm);

        Messaging.OnUseCoverFlowMode += ShowGameSelectionGrid;

        Messaging.OnResolutionChanged += OnResolutionChanged;
    }

    private void OnDisable()
    {
        Messaging.OnGameSelected -= OnGameSelected;
        Messaging.OnGameLaunched -= OnGameLaunched;
        Messaging.OnGameExited -= OnGameExited;
        Messaging.OnBackPressed -= OnBackPressed;

        Messaging.OnGameVersionSelected -= SetSelectedGameVersion;

        Messaging.OnPatchRomFile -= ShowBadRom;
        Messaging.OnPatchRomFileSuccess -= ShowPatchingSuccess;
        Messaging.OnPatchRomFileFailure -= ShowPatchingFailure;
        patchingFailureButton.onClick.RemoveListener(HideBadRomConfirm);

        Messaging.OnUseCoverFlowMode -= ShowGameSelectionGrid;

        Messaging.OnResolutionChanged -= OnResolutionChanged;
    }

    private void Awake()
    {
        selectedGameVersions.games = new List<int>();
        for (int i = 0; i < gameGroupSOs.Count; i++)
        {
            selectedGameVersions.games.Add(0);
        }

        ReadJsonFile();

        for (int i = 0; i < gameGroupSOs.Count; i++)
        {
            SelectGameButton selectGameButton = Instantiate(selectGameButtonPrefab, selectGameButtonsParent).GetComponent<SelectGameButton>();

            GameView gameView = Instantiate(gameViewPrefab, gameViewsParent).GetComponent<GameView>();
            gameView.Setup(gameGroupSOs[i], selectGameButton, i, selectedGameVersions);
            gameViews.Add(gameView);
        }

        coverFlowManager.Setup(gameViews);

        /*
        //Debug.Log(Screen.currentResolution);
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            //Debug.Log(Screen.resolutions[i]);
        }

        //gameViews.AddRange(FindObjectsOfType<GameView>());
        width = Screen.resolutions[Screen.resolutions.Length - 1].width; //Screen.currentResolution.width;
        height = Screen.resolutions[Screen.resolutions.Length - 1].height;//.currentResolution.height;
        fullScreenMode = Screen.fullScreenMode;
        preferredRefreshRate = Screen.resolutions[Screen.resolutions.Length - 1].refreshRate;
        startingFrameRate = Application.targetFrameRate;


        Screen.SetResolution(width, height, fullScreenMode, preferredRefreshRate);
        //Debug.Log(Screen.res.currentResolution);
        */
    }

    private void SetSelectedGameVersion(GameGroupSO gameGroup, GameVersion gameVersion)
    {
        for (int i = 0; i < gameGroupSOs.Count; i++)
        {
            if (gameGroupSOs[i].Equals(gameGroup))
            {
                for (int h = 0; h < gameGroupSOs[i].gameVersions.Count; h++)
                {
                    if (gameGroupSOs[i].gameVersions[h].Equals(gameVersion))
                    {
                        selectedGameVersions.games[i] = h;
                        WriteJsonFile();
                        return;
                    }
                }
            }
        }
    }

    public string GetSelectedGameVersionsJson()
    {
        string json = JsonUtility.ToJson(selectedGameVersions, true); //JsonConvert.SerializeObject(arguments, Formatting.Indented);
        //string json = JsonUtility.ToJson(arguments);
        return json;
    }

    public void SetSelectedGameVersionsJson(string json)
    {
        selectedGameVersions = JsonUtility.FromJson<SelectedGameVersions>(json);
        Messaging.OnGameVersionSelectedWasSet(selectedGameVersions);
    }

    public void WriteJsonFile()
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

            string filePath = $@"{jsonPath}\SelectedGameVersions.json";

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(GetSelectedGameVersionsJson());

                //Debug.Log($"{GetSelectedGameVersionsJson()}");
            }
    }

    public void ReadJsonFile()
    {
#if !UNITY_EDITOR
        string jsonPath = $@"{Directory.GetParent(Application.dataPath).ToString()}\GameConfigurationsJson";
#else
        string jsonPath = $@"C:\SupermodelsAssistant\GameConfigurationsJson";
#endif
        string filePath = $@"{jsonPath}\SelectedGameVersions.json";

        FileInfo fi = new FileInfo(filePath);
        if (fi.Exists)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string json = sr.ReadToEnd();
                if (!string.IsNullOrEmpty(json))
                {
                    //Debug.Log($"{json}");
                    SetSelectedGameVersionsJson(json);
                }
            }
        }
        else
        {
            WriteJsonFile();
        }
    }

    private void Update()
    {
        /*
        if (didGameExit)
        {
            didGameExit = false;
            Screen.SetResolution(width, height, fullScreenMode, preferredRefreshRate);
            HideBlackScreen();
        }
        */
    }

    private void ShowGameSelectionGrid(bool shouldShow)
    {
        selectGameButtonsParent.gameObject.SetActive(!shouldShow);
        gameViewsParent.gameObject.SetActive(!shouldShow);
    }

    private void OnGameSelected(GameView gameView)
    {
        currentGameView = gameView;
        ShowSelectedGameView();
    }


    private void ShowSelectedGameView()
    {
        currentGameView.Show();
    }

    private void HideSelectedGameView()
    {
        currentGameView.Hide();
    }

    private void OnGameLaunched(GameView gameView)
    {
        ShowBlackScreen();
    }

    private void ShowBlackScreen()
    {
        main.SetActive(false);
        blackScreen.SetActive(true);
        launchingText.SetActive(true);
        closingText.SetActive(false);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            //StartCoroutine(ShowClosingTextIE());
            //Application.targetFrameRate = 1;
        }
        else
        {
            //Application.targetFrameRate = startingFrameRate;
        }
    }

    private void ShowBadRom(string fileName)
    {
        startPatchingFileName.text = $"{fileName} ...";
        blackScreen.SetActive(true);
        launchingText.SetActive(false);
        badRom.SetActive(true);
        startPatching.SetActive(true);
    }

    private void ShowPatchingSuccess(string fileName)
    {
        patchingSuccessFileName.text = $"{fileName}!";
        startPatching.SetActive(false);
        patchingSuccess.SetActive(true);
        StartCoroutine(ReTryLaunchGameIE(fileName));
    }

    private void ShowPatchingFailure(string fileName, List<string> fileList)
    {
        patchingFailureFileName.text = fileName;
        startPatching.SetActive(false);
        patchingFailure.SetActive(true);

        string fileListString = "";
        for (int i = 0; i < fileList.Count; i++)
        {
            fileListString += $"{fileList[i]}\n";
        }
        missingRomfilesField.text = missingRomfilesField.textComponent.text = fileListString;
    }

    IEnumerator ReTryLaunchGameIE(string fileName)
    {
        yield return new WaitForSeconds(3f);
        HideBadRom();
        ShowBlackScreen();
        Messaging.OnGameReLaunched();
    }

    private void HideBadRom()
    {
        blackScreen.SetActive(false);
        badRom.SetActive(false);
        startPatching.SetActive(false);
        patchingSuccess.SetActive(false);
        patchingFailure.SetActive(false);
        missingRomfilesField.text = missingRomfilesField.textComponent.text = "";
    }

    private void HideBadRomConfirm()
    {
        Messaging.OnPatchRomFileFailureConfirm();
        HideBadRom();
    }

    IEnumerator ShowClosingTextIE()
    {
        yield return new WaitForSeconds(1f);
        ShowClosingText();
    }

    private void ShowClosingText()
    {
        launchingText.SetActive(false);
        closingText.SetActive(true);
    }

    private void OnResolutionChanged(int width, int height, FullScreenMode fullScreenMode, int preferredRefreshRate)
    {
        this.width = width;
        this.height = height;
        this.fullScreenMode = fullScreenMode;
        this.preferredRefreshRate = preferredRefreshRate;
    }

    private void OnGameExited(bool hasMissingFiles)
    {
        didGameExit = true;

        didGameExit = false;
        Screen.SetResolution(width, height, fullScreenMode, preferredRefreshRate);
        HideBlackScreen();
    }

    private void HideBlackScreen()
    {
        main.SetActive(true);
        blackScreen.SetActive(false);
        launchingText.SetActive(false);
        closingText.SetActive(false);
    }

    private void OnBackPressed()
    {
        HideSelectedGameView();
    }
}