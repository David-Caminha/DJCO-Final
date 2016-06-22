using UnityEngine;
using System.Collections;

public class MovingPlataforms : MonoBehaviour {

    public bool movingUp;
    public Transform up;
    public Transform down;
    public float time;
    public float velocity;
    public float coldown;
    public Vector3 pos;
    // Use this for initialization
    void Start () {
        pos = up.position;
	}
	
	// Update is called once per frame
	void Update () {
	    if(movingUp)
        {
            transform.position = Vector3.Lerp(transform.position, pos, velocity * time);
            time += Time.deltaTime;
            if (time >= coldown)
            {
                movingUp = false;
                time = 0;
                pos = down.position;
            }
            return;
        }
            else
        {
            transform.position = Vector3.Lerp(transform.position, pos, velocity * time);
            time += Time.deltaTime;
            if (time >= coldown)
            {
                movingUp = true;
                time = 0;
                pos = up.position;

            }
            return;
        }
	}
}
