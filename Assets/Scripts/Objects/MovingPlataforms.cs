using UnityEngine;
using System.Collections;

public class MovingPlataforms : MonoBehaviour {

    public bool movingUp;
    public Transform up;
    public Transform down;
    public float time;
    public float velocity;
    public float coldown;
    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    if(movingUp)
        {
            transform.position = Vector3.Lerp(transform.position, up.position, velocity * time);
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
            transform.position = Vector3.Lerp(transform.position, down.position, velocity * time);
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
