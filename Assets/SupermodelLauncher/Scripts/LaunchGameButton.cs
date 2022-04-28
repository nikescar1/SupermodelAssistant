using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LaunchGameButton : MonoBehaviour
{
    public GameView gameView;

    public void Pressed()
    {
        Messaging.OnGameLaunched(gameView);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
