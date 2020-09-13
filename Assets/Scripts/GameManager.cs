﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private GameStateVariables gameVariables;

    private bool newLerp = false;
    private float top = 0.0f;

    public GameObject cubePrefab;
    public Material sharedCubeMaterial;

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
            GameState = 1,
            CompletedBlockCount = 0,
            CurrentCube = null,
            DropLocation = new Vector3(0, 62, 100),
        };

        top = Camera.main.transform.position.y;
    }

    private void Start()
    {
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

        if (!newLerp)
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
        newLerp = true;

        SpawnCube();
    }

    /// <summary>
    /// Called on failure
    /// </summary>
    private void GameOver(object sender, System.EventArgs e)
    {
        Debug.Log("Game Over");

        gameVariables.GameState = 2;

        newLerp = true;
        Camera.main.transform.rotation = Quaternion.LookRotation(new Vector3(0, 4.3f, 100.0f) - Camera.main.transform.position, Camera.main.transform.up);
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

        Vector3 lerpBlockPosition = new Vector3(gameVariables.CurrentCube.transform.position.x,
            gameVariables.CurrentCube.transform.position.y - 15.0f,
            gameVariables.CurrentCube.transform.position.z);

        Camera.main.transform.rotation = Quaternion.LookRotation((((lerpBlockPosition - Camera.main.transform.position).normalized * 0.0175f) + (Camera.main.transform.forward)) / 2.0f, Camera.main.transform.up);
        Camera.main.transform.position = lerpCameraPosition;

        if (Mathf.Abs(Camera.main.transform.position.y - pos) < 0.1f)
        {
            Camera.main.transform.position = new Vector3(
            Camera.main.transform.position.x,
            pos,
            Camera.main.transform.position.z);

            top = Camera.main.transform.position.y;

            newLerp = false;
        }
        else
        {
            StartCoroutine(nameof(LerpCameraPositionY), pos);
        }
    }
}