using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    private AudioSource music;
    private float t = 0.0f;
    private bool transitioning = false;

    float currentPitch = 1.0f;
    float targetPitch = 2.0f;

    float duration = 2.0f;

    float timer = 0.0f;

    void Start()
    {
        music = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (transitioning)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            music.pitch = Mathf.SmoothStep(currentPitch, targetPitch, t);
            if (timer > duration)
            {
                transitioning = false;
                timer = 0.0f;
                currentPitch = music.pitch;
            }
        }
    }

    public void ChangePitch(float targetPitch)
    {
        this.targetPitch = targetPitch;
        transitioning = true;
    }
}
