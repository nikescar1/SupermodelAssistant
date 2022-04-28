using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "GameGroupSO", menuName = "New Game Group", order = 1)]
public class GameGroupSO : ScriptableObject
{
    public string gameName;
    public Sprite gameSelectIcon;
    [ResizableTextArea]
    public string gameSynopsis;
    public Sprite flyerFront;
    public Sprite flyerRear;
    public Sprite bg;
    public List<Sprite> images;
    public List<GameVersion> gameVersions;
}

[System.Serializable]
public class GameVersion
{
    public string name;
    public string fileName;
}

