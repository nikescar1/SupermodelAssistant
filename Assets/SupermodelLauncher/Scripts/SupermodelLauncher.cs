using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class SupermodelLauncher : MonoBehaviour
{
    private static SupermodelLauncher instance;
    public static SupermodelLauncher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<SupermodelLauncher>();
            }
            return instance;
        }
    }

    public static string supermodelPath = @"C:\SupermodelsAssistant\Supermodel\";
    public static string supermodelFile = @"Supermodel.exe";
    public static string supermodelRomsPath = supermodelPath + @"Roms\";
    public static string supermodelSavesPath = supermodelPath + @"Saves\";

    public static string antimicroPath = @"C:\SupermodelsAssistant\antimicro\";

    private bool didGameExit = false;
    private Process antimicroProcess; 
    private Process supermodelProcess;

    private Thread antimicroThread;
    private Thread supermodelThread;

    const int KEY_ESCAPE = 0x1B;
    const int KEY_DOWN = (0x0100);
    const int KEY_UP = (0x0101);

    private static GameView currentGameView;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern int BringWindowToTop(IntPtr hwnd);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
    const int SWP_NOMOVE = (0x0002);
    const int SWP_NOSIZE = (0x0001);
    //Process unityProcess;
    IntPtr hwnd;
    IntPtr supermodelHwnd;
    private enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

    Keyboard keyboard;

    //[DllImport("user32.dll")]
    //static extern bool PostThreadMessage(uint threadId, uint msg, UIntPtr wParam, IntPtr lParam);
    //static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

    private void Awake()
    {
        instance = this;

        keyboard = new Keyboard();

#if !UNITY_EDITOR
        Debug.Log("Directory.GetParent: " + Directory.GetParent(Application.dataPath).ToString());
        string[] dirs = Directory.GetDirectories(Directory.GetParent(Application.dataPath).ToString(), "Supermodel", SearchOption.TopDirectoryOnly);
        string supermodelDirectory = "";
        foreach (string dir in dirs)
        {
            Debug.Log(dir);
            supermodelDirectory = dir;
        }
        supermodelPath = supermodelDirectory + @"\";
        supermodelRomsPath = supermodelPath + @"Roms\";
        supermodelSavesPath = supermodelPath + @"Saves\";
        antimicroPath = Directory.GetParent(Application.dataPath).ToString() + @"\antimicro";
        Debug.Log("Supermodel Roms path: " + supermodelRomsPath);
        Debug.Log("antimicro: " + antimicroPath);
        //unityProcess = Process.GetCurrentProcess();
        hwnd = GetActiveWindow();
#endif
        //unityProcess = Process.GetCurrentProcess();
        //Debug.Log(unityProcess.ProcessName);
    }

    private void OnEnable()
    {
        Messaging.OnGameLaunched += OnGameLaunched;
        Messaging.OnKillEmulator += KillProcesses;
        Messaging.OnShowInGameMenu += ShowInGameMenu;
        Messaging.OnHideInGameMenu += HideInGameMenu;
        Messaging.OnGameReLaunched += OnGameReLaunched;
    }

    private void OnDisable()
    {
        Messaging.OnGameLaunched -= OnGameLaunched;
        Messaging.OnKillEmulator -= KillProcesses;
        Messaging.OnShowInGameMenu -= ShowInGameMenu;
        Messaging.OnHideInGameMenu -= HideInGameMenu;
        Messaging.OnGameReLaunched -= OnGameReLaunched;
    }

    public void BringMainWindowToFront(IntPtr processHandle)
    {
#if !UNITY_EDITOR
        //SetWindowPos(processHandle, -1, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        
        // check if the process is running
        if (processHandle != null)
        {
            Debug.Log("processHandle != null  " + processHandle.ToString());
            // check if the window is hidden / minimized
            //if (processHandle == IntPtr.Zero)
            {
                // the window is hidden so try to restore it before setting focus.
                ShowWindow(processHandle, ShowWindowEnum.Minimize);
                ShowWindow(processHandle, ShowWindowEnum.Restore);
                ShowWindow(processHandle, ShowWindowEnum.Show);
            }    
            // set user the focus to the window
            SetForegroundWindow(processHandle);
        }
#endif
    }


    public void BringMainWindowToFront(IntPtr processHandleTarget, IntPtr processHandleOverlay)
    {
#if !UNITY_EDITOR
        SetWindowPos(processHandleTarget, processHandleOverlay, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
#endif
    }

    private void ShowInGameMenu()
    {
        /*
        Process[] processes = Process.GetProcessesByName("Supermodel's Assistant");
        for (int i = 0; i < processes.Length; i++)
        {
            BringMainWindowToFront(processes[i].Handle);
        }
        */

        //Process.GetCurrentProcess().Refresh();
        BringMainWindowToFront(hwnd);

        //BringWindowToTop(hwnd);
    }

    private void HideInGameMenu()
    {
        //BringMainWindowToFront(supermodelProcess.MainWindowHandle);
        StartCoroutine(HideInGameMenuIE());
    }

    IEnumerator HideInGameMenuIE()
    {
        yield return null;
        BringMainWindowToFront(supermodelHwnd);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Application.Quit();
        }

        if (didGameExit)
        {
            didGameExit = false;


            if (antimicroProcess != null)
            {
                antimicroProcess.CloseMainWindow();
            }
            if (antimicroProcess != null)
            {
                antimicroProcess.Close();
            }
            if (antimicroProcess != null)
            {
                //antimicroProcess.Kill();
            }

            Messaging.OnGameExited(hasMissingFiles);

            if (hasMissingFiles)
            {
                Messaging.OnPatchRomFile(currentGameView.currentGameVersion.fileName);
                hasMissingFiles = false;
            }
        }
    }

    private void OnGameLaunched(GameView gameView)
    {
        currentGameView = gameView;
        //ExecuteAnitMicro();
        //ExecuteSupermodel(gameView.currentGameVersion.fileName, gameView.gameArguments.GetArgumentString());
        StartCoroutine(ExecuteThreaded(currentGameView.currentGameVersion.fileName, currentGameView.gameArguments.GetArgumentString()));
        //StartCoroutine(GetSupermodelProcessHwnd());
    }

    private void OnGameReLaunched()
    {
        StartCoroutine(ExecuteThreaded(currentGameView.currentGameVersion.fileName, currentGameView.gameArguments.GetArgumentString()));
    }

    IEnumerator ExecuteThreaded(string gameFile, string arguments)
    {
        yield return null;
        if (File.Exists(antimicroPath + @"\antimicro.exe"))
        {
            antimicroThread = new Thread(delegate () { ExecuteAntiMicro(); });
            antimicroThread.Start();
            yield return null;
        }
        supermodelThread = new Thread(delegate () { ExecuteSupermodel(gameFile, arguments); });

        supermodelThread.Start();
        yield return null;
        StartCoroutine(GetSupermodelProcessHwnd());
    }

    private void ExecuteAntiMicro()
    {
        var processInfo = new ProcessStartInfo(antimicroPath + @"\antimicro.exe", "--no-tray --hidden");

        processInfo.WorkingDirectory = antimicroPath;
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;

        processInfo.RedirectStandardOutput = true;
        processInfo.RedirectStandardError = true;

        antimicroProcess = Process.Start(processInfo);
    }
    
    private void ExecuteSupermodel(string gameFile, string arguments)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(supermodelPath);
        stringBuilder.Append(supermodelFile);
        string fileName = $"\"{stringBuilder.ToString()}\"";
        Debug.Log($"Filename: {fileName}");

        stringBuilder.Clear();
        stringBuilder.Append($"\"{supermodelRomsPath}");
        stringBuilder.Append($"{gameFile}\"");
        stringBuilder.Append(arguments);
        string args = stringBuilder.ToString();
        Debug.Log($"Args: {args}");
        stringBuilder.Clear();

        var processInfo = new ProcessStartInfo(fileName, args);

        processInfo.WorkingDirectory = supermodelPath;
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;

        processInfo.RedirectStandardOutput = true;
        processInfo.RedirectStandardError = true;

        supermodelProcess = Process.Start(processInfo);
        supermodelProcess.EnableRaisingEvents = true;
        supermodelProcess.Exited += OnGameExited;
        supermodelProcess.OutputDataReceived += CaptureOutput;
        supermodelProcess.ErrorDataReceived += CaptureError;
        supermodelProcess.BeginOutputReadLine();
        supermodelProcess.BeginErrorReadLine();
        //supermodelHwnd = supermodelProcess.MainWindowHandle;
        //Debug.Log("Base " + supermodelHwnd);

        //supermodelProcess.WaitForExit();
    }
    
    IEnumerator GetSupermodelProcessHwnd()
    {
        yield return null;// new WaitForSeconds(5f);
        while (supermodelHwnd == IntPtr.Zero)
        {
            //yield return new WaitForSeconds(5f);
            //supermodelProcess.Refresh();
            //supermodelHwnd = supermodelProcess.MainWindowHandle;
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                processes[i].Refresh();
                if (!processes[i].HasExited && processes[i].ProcessName.Contains("Supermodel"))
                {
                    supermodelHwnd = processes[i].Handle;
                }
                //Debug.Log(processes[i].ProcessName);
            }

            //supermodelHwnd = supermodelProcess.MainWindowHandle;
            Debug.Log("IE " + supermodelHwnd);
            yield return null;
            //yield return null;
        }
        Debug.Log("Got supermodelHwnd: " + supermodelHwnd);
    }
    
    bool isSuspended = false;
    private void KillProcesses()
    {
        if (supermodelProcess != null)
        {
            supermodelProcess.Kill();
        }

        if (antimicroProcess != null)
        {
            antimicroProcess.CloseMainWindow();
            antimicroProcess.Close();
            antimicroProcess.Dispose();
        }
    }

    private void OnGameExited(object sender, EventArgs e)
    {
        didGameExit = true;
    }

    static void CaptureOutput(object sender, DataReceivedEventArgs e)
    {
        ShowOutput(e.Data);
    }

    static void CaptureError(object sender, DataReceivedEventArgs e)
    {
        ShowOutput(e.Data);
    }

    static bool hasMissingFiles = false;
    static void ShowOutput(string data)
    {
        if (data != null)
        {
            Debug.Log(data);

            if (data.Contains("Error:") && data.Contains("not found in") && data.Contains("for game") && data.Contains(currentGameView.currentGameVersion.fileName.Replace(".zip", "")))
            {
                string fileName = data.Split('\'')[1];
                Messaging.OnAddFileToRomPatching(fileName);
                hasMissingFiles = true;
            }
        }
    }
}

