using UnityEngine;
using UnityEngine.UI;
public class UIShaker : MonoBehaviour
{
    public float shakeAmount = 0.7f;
    public RectTransform canvas;
    float shakeTime = 0.0f;
    Vector3 initialPosition;

    public void VibrateForTime(float time)
    {
        shakeTime = time;
        //canvas.renderMode = RenderMode.ScreenSpaceCamera;
        //canvas.renderMode = RenderMode.WorldSpace;
    }

    void Start()
    {
        initialPosition = canvas.position;
        VibrateForTime(10);
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            canvas.position = Random.insideUnitSphere * shakeAmount + initialPosition;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            shakeTime = 0.0f;
            canvas.position = initialPosition;
            //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }
}