using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct GameStateVariables
{
    public float GameTime { get; set; }
    public int GameState { get; set; } // 0 = main menu, 1 = game, 2 = game over
    public int CompletedBlockCount { get; set; }
    public DropCube CurrentCube { get; set; }
    public Vector3 DropLocation { get; set; }
}

public class GameManager : MonoBehaviour
{
    #region Singlton
    public static GameManager Instance { get; private set; }
    #endregion

    public GameStateVariables gameVariables;

    private bool newLerpY = false;
    private bool newLerpAngle = false;
    private float top = 0.0f;

    public GameObject cubePrefab;
    public Material sharedCubeMaterial;

    private GameObject scoreCanvas;
    public GameObject music;

    private void Awake()
    {
        //Setup GameManager Singleton Instance
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += StartScene;
    }

    private void StartScene(Scene scene, LoadSceneMode mode)
    {
        ResetVariables();
        music = GameObject.FindGameObjectWithTag("Music");
        scoreCanvas = GameObject.FindGameObjectWithTag("ScoreCanvas");
        scoreCanvas.GetComponentInChildren<TMP_Text>().text = $"Score: <color=#{ColorUtility.ToHtmlStringRGB(sharedCubeMaterial.color)}>{gameVariables.CompletedBlockCount}</color>";
        SpawnCube();
    }

