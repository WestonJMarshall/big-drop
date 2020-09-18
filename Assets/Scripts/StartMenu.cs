using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject music;
    public GameObject startUI;
    public GameObject controlsUI;

    public void Awake()
    {
        DontDestroyOnLoad(music);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Controls()
    {
        startUI.SetActive(false);
        controlsUI.SetActive(true);
    }

    public void BackToStart()
    {
        startUI.SetActive(true);
        controlsUI.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
