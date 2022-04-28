using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Messaging : MonoBehaviour
{
    public static Action<GameView> OnGameSelected = delegate { };
    public static Action<GameGroupSO, GameVersion> OnGameVersionSelected = delegate { };
    public static Action<GameViewsManager.SelectedGameVersions> OnGameVersionSelectedWasSet = delegate { };
    public static Action<GameView> OnGameSelectedCoverFlow = delegate { };
    public static Action OnBackToSelection = delegate { };
    public static Action OnTitleScreenTurnedOff = delegate { };


    //Options
    public static Func<LauncherSettings.Settings> OnGetLauncherSettings = delegate { return null; };
    public static Action<bool> OnRotateGameFlyers = delegate { };
    public static Action<bool> OnShowGameTitlesOnSelectionScreen = delegate { };
    public static Action<bool> OnUseCoverFlowMode = delegate { };
    public static Action<int, int, FullScreenMode, int> OnResolutionChanged = delegate { };
    public static Action OnSettingsOpen = delegate { };
    public static Action OnSettingsClosed = delegate { };

    //Launching
    public static Action<GameView> OnGameLaunched = delegate { };
    public static Action<bool> OnGameExited = delegate { };
    public static Action OnBackPressed = delegate { };
    public static Action OnKillEmulator = delegate { };

    //In-Game Menu
    public static Action OnShowInGameMenu = delegate { };
    public static Action OnHideInGameMenu = delegate { };

    //Rom Patching
    public static Action<string> OnAddFileToRomPatching = delegate { };
    public static Action<string> OnShowBadRomFile = delegate { };
    public static Action<string> OnPatchRomFile = delegate { };
    public static Action<string> OnPatchRomFileSuccess = delegate { };
    public static Action<string, List<string>> OnPatchRomFileFailure = delegate { };
    public static Action OnPatchRomFileFailureConfirm = delegate { };
    public static Action OnGameReLaunched = delegate { };
}