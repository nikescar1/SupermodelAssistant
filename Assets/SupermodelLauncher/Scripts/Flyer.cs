using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyer : MonoBehaviour
{
    public float speed;
    public RectTransform flyer;
    public RectTransform front;
    public RectTransform rear;
    public float rot;

    public Transform cam;

    private bool shouldRotate;

    private void OnEnable()
    {
        Messaging.OnRotateGameFlyers += Rotate;
        //shouldRotate = Messaging.OnGetLauncherSettings().rotateGameFlyers;
    }

    private void OnDisable()
    {
    }
    
    void Start()
    {
        cam = FindObjectOfType<Camera>().transform;
    }

    private void Rotate(bool shouldRotate)
    {
        this.shouldRotate = shouldRotate;
    }

    void Update()
    {
        if (shouldRotate)
        {
            flyer.Rotate(Vector3.up * speed * Time.deltaTime);
            rot = flyer.localRotation.eulerAngles.y;
            /*
            if (flyer.localRotation.eulerAngles.y > 270 && flyer.localRotation.eulerAngles.y < 360 || flyer.localRotation.eulerAngles.y > 0 && flyer.localRotation.eulerAngles.y < 90)
            {
                rear.SetAsFirstSibling();
            }
            else if (flyer.localRotation.eulerAngles.y > 90 && flyer.localRotation.eulerAngles.y < 270)
            {
                front.SetAsFirstSibling();
            }
            */
            if (Vector3.Dot(flyer.forward, cam.position - flyer.position) > 0)
            {
                front.SetAsFirstSibling();

            }
            else
            {
                rear.SetAsFirstSibling();
            }
        }
        else if (flyer.eulerAngles.y != 0)
        {
            flyer.eulerAngles = Vector3.zero;
        }
    }
}
