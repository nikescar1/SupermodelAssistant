using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleVideoPlayerSizeWithScreenWidth : MonoBehaviour
{
    public RectTransform canvas;
    public RectTransform videoPlayer;

    private void Start()
    {
        ChangeRatio();
    }

    void Update()
    {
        if (canvas.hasChanged)
        {
            ChangeRatio();
        }
    }

    private void ChangeRatio()
    {
        videoPlayer.sizeDelta = new Vector2((canvas.sizeDelta.x / canvas.sizeDelta.y) * videoPlayer.sizeDelta.y, videoPlayer.sizeDelta.y);
    }
}
