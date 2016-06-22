using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {

    float time;
    public Transform initialPosition;
    public Transform finalPosition;

	// Use this for initialization
	void Start () {
        time = 0;
        initialPosition = transform;
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 10) * Time.deltaTime);
        //time += Time.deltaTime;
        //transform.rotation = Quaternion.Lerp(initialPosition.rotation, finalPosition.rotation, time * 0.001f);
	}
}
