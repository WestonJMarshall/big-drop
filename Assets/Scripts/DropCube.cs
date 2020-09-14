using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class DropCube : MonoBehaviour
{
    public event EventHandler OnBlockRested;
    public event EventHandler OnFailure;

    public GameObject mouseEffect;

    #region Private Variables
    [SerializeField]
    private bool functioning = true;

    [Header("Block Falling Properties")]
    [SerializeField]
    private float swaySpeedMax = 2.5f;
    [SerializeField]
    private float swaySpeedMin = 0.7f;

    [SerializeField]
    private float swayDurationMax = 4.5f;
    [SerializeField]
    private float swayDurationMin = 1.9f;

    [SerializeField]
    private float fallSpeedMultiplierMax = 1.002f;
    [SerializeField]
    private float fallSpeedMultiplierMin = 0.9985f;

    [SerializeField]
    private Vector3 cubeSizeMax = new Vector3(17.5f,10.5f, 17.5f);
    [SerializeField]
    private Vector3 cubeSizeMin = new Vector3(12.5f, 5.5f, 12.5f);

    private Rigidbody rb;
    private float previousFrameY = 0.0f;
    private float restTimer = 0.0f;
    private float swapTimer = 0.0f;
    private bool rlSwap = false;

    private float swaySpeed = 2.5f;
    private float swayDuration = 2.0f;
    private float fallSpeedMultiplier = 1.015f;
    private Vector3 cubeSize = new Vector3(15, 10, 15);
    #endregion

    private float FrameHeightChange => Mathf.Abs(previousFrameY - transform.position.y);
    public bool Falling { get; set; }

    private void Awake()
    {
        if (!functioning)
            return;

        Falling = true;
        rb = GetComponentInChildren<Rigidbody>();

        swaySpeed = UnityEngine.Random.Range(swaySpeedMin, swaySpeedMax);
        swayDuration = UnityEngine.Random.Range(swayDurationMin, swayDurationMax);
        fallSpeedMultiplier = UnityEngine.Random.Range(fallSpeedMultiplierMin, fallSpeedMultiplierMax);

        transform.localScale = new Vector3(
            UnityEngine.Random.Range(cubeSizeMin.x, cubeSizeMax.x),
            UnityEngine.Random.Range(cubeSizeMin.y, cubeSizeMax.y),
            UnityEngine.Random.Range(cubeSizeMin.z, cubeSizeMax.z));

        swapTimer = swayDuration / 2.0f;
    }

    private void Start()
    {
        if (!functioning)
            return;

        previousFrameY = transform.position.y;

        GetComponentInChildren<MeshRenderer>().material = GameManager.Instance.sharedCubeMaterial;
        StartCoroutine(nameof(LerpColorChange), new Tuple<float,int>(HsvData()[0] + 0.04f, 0));
    }

    /// <summary>
    /// Basically if the block is static for a little while, it tells the manager it is rested, manager will move on to the next block
    /// </summary>
    /// <returns>If the block has been static for long enough</returns>
    public bool IsBlockRested()
    {
        restTimer = Convert.ToInt32(FrameHeightChange < 0.01f) * (restTimer + Time.deltaTime);
        previousFrameY = transform.position.y;
        return restTimer > 0.9f ? true : false;
    }

    public void BlockRested()
    {
        OnBlockRested?.Invoke(this, null);
        StopAllCoroutines();
    }

    /// <summary>
    /// Gives a block its swaying motion based on a set sway speed
    /// </summary>
    public void BlockFallingMovement()
    {
        swapTimer += Time.deltaTime;
        if (swapTimer > swayDuration)
        {
            rlSwap = !rlSwap;
            swapTimer = 0.0f;
        }

        float forceX = rlSwap ? swaySpeed : -swaySpeed;
        rb.AddForceAtPosition(new Vector3(forceX, 0, 0), transform.position - new Vector3(0, 0.2f, 0));

        rb.velocity = new Vector3(
            rb.velocity.x,
            rb.velocity.y / fallSpeedMultiplier,
            rb.velocity.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Falling = false;

        if(collision.gameObject.GetComponentInChildren<DropCube>() == null) //This is game over
            OnFailure?.Invoke(this, null);
    }

    private float[] HsvData() 
    { 
        Color.RGBToHSV(GameManager.Instance.sharedCubeMaterial.color, out float h, out float s, out float v); return new float[3] { h, s, v }; 
    }

    public IEnumerator LerpColorChange(Tuple<float, int> changeData)
    {
        yield return new WaitForEndOfFrame();

        GameManager.Instance.sharedCubeMaterial.color = Color.HSVToRGB(HsvData()[0] + ((changeData.Item1 - HsvData()[0]) * 0.011f), HsvData()[1], HsvData()[2]);

        if(changeData.Item1 - HsvData()[0] < 0.01f || changeData.Item2 > 50)
        {
            GameManager.Instance.sharedCubeMaterial.color = Color.HSVToRGB(changeData.Item1, HsvData()[1], HsvData()[2]);
        }
        else
        {
            StartCoroutine(nameof(LerpColorChange), new Tuple<float, int>(HsvData()[0] + 0.04f, changeData.Item2 + 1));
        }
    }
}