    void Update()
    {
        if (gameVariables.CurrentCube == null || gameVariables.GameState != 1)
            return;

        //Only check cube functions on current cube
        if (gameVariables.CurrentCube.IsBlockRested())
            gameVariables.CurrentCube.BlockRested();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitData, 1000))
        {
            if (hitData.transform.gameObject.GetComponentInChildren<DropCube>() == gameVariables.CurrentCube) //Clicked on the current falling cube
            {
                gameVariables.CurrentCube.mouseEffect.SetActive(true);
                if (Input.GetMouseButtonDown(0))
                {
                    gameVariables.CurrentCube.Falling = false;
                    gameVariables.CurrentCube.GetComponentInChildren<Rigidbody>().drag = 0.2f;
                    gameVariables.CurrentCube.ParachuteFlyAway();
                }
            }
            else
            {
                gameVariables.CurrentCube.mouseEffect.SetActive(false);
            }
        }
        else
        {
            gameVariables.CurrentCube.mouseEffect.SetActive(false);
        }

        //Only run special cube movement on current cube
        if (gameVariables.CurrentCube.Falling)
            gameVariables.CurrentCube.BlockFallingMovement();

        if (!newLerpY)
        {
            Vector3 viewPoint = new Vector3(gameVariables.CurrentCube.transform.position.x,
            gameVariables.CurrentCube.transform.position.y - 15.0f,
            gameVariables.CurrentCube.transform.position.z);
        }
        if(!newLerpAngle)
        {
            Vector3 viewPoint = new Vector3(gameVariables.CurrentCube.transform.position.x,
            gameVariables.CurrentCube.transform.position.y - 15.0f,
            gameVariables.CurrentCube.transform.position.z);

            Camera.main.transform.rotation = Quaternion.LookRotation(viewPoint - Camera.main.transform.position, Camera.main.transform.up);
        }
    }

    /// <summary>
    /// Spawns a new drop cube at the current DropLocation
    /// </summary>
    public void SpawnCube()
    {
        if (cubePrefab != null)
            gameVariables.CurrentCube = Instantiate(cubePrefab).GetComponent<DropCube>();
            gameVariables.CurrentCube.mouseEffect.SetActive(false);
            gameVariables.CurrentCube.transform.position = gameVariables.DropLocation;
            gameVariables.CurrentCube.OnBlockRested += TopBlockRested;
            gameVariables.CurrentCube.OnFailure += GameOver;
    }

    /// <summary>
    /// Called when the block that was falling is now not falling and we can move on to the next
    /// </summary>
    private void TopBlockRested(object sender, System.EventArgs e)
    {
        gameVariables.CompletedBlockCount += 1;
        scoreCanvas.GetComponentInChildren<TMP_Text>().text = $"Score: <color=#{ColorUtility.ToHtmlStringRGB(sharedCubeMaterial.color)}>{gameVariables.CompletedBlockCount}</color>";
        Debug.Log(gameVariables.CompletedBlockCount);
        music.GetComponent<Music>().ChangePitch((gameVariables.CompletedBlockCount / 40f) + 1.0f);

        gameVariables.CurrentCube.mouseEffect.SetActive(false);

        Vector3 yDistIncrement = new Vector3(0, gameVariables.CurrentCube.transform.localScale.y, 0);
        gameVariables.DropLocation += yDistIncrement;
        StartCoroutine(nameof(LerpCameraPositionY), Camera.main.transform.position.y + yDistIncrement.y);
        StartCoroutine(nameof(LerpCameraAngle));
        newLerpY = true;
        newLerpAngle = true;
        SpawnCube();
    }

    /// <summary>
    /// Called on failure
    /// </summary>
    private void GameOver(object sender, System.EventArgs e)
    {
        Debug.Log("Game Over");
        music.GetComponent<Music>().ChangePitch(1.0f);
        gameVariables.GameState = 2;

        newLerpY = true;
        newLerpAngle = true;
        Camera.main.transform.rotation = Quaternion.LookRotation(new Vector3(0, 4.3f, 100.0f) - Camera.main.transform.position, Camera.main.transform.up);
    }

    public void ResetVariables()
    {
        StopAllCoroutines();

        //Setup Game Variables
        gameVariables = new GameStateVariables
        {
            GameTime = 0.0f,
            GameState = 1,
            CompletedBlockCount = 0,
            CurrentCube = null,
            DropLocation = new Vector3(0, 62, 100),
        };

        newLerpY = false;
        newLerpAngle = false;
        top = Camera.main.transform.position.y;
    }

    /// <summary>
    /// Will lerp the camera to a specified y value
    /// </summary>
    /// <param name="pos">the y position camera will lerp to</param>
    public IEnumerator LerpCameraPositionY(float pos)
    {
        yield return new WaitForEndOfFrame();

        Vector3 lerpCameraPosition = new Vector3(Camera.main.transform.position.x,
            Camera.main.transform.position.y - ((Camera.main.transform.position.y - pos) * 0.015f),
            Camera.main.transform.position.z);

        Camera.main.transform.position = lerpCameraPosition;

        if (Mathf.Abs(Camera.main.transform.position.y - pos) < 0.1f)
        {
            Camera.main.transform.position = new Vector3(
            Camera.main.transform.position.x,
            pos,
            Camera.main.transform.position.z);

            top = Camera.main.transform.position.y;

            newLerpY = false;
        }
        else
        {
            StartCoroutine(nameof(LerpCameraPositionY), pos);
        }
    }

    /// <summary>
    /// Will lerp the camera to the top block
    /// </summary>
    /// <param name="pos">the y position camera will lerp to</param>
    public IEnumerator LerpCameraAngle()
    {
        yield return new WaitForEndOfFrame();

        Vector3 lerpBlockPosition = new Vector3(gameVariables.CurrentCube.transform.position.x,
            gameVariables.CurrentCube.transform.position.y - 15.0f,
            gameVariables.CurrentCube.transform.position.z);

        Camera.main.transform.rotation = Quaternion.LookRotation((((lerpBlockPosition - Camera.main.transform.position).normalized * 0.0225f) + (Camera.main.transform.forward)) / 2.0f, Camera.main.transform.up);

        if (Vector3.Distance((gameVariables.CurrentCube.transform.position - Camera.main.transform.position).normalized, (Camera.main.transform.forward)) < 0.065f)
        {
            newLerpAngle = false;
        }
        else
        {
            StartCoroutine(nameof(LerpCameraAngle));
        }
    }
}
