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

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        IsGamePaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        IsGamePaused = true;
    }

    public void RestartDelay()
    {
        Debug.Log("delay");
        Invoke("Restart", restartDelay);
    }

    private void Restart()
    {
        Debug.Log("restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //GameManager.gameVariables.GameTime = 0.0f;
        //GameManager.gameVariables.GameState = 1;
        //GameManager.gameVariables.CompletedBlockCount = 0;
        //GameManager.gameVariables.CurrentCube = null;
        //GameManager.gameVariables.DropLocation = new Vector3(0, 62, 100);
        GameManager.SpawnCube();
        Time.timeScale = 1.0f;
        IsGamePaused = false;
    }

    public void Exit()
    {
        Debug.Log("exit");
        Application.Quit();
    }
}
