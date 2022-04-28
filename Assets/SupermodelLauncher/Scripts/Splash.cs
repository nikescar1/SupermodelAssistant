using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(LoadIE());
    }

    IEnumerator LoadIE()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Launcher");
    }
}
