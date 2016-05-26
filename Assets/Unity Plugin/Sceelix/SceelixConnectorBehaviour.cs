using UnityEngine;

public class SceelixConnectorBehaviour : MonoBehaviour {
    
    private readonly SceelixConnector _connector = new SceelixConnector();


    // Use this for initialization
	void Start () {

        _connector.Start();
	}

    // Update is called once per frame
    void Update()
    {
        _connector.Update();
    }


    public void OnApplicationQuit()
    {
        _connector.Stop();
    }
    
}
