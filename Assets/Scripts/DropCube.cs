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

    [Header("Block Falling Properties")]
    [SerializeField]
    private float swaySpeed = 0.5f;

    [SerializeField]
    private float swayDuration = 2.0f;

    [SerializeField]
    private float fallSpeedMultiplier = 1.015f;

    private Rigidbody rb;
    private float restTimer = 0.0f;
    private float swapTimer = 0.0f;
    private bool rlSwap = false;

    public bool Falling { get; set; }

    private void Awake()
    {
        Falling = true;
        rb = GetComponentInChildren<Rigidbody>();

        swaySpeed = UnityEngine.Random.Range(0.7f, 2.5f);
        swayDuration = UnityEngine.Random.Range(1.9f, 4.5f);
        fallSpeedMultiplier = UnityEngine.Random.Range(0.9985f, 1.002f);

        swapTimer = swayDuration / 2.0f;

        //GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB((Time.time / 400.0f) % 1, 1, 1);
    }

    /// <summary>
    /// Basically if the block is static for a little while, it tells the manager it is rested, manager will move on to the next block
    /// </summary>
    /// <returns>If the block has been static for long enough</returns>
    public bool IsBlockRested()
    {
        restTimer = Convert.ToInt32(Mathf.Abs(rb.velocity.y) < 0.26f) * (restTimer + Time.deltaTime);
        return restTimer > 0.9f ? true : false;
    }

    public void BlockRested()
    {
        OnBlockRested?.Invoke(this, null);
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
}
