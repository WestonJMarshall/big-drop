using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameStateVariables
{
    public float GameTime { get; set; }
    public int CompletedBlockCount { get; set; }
    public DropCube CurrentCube { get; set; }
    public Vector3 DropLocation { get; set; }
}

public class GameManager : MonoBehaviour
{
    #region Singlton
    public static GameManager Instance { get; private set; }
    #endregion

    private GameStateVariables gameVariables;

    public GameObject cubePrefab;

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

        //Setup Game Variables
        gameVariables = new GameStateVariables
        {
            GameTime = 0.0f,
            CompletedBlockCount = 0,
            CurrentCube = null,
            DropLocation = new Vector3(0, 62, 100),
        };
    }

    private void Start()
    {
        SpawnCube();
    }

    void Update()
    {
        if (gameVariables.CurrentCube == null)
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
        {
            gameVariables.CurrentCube.BlockFallingMovement();

        }
    }

    /// <summary>
    /// Spawns a new drop cube at the current DropLocation
    /// </summary>
    private void SpawnCube()
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
        Debug.Log(gameVariables.CompletedBlockCount);

        gameVariables.CurrentCube.mouseEffect.SetActive(false);

        Vector3 yDistIncrement = new Vector3(0, gameVariables.CurrentCube.transform.localScale.y, 0);
        gameVariables.DropLocation += yDistIncrement;
        StartCoroutine(nameof(LerpCameraPositionY), Camera.main.transform.position.y + yDistIncrement.y);

        SpawnCube();
    }

    /// <summary>
    /// Called on failure
    /// </summary>
    private void GameOver(object sender, System.EventArgs e)
    {
        Debug.Log("Game Over");
    }

    public IEnumerator LerpCameraPositionY(float pos)
    {
        yield return new WaitForEndOfFrame();

        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x,
            Camera.main.transform.position.y - ((Camera.main.transform.position.y - pos) * 0.015f),
            Camera.main.transform.position.z);

        if (Mathf.Abs(Camera.main.transform.position.y - pos) < 0.1f)
        {
            Camera.main.transform.position = new Vector3(
            Camera.main.transform.position.x,
            pos,
            Camera.main.transform.position.z);
        }
        else
        {
            StartCoroutine(nameof(LerpCameraPositionY), pos);
        }
    }
}
