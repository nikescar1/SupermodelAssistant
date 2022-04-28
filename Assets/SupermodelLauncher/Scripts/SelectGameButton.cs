using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectGameButton : MonoBehaviour
{
    public Image icon;
    public Text gameNameLabel;
    public GameView gameView;

    private void OnEnable()
    {
        Messaging.OnShowGameTitlesOnSelectionScreen += ShowTitles;
    }

    private void OnDisable()
    {
        //Messaging.OnShowGameTitlesOnSelectionScreen -= ShowTitles;
    }

    public void Setup(GameView gameView)
    {
        this.gameView = gameView;
        icon.sprite = gameView.gameGroup.gameSelectIcon;
        gameNameLabel.text = gameView.gameGroup.gameName;
        gameObject.name = gameView.name;
    }

    public void Pressed()
    {
        Messaging.OnGameSelected(gameView);
    }

    private void ShowTitles(bool shouldShow)
    {
        gameNameLabel.gameObject.SetActive(shouldShow);
    }
}
