using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerNetworkSetup : NetworkBehaviour
{

    public ThirdPersonController thirdPersonController;
    public Camera fpsCamera;
    public AudioListener audioListener;

    public override void OnStartLocalPlayer()
    {
        GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>().localPlayer = thirdPersonController;
        fpsCamera.gameObject.SetActive(true);
        audioListener.enabled = true;
        for (int i = 0; i < 13; i++)
            GetComponent<NetworkAnimator>().SetParameterAutoSend(i, true);

        gameObject.name = "LOCAL Player";
        base.OnStartLocalPlayer();
    }

    public override void PreStartClient()
    {
        for (int i = 0; i < 13; i++)
            GetComponent<NetworkAnimator>().SetParameterAutoSend(i, true);
        base.PreStartClient();
    }
}
