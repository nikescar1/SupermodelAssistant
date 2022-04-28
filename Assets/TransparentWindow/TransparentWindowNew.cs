using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindowNew : MonoBehaviour
{
    [SerializeField]
    private Material m_Material;

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    //private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    static extern int SetLayeredWindowAttributes(int hwnd, int crKey, byte bAlpha, int dwFlags);

    [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
    private static extern int GetActiveWindowNew();

    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;

    /// <summary>
    /// More swicthing window info here:
    /// https://forum.unity.com/threads/how-to-set-focus-activation-to-unity-player-window.225952/
    /// https://forum.unity.com/threads/solved-windows-transparent-window-with-opaque-contents-lwa_colorkey.323057/
    /// https://github.com/Elringus/UnityRawInput
    /// https://forum.unity.com/threads/how-to-create-a-steam-like-launcher-shell.471657/?_ga=2.47897186.435511174.1586806550-1703585804.1547824964
    /// https://stackoverflow.com/questions/71257/suspend-process-in-c-sharp
    /// 
    /// </summary>
    /// 

    uint startingStyle;
    IntPtr hwnd;
    bool isMenuShowing;

    private void OnEnable()
    {
        Messaging.OnShowInGameMenu += Show;
        Messaging.OnHideInGameMenu += Hide;
    }

    private void OnDisable()
    {
        Messaging.OnShowInGameMenu -= Show;
        Messaging.OnHideInGameMenu -= Hide;
    }

    void Start()
    {
#if !UNITY_EDITOR   // You really don't want to enable this in the editor..

        hwnd = GetActiveWindow();
        startingStyle = (uint)GetWindowLong(hwnd, GWL_STYLE);
#endif
    }

    private void Show()
    {
#if !UNITY_EDITOR
        var margins = new MARGINS() { cxLeftWidth = -1 };
        //var hwnd = GetActiveWindow();

        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        isMenuShowing = true;
#endif
    }

    private void Hide()
    {
#if !UNITY_EDITOR
        var margins = new MARGINS() { cxLeftWidth = 0 };
        //var hwnd = GetActiveWindow();

        SetWindowLong(hwnd, GWL_STYLE, startingStyle);

        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        isMenuShowing = false;
#endif
    }

    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        if (isMenuShowing)
        {
            Graphics.Blit(from, to, m_Material);
        }
        else
        {
            Graphics.Blit(from, to);
        }
    }
}

/*
public class BorderlessMode : MonoBehaviour
{
    public Rect ScreenPosition;
    public bool IsFullscreen = false;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    const uint SWP_SHOWWINDOW = 0x0040;
    const int GWL_STYLE = -16;
    const int WS_BORDER = 1;

    void Start()
    {
        if (IsFullscreen)
        {
            ScreenPosition = GetFullscreenResolution();
        }

        SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_BORDER);
        bool result = SetWindowPos(GetForegroundWindow(), 0, (int)ScreenPosition.x, (int)ScreenPosition.y, (int)ScreenPosition.width, (int)ScreenPosition.height, SWP_SHOWWINDOW);
    }
#endif

#if UNITY_EDITOR
    void Update()
    {
        if (IsFullscreen)
        {
            ScreenPosition = GetFullscreenResolution();
        }

    }
#endif

    Rect GetFullscreenResolution()
    {
        Resolution resolution = Screen.currentResolution;
        return new Rect(0f, 0f, (float)resolution.width, (float)resolution.height);
    }
}
*/