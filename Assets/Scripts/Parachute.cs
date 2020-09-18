using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parachute : MonoBehaviour
{
    public bool flyAway;
    public float timer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (flyAway)
        {
            transform.parent = null;
            timer += Time.deltaTime;
            transform.Translate(0f, 0f, 0.2f);
            if (timer > 3.0f) Destroy(gameObject);
        }
    }
}