[Flags]
public enum ThreadAccess : int
{
    TERMINATE = (0x0001),
    SUSPEND_RESUME = (0x0002),
    GET_CONTEXT = (0x0008),
    SET_CONTEXT = (0x0010),
    SET_INFORMATION = (0x0020),
    QUERY_INFORMATION = (0x0040),
    SET_THREAD_TOKEN = (0x0080),
    IMPERSONATE = (0x0100),
    DIRECT_IMPERSONATION = (0x0200)
}

public static class ProcessExtension
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
    [DllImport("kernel32.dll")]
    static extern uint SuspendThread(IntPtr hThread);
    [DllImport("kernel32.dll")]
    static extern int ResumeThread(IntPtr hThread);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool CloseHandle(IntPtr handle);

    public static void Suspend(this Process process)
    {
        foreach (ProcessThread pT in process.Threads)
        {
            IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

            if (pOpenThread == IntPtr.Zero)
            {
                continue;
            }

            SuspendThread(pOpenThread);

            CloseHandle(pOpenThread);
        }
    }

    public static void Resume(this Process process)
    {
        foreach (ProcessThread pT in process.Threads)
        {
            IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

            if (pOpenThread == IntPtr.Zero)
            {
                continue;
            }

            var suspendCount = 0;
            do
            {
                suspendCount = ResumeThread(pOpenThread);
            } while (suspendCount > 0);

            CloseHandle(pOpenThread);
        }
    }
}