using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class BackgroundInputCapture : MonoBehaviour
{
    
    [DllImport("user32")]
    protected static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);
    [DllImport("user32")]
    protected static extern int UnhookWindowsHookEx(IntPtr hhook);
    [DllImport("user32")]
    protected static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);
    
    protected enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        int scanCode;
        public int flags;
        int time;
        int dwExtraInfo;
    }

    static protected IntPtr m_hhook = IntPtr.Zero;
    protected HookType m_hookType = HookType.WH_KEYBOARD_LL;
    protected delegate int HookProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

    private static bool isShown;
    
    void OnDisable()
    {
        Debug.Log("Uninstall hook");
        Uninstall();
    }

    void OnEnable()
    {
        Install(LowLevelKeyboardProc);
        Debug.Log("install hook");
    }

    protected bool Install(HookProc cbFunc)
    {
        if (m_hhook == IntPtr.Zero)
        {
            m_hhook = SetWindowsHookEx(m_hookType, cbFunc, IntPtr.Zero, 0);  // <- This works, having thread-ID doesn't
            return false;
        }
        return true;
    }
    protected void Uninstall()
    {
        if (m_hhook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(m_hhook);
            m_hhook = IntPtr.Zero;
        }
    }

    public static int LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
    {
        if (nCode >= 0)
        {
            if (lParam.vkCode.ToString() == "46") // delete key
            {
                Debug.Log("Got Delete Key");
                if (isShown)
                {
                    Messaging.OnHideInGameMenu();
                }
                else
                {
                    Messaging.OnShowInGameMenu();
                }
                isShown = !isShown;
            }

            Debug.Log("Got: " + lParam.vkCode.ToString());
        }

        return 0;
    }

    private void Update()
    {
        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            if (isShown)
            {
                Messaging.OnHideInGameMenu();
                isShown = !isShown;
            }
        }
    }
}
