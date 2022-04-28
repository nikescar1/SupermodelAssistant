using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Button optionButton;
    public Button optionBackButton;
    public Image optionsOpenButtonImage;
    public Sprite optionsButtonNormalSprite;
    public Sprite optionsButtonHoverSprite;

    public GameObject optionsMenu;
    /*
    private void OnEnable()
    {
        optionButton.onClick.AddListener(ShowMenu);
        optionBackButton.onClick.AddListener(HideMenu);
    }

    private void OnDisable()
    {
        optionButton.onClick.RemoveListener(ShowMenu);
        optionBackButton.onClick.RemoveListener(HideMenu);
    }
    */
    public void Start()
    {
        optionButton.onClick.AddListener(ShowMenu);
        optionBackButton.onClick.AddListener(HideMenu);
    }

    public void OnEnterOptionsButton()
    {
        optionsOpenButtonImage.sprite = optionsButtonHoverSprite;
    }

    public void OnExitOptionsButton()
    {
        optionsOpenButtonImage.sprite = optionsButtonNormalSprite;
    }

    public void ShowMenu()
    {
        optionsMenu.SetActive(true);
        Messaging.OnSettingsOpen();
    }

    public void HideMenu()
    {
        optionsMenu.SetActive(false);
        Messaging.OnSettingsClosed();
    }

    public void OpenSupermodelFolder()
    {
        Application.OpenURL($"file://{SupermodelLauncher.supermodelPath}");
    }

    public void OpenRomFolder()
    {
        Application.OpenURL($"file://{SupermodelLauncher.supermodelPath}"+"/Roms");
    }
}
