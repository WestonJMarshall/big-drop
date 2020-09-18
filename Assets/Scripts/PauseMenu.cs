using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsGamePaused = false;
    public GameObject pauseMenuUI;
    public float restartDelay = 2.0f;

    GameManager GameManager;

    void Start()
    {
        GameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        //stops overlay of both game over and pause UIs
        if (GameManager.gameVariables.GameState != 2)
        {
            //escape key and 'P' are set to pause the game
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (IsGamePaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        IsGamePaused = false;

        GameManager.Instance.gameVariables.GameState = 1;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        IsGamePaused = true;

        GameManager.Instance.gameVariables.GameState = 0;
    }

    public void RestartDelay()
    {
        Restart();
        Debug.Log("delay");
    }

    private void Restart()
    {
        Debug.Log("restart");
        GameManager.Instance.ResetVariables();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1.0f;
        IsGamePaused = false;
    }

    public void Exit()
    {
        Debug.Log("exit");
        Application.Quit();
    }
}
