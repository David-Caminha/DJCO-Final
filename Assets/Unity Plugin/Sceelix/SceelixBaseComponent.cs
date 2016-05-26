using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

/// <summary>
/// For now, it is just used to determine if a gameobject was generated using Construct or not.
/// </summary>
public class SceelixBaseComponent : MonoBehaviour {

    public List<String> Assets = new List<string>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void CreateAsset(Object asset, string path)
    {
        AssetDatabase.CreateAsset(asset, path);
        Assets.Add(path);
    }

    public void DeleteAssets()
    {
        foreach (var asset in Assets)
            AssetDatabase.DeleteAsset(asset);
    }
}
