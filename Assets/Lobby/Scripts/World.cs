using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 10) * Time.deltaTime);
	}
}
