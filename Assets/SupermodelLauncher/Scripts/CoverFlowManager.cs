using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CoverFlowManager : MonoBehaviour
{
    public GameObject coverFlow;
    public GameObject coverFlowUI;
    public Text coverFlowGameName;

    public GameView currentGameView;

    public Color tintColor;

    [SerializeField]
    private UICollectionView m_coverFlow;

    [SerializeField]
    private Color[] m_colourData;

    [SerializeField]
    private int m_numberOfCells = 10;

    [SerializeField]
    private int m_groupSizes = 10;

    public List<GameView> gameViews;

    public int currentIndex = -1;

    private bool wasControlActivated = false;
    private bool isControlHeld = false;
    private bool shouldShowFromSettings = false;
    private bool shouldShowFromTitleScreen = false;
    private bool shouldShowFromOptionsScreen = false;

    private void OnEnable()
    {
        Messaging.OnUseCoverFlowMode += Show;
        Messaging.OnTitleScreenTurnedOff += TitleScreenTurnedOff;
        Messaging.OnGameExited += ShowOnGameExit;
        Messaging.OnSettingsOpen += Hide;
        Messaging.OnSettingsClosed += Show;
        Messaging.OnPatchRomFile += HideOnRomPatching;
        Messaging.OnPatchRomFileFailureConfirm += ShowOnRomPatchingFailure;
        //Messaging.OnGameSelectedCoverFlow += SetGameView;
    }

    private void OnDisable()
    {
        Messaging.OnUseCoverFlowMode -= Show;
        Messaging.OnTitleScreenTurnedOff -= TitleScreenTurnedOff;
        Messaging.OnGameExited -= ShowOnGameExit;
        Messaging.OnSettingsOpen -= Hide;
        Messaging.OnSettingsClosed -= Show;
        Messaging.OnPatchRomFile -= HideOnRomPatching;
        Messaging.OnPatchRomFileFailureConfirm -= ShowOnRomPatchingFailure;
        //Messaging.OnGameSelectedCoverFlow -= SetGameView;
    }

    private void Start()
    {
        StartCoroutine(WaitToSetCurrentGameView());
    }

    IEnumerator WaitToSetCurrentGameView()
    {
        yield return new WaitForSeconds(1f);

        currentIndex = (int)m_coverFlow.m_layout.CurrentIndex;
        SetGameView(gameViews[currentIndex]);
    }

    public void Setup(List<GameView> gameViews)
    {
        this.gameViews = gameViews;

        //Build cells
        if (m_coverFlow != null)
        {
            //Build a bunch of cells - pass in data
            List<QuadCell.QuadCellData> data = new List<QuadCell.QuadCellData>();
            for (int i = 0; i < gameViews.Count; i++)
            {
                /*
                if (m_colourData != null && m_colourData.Length > 0)
                {
                    data.Add(new QuadCell.QuadCellData() { MainColor = m_colourData[(i / m_groupSizes) % m_colourData.Length] });
                }
                else
                {
                    data.Add(new QuadCell.QuadCellData() { MainTexture = gameGroupSOs[i].flyerFront.texture, GameName = gameGroupSOs[i].gameName });
                }
                */

                data.Add(new QuadCell.QuadCellData() { Game = gameViews[i], MainColor = tintColor });
            }

            //Bleugh
            m_coverFlow.Data = new List<object>(data.ToArray());
        }
    }

    private void SetGameView(GameView gameView)
    {
        currentGameView = gameView;
        coverFlowGameName.text = currentGameView.currentGameVersion.name;
    }

    private void Show(bool shouldShow)
    {
        shouldShowFromSettings = shouldShow;
        if (shouldShowFromTitleScreen && shouldShowFromSettings && shouldShowFromOptionsScreen)
        {
            coverFlow.SetActive(shouldShowFromSettings);
            coverFlowUI.SetActive(shouldShowFromSettings);
        }
    }

    private void ShowOnGameExit(bool hasMissingFiles)
    {
        if (shouldShowFromTitleScreen && shouldShowFromSettings && shouldShowFromOptionsScreen && !hasMissingFiles)
        {
            coverFlow.SetActive(shouldShowFromSettings);
            coverFlowUI.SetActive(shouldShowFromSettings);
        }
    }

    private void ShowOnRomPatchingFailure()
    {
        if (shouldShowFromTitleScreen && shouldShowFromSettings && shouldShowFromOptionsScreen)
        {
            coverFlow.SetActive(shouldShowFromSettings);
            coverFlowUI.SetActive(shouldShowFromSettings);
        }
    }

    private void Show()
    {
        if (shouldShowFromTitleScreen && shouldShowFromSettings)
        {
            coverFlow.SetActive(shouldShowFromSettings);
            coverFlowUI.SetActive(shouldShowFromSettings);
        }
        shouldShowFromOptionsScreen = true;
    }

    private void HideOnRomPatching(string fileName)
    {
        coverFlow.SetActive(false);
        coverFlowUI.SetActive(false);
    }

    private void HideOnGameLaunching()
    {
        coverFlow.SetActive(false);
        coverFlowUI.SetActive(false);
    }

    private void Hide()
    {
        coverFlow.SetActive(false);
        coverFlowUI.SetActive(false);
        shouldShowFromOptionsScreen = false;
    }

    private void TitleScreenTurnedOff()
    {
        shouldShowFromTitleScreen = true;
        if (shouldShowFromTitleScreen && shouldShowFromSettings)
        {
            coverFlow.SetActive(shouldShowFromSettings);
            coverFlowUI.SetActive(shouldShowFromSettings);
        }
        shouldShowFromOptionsScreen = true;
    }

    public float timeHeldToStartScrolling = 1f;
    private float timeHeld = 0;
    private float timeLastScrolled;
    public float timeBetweenScrolling = .25f;
    private float leftRightValue = 0;

    private void Update()
    {
        if (shouldShowFromTitleScreen && shouldShowFromSettings && shouldShowFromOptionsScreen)
        {
            if (currentIndex != m_coverFlow.m_layout.CurrentIndex)
            {
                currentIndex = (int)m_coverFlow.m_layout.CurrentIndex;
                SetGameView(gameViews[currentIndex]);
            }

            if (Gamepad.current.leftStick.ReadValue().x > .5f)
            {
                leftRightValue = Gamepad.current.leftStick.ReadValue().x;
                if (!wasControlActivated)
                {
                    wasControlActivated = true;
                    timeLastScrolled = Time.time;
                    m_coverFlow.StepRight();
                }
                else
                {
                    timeHeld += Time.deltaTime;
                }
            }
            else if (Gamepad.current.leftStick.ReadValue().x < -.5f)
            {
                leftRightValue = Gamepad.current.leftStick.ReadValue().x;
                if (!wasControlActivated)
                {
                    wasControlActivated = true;
                    timeLastScrolled = Time.time;
                    m_coverFlow.StepLeft();
                }
                else
                {
                    timeHeld += Time.deltaTime;
                }
            }
            else
            {
                timeHeld = 0;
                leftRightValue = 0;
                wasControlActivated = false;
                isControlHeld = false;
            }

            if (timeHeld > timeHeldToStartScrolling)
            {
                if (leftRightValue > .1f)
                {
                    if (timeBetweenScrolling < Time.time - timeLastScrolled)
                    {
                        m_coverFlow.StepRight();
                        timeLastScrolled = Time.time;
                    }
                }
                else if (leftRightValue < -.1f)
                {
                    if (timeBetweenScrolling < Time.time - timeLastScrolled)
                    {
                        m_coverFlow.StepLeft();
                        timeLastScrolled = Time.time;
                    }
                }
            }

            if (Gamepad.current.aButton.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.xButton.wasPressedThisFrame)
            {
                Messaging.OnGameLaunched(currentGameView);
                HideOnGameLaunching();
            }
        }
    }
}
