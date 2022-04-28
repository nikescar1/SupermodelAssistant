using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TurnOff());
    }

    IEnumerator TurnOff()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
        Messaging.OnTitleScreenTurnedOff();
    }
}