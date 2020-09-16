using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverMenu : MonoBehaviour
{
    public GameObject gameOverMenuUI;
    public TextMeshProUGUI scoreText;

    GameManager GameManager;
    private float restartDelay = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        GameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameVariables.GameState == 2)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        scoreText.text = "Score: " + GameManager.Instance.gameVariables.CompletedBlockCount.ToString();
        gameOverMenuUI.SetActive(true);
        GameManager.gameVariables.GameState = 1;
    }

    public void RestartDelay()
    {
        gameOverMenuUI.SetActive(false);
        Invoke("Restart", restartDelay);
    }

    private void Restart()
    {
        Debug.Log("restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.SpawnCube();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
