using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSkinningManager : MonoBehaviour
{
    [System.Serializable]
    public class UiSkin
    {
        public Font font;
        public float fontMultiplier;
        public Sprite buttonSprite;

    }

    [System.Serializable]
    public class UiSkinningElements
    {
        public UiSkinningElement selectGameButton;
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
