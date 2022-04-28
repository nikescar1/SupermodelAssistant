using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Uween;

public class GameView : MonoBehaviour
{
    public LaunchGameButton launchGameButton;
    public SelectGameButton selectGameButton;
    public GameArguments gameArguments;

    private Vector3 startingLocation;
    private RectTransform rect;
    public Text gameSynopsis;
    public Image flyerFrontImage;
    public Image flyerRearImage;
    public Image bgImage;
    public List<Image> images;

    private bool isStart = true;

    public GameVersion currentGameVersion;
    public GameGroupSO gameGroup;

    public Dropdown gameVersionDropdown;

    private GameViewsManager.SelectedGameVersions selectedGameVersions;

    public int thisIndex;

    private void OnEnable()
    {
        Messaging.OnGameVersionSelectedWasSet += SetGameVersionFromSave;
        if (selectedGameVersions != null)
        {
            gameVersionDropdown.value = selectedGameVersions.games[thisIndex];
        }
    }

    private void OnDisable()
    {
        Messaging.OnGameVersionSelectedWasSet -= SetGameVersionFromSave;
        if (!isStart)
        {
            gameArguments.WriteJsonFile(this);
            gameArguments.WriteBatchFile(this);
        }
        else
        {
            isStart = false;
        }
    }

    private void Start()
    {
        //launchGameButton.game = game;
        //gameObject.SetActive(false);
        //game.ReadJsonFile();
        //transform.localScale = Vector3.zero;
        //rect = GetComponent<RectTransform>();
        //startingLocation = rect.anchoredPosition3D;
    }

    public void Setup(GameGroupSO gameGroupSO, SelectGameButton selectGameButton, int thisIndex, GameViewsManager.SelectedGameVersions selectedGameVersions)
    {
        this.thisIndex = thisIndex;
        this.selectedGameVersions = selectedGameVersions;
        gameGroup = gameGroupSO;
        currentGameVersion = gameGroup.gameVersions[selectedGameVersions.games[thisIndex]];
        gameObject.name = currentGameVersion.name + " View";
        gameSynopsis.text = gameGroup.gameSynopsis;
        flyerFrontImage.sprite = gameGroup.flyerFront;
        flyerRearImage.sprite = gameGroup.flyerRear;
        bgImage.sprite = gameGroup.bg;

        this.selectGameButton = selectGameButton;
        selectGameButton.Setup(this);

        launchGameButton.gameView = this;
        gameObject.SetActive(false);
        gameArguments.ReadJsonFile(this);
        gameArguments.gameView = this;
        gameVersionDropdown.ClearOptions();

        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < gameGroupSO.gameVersions.Count; i++)
        {
            options.Add(new Dropdown.OptionData(gameGroupSO.gameVersions[i].name));
        }

        gameVersionDropdown.AddOptions(options);
    }

    public void SetGameVersionFromSave(GameViewsManager.SelectedGameVersions selectedGameVersions)
    {
        gameVersionDropdown.value = selectedGameVersions.games[thisIndex];
    }

    public void SelectGameVersion(int index)
    {
        SetGameVersion(gameGroup.gameVersions[index]);
        /*
        for (int i = 0; i < gameGroup.gameVersions.Count; i++)
        {
            if (gameGroup.gameVersions[i].name.Equals(gameName))
            {
                return;
            }
        }
        */
    }

    public void SetGameVersion(int value)
    {
        SelectGameVersion(value);
    }

    public void SetGameVersion(GameVersion gameVersion)
    {
        currentGameVersion = gameVersion;
        gameObject.name = currentGameVersion.name + " View";

        Messaging.OnGameVersionSelected(gameGroup, gameVersion);

        //gameArguments.ReadJsonFile(this);

        //gameArguments.arguments.gameVersionIndex = gameVersionDropdown.value;

        //gameArguments.WriteJsonFile(this);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        //rect.anchoredPosition3D = selectGameButton.transform.position;
        //TweenSXYZ.Add(gameObject, 1f, Vector3.one);
        //TweenXYZ.Add(gameObject, 1f, startingLocation);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        //TweenXYZ.Add(gameObject, 1f, selectGameButton.GetComponent<RectTransform>().anchoredPosition3D);
        //TweenSXYZ.Add(gameObject, 1f, Vector3.zero).Then(() => gameObject.SetActive(false));
    }
}