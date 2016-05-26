using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SceelixConnectorWindow : EditorWindow {

    private static readonly SceelixConnector Connector = new SceelixConnector();

    static SceelixConnectorWindow()
    {
        EditorApplication.update += Update;
        EditorApplication.playmodeStateChanged += HandleOnPlayModeChanged;
    }


    private static void HandleOnPlayModeChanged()
    {
        //It is imperative to close the socket before playing, otherwise Unity will hang/crash
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Connector.Stop();
        }
    }

    

    [MenuItem("Sceelix/Connect To Designer",priority = 0)]
    public static void ConnectToEditor()
    {
        Connector.Start();
    }



    static void Update()
    {
        Connector.Update();

        //It is imperative to close the socket on compilation, otherwise Unity will hang
        if(EditorApplication.isCompiling)
            Connector.Stop();
    }



    [MenuItem("Sceelix/Disconnect From Designer", priority = 1)]
    public static void DisconnectFromEditor()
    {
        Connector.Stop();
    }



    [MenuItem("Sceelix/Clear Sceelix Generated Objects", priority = 15)]
    public static void ClearGeneratedObjects()
    {
        
        foreach (GameObject obj in FindObjectsOfType(typeof(GameObject)).Cast<GameObject>().Where(x => x.GetComponent<SceelixBaseComponent>()).ToList())
        {
            obj.GetComponent<SceelixBaseComponent>().DeleteAssets();

            DestroyImmediate(obj);
        }
    }
}
