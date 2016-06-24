using UnityEngine;
using System.Collections;

public class MovingPlataforms : MonoBehaviour
{

    public bool movingUp;
    public Transform up;
    public Transform down;
    public float time;
    public float velocity;
    public float coldown;
    public Vector3 posUp;
    public Vector3 posDown;
    // Use this for initialization
    void Start()
    {
        posUp = up.position;
        posDown = down.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (movingUp)
        {
            transform.position = Vector3.Lerp(transform.position, posUp, velocity * time);
            time += Time.deltaTime;
            if (time >= coldown)
            {
                movingUp = false;
                time = 0;
            }
            return;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, posDown, velocity * time);
            time += Time.deltaTime;
            if (time >= coldown)
            {
                movingUp = true;
                time = 0;

            }
            return;
        }
    }
}
