using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightBuzz.Archiver;
using System.IO;

public class RomPatching : MonoBehaviour
{
    public List<string> filesToPatch = new List<string>();
    public Dictionary<string, RomFileList> romFileLists = new Dictionary<string, RomFileList>();

    public class RomFileList
    {
        public string romName;
        public List<string> files = new List<string>();

        public string GetFileByName(string fileName)
        {
            foreach (var file in files)
            {
                if (file.Equals(fileName))
                {
                    return fileName;
                }
            }

            return "";
        }
    }

    private void OnEnable()
    {
        Messaging.OnAddFileToRomPatching += AddFileToRomPatching;
        Messaging.OnPatchRomFile += PatchRomFile;
    }

    private void OnDisable()
    {
        Messaging.OnAddFileToRomPatching += AddFileToRomPatching;
        Messaging.OnPatchRomFile += PatchRomFile;
    }

    void Start()
    {
        SetupRomFileLists();
    }

    private void SetupRomFileLists()
    {
#if !UNITY_EDITOR
        string folderPath = Directory.GetParent(Application.dataPath).ToString() + @"\RomFileLists\";
#else
        string folderPath = @"C:\SupermodelsAssistant\RomFileLists\";
#endif
        string[] romFileListPaths = Directory.GetFiles(folderPath);
        for (int i = 0; i < romFileListPaths.Length; i++)
        {
            string fileName = romFileListPaths[i].Replace(".txt", "").Replace(folderPath, "");

            RomFileList romFileList = new RomFileList();
            romFileList.romName = fileName;

            string path = romFileListPaths[i];
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                romFileList.files.Add(reader.ReadLine());
            }
            reader.Close();

            romFileLists.Add(fileName, romFileList);
        }
    }

    private void AddFileToRomPatching(string fileName)
    {
        filesToPatch.Add(fileName);
    }

    private void PatchRomFile(string fileName)
    {
        StartCoroutine(PatchRomFileIE(fileName));
    }

    IEnumerator PatchRomFileIE(string fileName)
    {        
        bool wasSuccessful = true;

#if !UNITY_EDITOR
        string romPath = Directory.GetParent(Application.dataPath).ToString() + @"\Supermodel\Roms\";
#else
        string romPath = @"C:\SupermodelsAssistant\Supermodel\Roms\";
#endif

#if !UNITY_EDITOR
        string tempPathDonor = Directory.GetParent(Application.dataPath).ToString() + @"\Supermodel\TempDonor\";
#else
        string tempPathDonor = @"C:\SupermodelsAssistant\TempDonor\";
#endif
        if (!Directory.Exists(tempPathDonor))
        {
            Directory.CreateDirectory(tempPathDonor);
        }

#if !UNITY_EDITOR
        string tempPathRecipient = Directory.GetParent(Application.dataPath).ToString() + @"\Supermodel\TempRecipient\";
#else
        string tempPathRecipient = @"C:\SupermodelsAssistant\TempRecipient\";
#endif
        if (!Directory.Exists(tempPathRecipient))
        {
            Directory.CreateDirectory(tempPathRecipient);
        }

        Archiver.Decompress($"{romPath}{fileName}", tempPathRecipient);
        
        for (int i = 0; i < filesToPatch.Count; i++)
        {
            RomFileList romFileList = FindMissingFileParent(filesToPatch[i]);
            if (romFileList == null)
            {
                wasSuccessful = false;
                break;
            }

            Archiver.Decompress($"{romPath}{romFileList.romName}.zip", tempPathDonor);

            yield return null;

            if (File.Exists($"{tempPathDonor}{filesToPatch[i]}"))
            {
                File.Move($"{tempPathDonor}{filesToPatch[i]}", $"{tempPathRecipient}{filesToPatch[i]}");
            }
            else
            {
                wasSuccessful = false;
                break;
            }

            yield return null;

            DirectoryInfo dirInfoDonor = new DirectoryInfo(tempPathDonor);
            foreach (FileInfo file in dirInfoDonor.EnumerateFiles())
            {
                file.Delete();
            }
        }

        yield return new WaitForSeconds (2f);

        if (wasSuccessful)
        {
            Archiver.Compress(tempPathRecipient, $"{romPath}{fileName}");

            yield return null;

            Messaging.OnPatchRomFileSuccess(fileName);
        }
        else
        {
            Messaging.OnPatchRomFileFailure(fileName, filesToPatch);
        }

        DirectoryInfo dirInfoRecipient = new DirectoryInfo(tempPathRecipient);
        dirInfoRecipient.Delete(true);
        DirectoryInfo dirInfo = new DirectoryInfo(tempPathDonor);
        dirInfo.Delete(true);

        filesToPatch.Clear();
    }

    private RomFileList FindMissingFileParent(string fileName)
    {
        foreach (var rom in romFileLists)
        {
            foreach (var childFile in rom.Value.files)
            {
                if (childFile.Equals(fileName))
                {
                    return rom.Value;
                }
            }
        }

        return null;
    }
}
