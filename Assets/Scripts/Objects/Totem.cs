using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Totem : NetworkBehaviour {

    [SyncVar]
    public Quaternion rot;
    [SyncVar]
    public bool blueTotem;
    [SyncVar]
    public bool finalTotem = false;

    public Material blueMaterial;
    public Material redMaterial;

	// Use this for initialization
	void Start () {
        transform.rotation = rot;
        if(finalTotem)
        {
            if (blueTotem)
                GetComponent<MeshRenderer>().material = blueMaterial;
            else
                GetComponent<MeshRenderer>().material = redMaterial;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
