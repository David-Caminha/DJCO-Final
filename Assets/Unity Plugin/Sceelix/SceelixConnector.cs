using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Sceelix.GameComponents.Data;
using UnityEditor;
using UnityEngine;

public class SceelixConnector
{
    public static Dictionary<String,Action<GenericData>> ComponentHandlers = new Dictionary<string, Action<GenericData>>();

    public const String DefaultHostName = "127.0.0.1";
    public const int DefaultPort = 3500;

    //private MessageClient _messageClient;
    private MessageServer _messageServer;
    

    /// <summary>
    /// Starts the Sceelix Connector using the default hostname and port
    /// </summary>
    public void Start()
    {
        Start(DefaultHostName, DefaultPort);
    }


    /// <summary>
    /// Starts the Sceelix Connector using the indicated hostname and port
    /// </summary>
    public void Start(String hostName, int port)
    {
        //make sure we close a previously existing connection
        if (_messageServer != null)
            _messageServer.Close();

        _messageServer = new MessageServer(IPAddress.Parse(hostName), port);
        _messageServer.RawMessageReceived += OnRawMessageReceived;
        _messageServer.MessageReceived += OnMessageReceived;
        
        Debug.Log("Started Listening To Sceelix Designer.");
    }

    private void OnRawMessageReceived(MessageServer.ClientConnection clientconnection, string message)
    {
        EditorUtility.DisplayProgressBar("Loading Sceelix Data", "Please wait...", 0);
    }

    private void OnMessageReceived(MessageServer.ClientConnection clientconnection, String subject, object data)
    {
        if (subject == "Add Game Objects")
        {
            GenericData[] genericDataObjects = (GenericData[])data;
            for (int index = 0; index < genericDataObjects.Length; index++)
            {
                EditorUtility.DisplayProgressBar("Loading Sceelix Data", "Please wait...", index / (float)genericDataObjects.Length);

                GenericData genericDataObject = genericDataObjects[index];

                ProcessGameObject(genericDataObject);
            }
        }
        else if (subject == "...")
        {

        }

        EditorUtility.ClearProgressBar();
    }


    private void MessageClientOnClientDisconnected()
    {
        Debug.Log("Disconnected From Sceelix Designer.");
    }


    public void Update()
    {
        if (_messageServer != null)
            _messageServer.Synchronize();
    }


    public void Stop()
    {
        if (_messageServer != null)
        {
            _messageServer.Close();
            _messageServer = null;

            Debug.Log("Stopped Listening To Sceelix Designer.");
        }   
    }


    private void ProcessGameObject(GenericData genericDataObject)
    {
        //create asset folder
        if(!AssetDatabase.IsValidFolder("Assets/SceelixAssets"))
            AssetDatabase.CreateFolder("Assets", "SceelixAssets");

        //first of all, let's see if we are loading a prefab
        var prefabPath = genericDataObject.Get<String>("Prefab");

        GameObject gameObject;

        //if a prefab instruction is passed, load it
        if (!String.IsNullOrEmpty(prefabPath))
        {
            if (!prefabPath.StartsWith("Assets/"))
                prefabPath = "Assets/" + prefabPath;

            //make sure the extension is set
            prefabPath = Path.ChangeExtension(prefabPath, ".prefab");

            gameObject = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, (typeof(GameObject))));

            //try to get the bounds of the object
            var renderer = gameObject.GetComponent<MeshFilter>();
            if (renderer != null && !genericDataObject.Get<bool>("Fit Minimum"))
            {
                var scale = new Vector3(1 / renderer.sharedMesh.bounds.size.x, 1 / renderer.sharedMesh.bounds.size.y, 1 / renderer.sharedMesh.bounds.size.z);
                
                gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale,scale);
            }
        }
        else
        {
            gameObject = new GameObject();
        }
        
        gameObject.name = genericDataObject.Get<String>("Label");

        //genericDataObject.GetVector3("Position");

        gameObject.transform.position += genericDataObject.GetVector3("Position");
        gameObject.transform.rotation *= genericDataObject.GetQuartenion("Rotation");

        if (!genericDataObject.Get<bool>("Fit Minimum"))
            gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, genericDataObject.GetVector3("Scale"));

        
        var renderer2 = gameObject.GetComponent<MeshFilter>();
        if (renderer2 != null)
        {
            if (genericDataObject.Get<bool>("Fit Minimum"))
            {
                var intendedScale = genericDataObject.GetVector3("Scale");

                var actualSize = Vector3.Scale(renderer2.sharedMesh.bounds.size, gameObject.transform.localScale);
                var ratio = Vector3.Scale(intendedScale, (new Vector3(1f / actualSize.x, 1f / actualSize.y, 1f / actualSize.z)));

                var minCoordinate = Math.Min(Math.Min(ratio.x, ratio.y), ratio.z);

                gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, new Vector3(minCoordinate, minCoordinate, minCoordinate));
            }

            gameObject.transform.Translate(Vector3.Scale(-renderer2.sharedMesh.bounds.min, gameObject.transform.localScale));
        }

        //note: the scale cannot be 0, otherwise the mesh will not be shown properly
        gameObject.transform.localScale = new Vector3(GetOneIfZero(gameObject.transform.localScale.x),GetOneIfZero(gameObject.transform.localScale.y),GetOneIfZero(gameObject.transform.localScale.z));


        //now, let's process the components
        SceelixBaseComponent sceelixBaseComponent = gameObject.AddComponent<SceelixBaseComponent>();

        GenericData[] genericGameComponents = genericDataObject.Get<GenericData[]>("GameComponents");
        foreach (GenericData genericGameComponent in genericGameComponents)
        {
            if (genericGameComponent.Name == "MeshFilter")
            {
                //if a meshfilter already exists, don't overwrite it
                if (gameObject.GetComponent<MeshFilter>() != null)
                    continue;

                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                
                if(meshFilter == null)
                    continue;

                GenericData genericMesh = genericGameComponent.Get<GenericData>("Mesh");

                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.vertices =
                    genericMesh.Get<float[][]>("Vertices").Select(x => new Vector3(x[0], x[1], x[2])).ToArray();
                meshFilter.sharedMesh.normals =
                    genericMesh.Get<float[][]>("Normals").Select(x => new Vector3(x[0], x[1], x[2])).ToArray();
                meshFilter.sharedMesh.colors =
                    genericMesh.Get<float[][]>("Colors").Select(x => new Color(x[0], x[1], x[2], x[3])).ToArray();
                meshFilter.sharedMesh.uv =
                    genericMesh.Get<float[][]>("Uvs").Select(x => new Vector2(x[0], x[1])).ToArray();
                
                int[][] list = genericMesh.Get<int[][]>("SubmeshTriangles");
                meshFilter.sharedMesh.subMeshCount = list.Length;

                for (int index = 0; index < list.Length; index++)
                {
                    int[] subList = list[index];
                    meshFilter.sharedMesh.SetTriangles(subList, index);
                }

                sceelixBaseComponent.CreateAsset(meshFilter.sharedMesh,
                    "Assets/SceelixAssets/" + Guid.NewGuid() + ".mesh");
            }
            else if (genericGameComponent.Name == "MeshRenderer")
            {
                //if a MeshRenderer already exists, don't overwrite it
                if (gameObject.GetComponent<MeshRenderer>() != null)
                    continue;

                MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
                
                GenericData[] genericMaterials = genericGameComponent.Get<GenericData[]>("Materials");
                
                Material[] sharedMaterials = new Material[genericMaterials.Length];
                for (int index = 0; index < genericMaterials.Length; index++)
                {
                    GenericData genericMaterial = genericMaterials[index];

                    //first step, create the material
                    var newMaterial = new Material(Shader.Find(genericMaterial.Get<String>("Shader")));

                    var mainTextureName = genericMaterial.Get<String>("MainTextureName");

                    var baseName = "Assets/SceelixAssets/" + mainTextureName;

                    if (!File.Exists(baseName + ".mat"))
                    {
                        sceelixBaseComponent.CreateAsset(newMaterial, baseName + ".mat");

                        newMaterial.mainTexture = genericMaterial.GetTexture("MainTexture");
                        //save the texture to 
                        File.WriteAllBytes(baseName + ".png", genericMaterial.GetTexture("MainTexture").EncodeToPNG());

                        AssetDatabase.Refresh();
                    }

                    newMaterial = (Material) (AssetDatabase.LoadAssetAtPath(baseName + ".mat", typeof (Material)));
                    newMaterial.mainTexture =
                        (Texture2D) (AssetDatabase.LoadAssetAtPath(baseName + ".png", typeof (Texture2D)));

                    sharedMaterials[index] = newMaterial;
                }

                renderer.sharedMaterials = sharedMaterials;
            }
            else if (genericGameComponent.Name == "Terrain")
            {
                //if a Terrain already exists, don't overwrite it
                if (gameObject.GetComponent<Terrain>() != null)
                    continue;

                //load the data from the generic definition
                var terrainAssetName = "Assets/SceelixAssets/" + genericGameComponent.Get<String>("Asset Name") + ".asset";
                var heights = genericGameComponent.Get<float[,]>("Heights");
                var resolution = genericGameComponent.Get<int>("Resolution");
                //var numColumns = genericGameComponent.Get<int>("NumColumns");
                var sizes = genericGameComponent.Get<float[]>("Size");
                var splatmapSentData = genericGameComponent.Get<float[,][]>("Splatmap");
                
                //initialize the terrain data instance and set height data
                //unfortunately unity terrain maps have to be square and the sizes must be powers of 2
                TerrainData terrainData = new TerrainData();

                terrainData.heightmapResolution = resolution;
                terrainData.SetHeights(0, 0, heights);
                terrainData.size = new Vector3(sizes[0], sizes[1], sizes[2]);
                terrainData.alphamapResolution = resolution;

                //load all texture information
                List<SplatPrototype> splatPrototypes = new List<SplatPrototype>();
                foreach (var textureData in genericGameComponent.Get<GenericData[]>("Surface Textures"))
                {
                    var textureName = textureData.Get<String>("TextureName");
                    var baseName = "Assets/SceelixAssets/" + textureName;
                    var tileSize = textureData.Get<float[]>("UV");

                    if (!File.Exists(baseName + ".png"))
                    {
                        //save the texture to 
                        File.WriteAllBytes(baseName + ".png", textureData.GetTexture("Texture").EncodeToPNG());

                        AssetDatabase.Refresh();
                    }

                    splatPrototypes.Add(new SplatPrototype()
                    {
                        texture = (Texture2D) (AssetDatabase.LoadAssetAtPath(baseName + ".png", typeof (Texture2D))),
                        tileSize = new Vector2(tileSize[0], tileSize[1])
                    });
                }
                terrainData.splatPrototypes = splatPrototypes.ToArray();

                //now, load the splat map information
                float[, ,] splatMap = new float[splatmapSentData.GetLength(0), splatmapSentData.GetLength(1), terrainData.splatPrototypes.Length];
                for (int i = 0; i < splatmapSentData.GetLength(0); i++)
                    for (int j = 0; j < splatmapSentData.GetLength(1); j++)
                        for (int k = 0; k < terrainData.splatPrototypes.Length; k++)
                            splatMap[i, j, k] = splatmapSentData[i, j][k];

                terrainData.SetAlphamaps(0, 0, splatMap);


                //finally, create the terrain components
                Terrain terrain = gameObject.AddComponent<Terrain>();
                TerrainCollider collider = gameObject.AddComponent<TerrainCollider>();

                terrain.terrainData = terrainData;
                collider.terrainData = terrainData;
            }
            else if (genericGameComponent.Name == "RigidBody")
            {
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();

                //if a rigidbody already exists, this will be null, so don't overwrite it
                if (rigidbody == null)
                    continue;

                rigidbody.mass = genericGameComponent.GetFloat("Mass");
                rigidbody.drag = genericGameComponent.GetFloat("Drag");
                rigidbody.angularDrag = genericGameComponent.GetFloat("Angular Drag");
                rigidbody.useGravity = genericGameComponent.Get<bool>("Use Gravity");
                rigidbody.isKinematic = genericGameComponent.Get<bool>("Is Kinematic");
                rigidbody.interpolation = genericGameComponent.GetEnum<RigidbodyInterpolation>("Interpolate");
                rigidbody.collisionDetectionMode =
                    genericGameComponent.GetEnum<CollisionDetectionMode>("Collision Detection");
            }
            else if (genericGameComponent.Name == "Mesh Collider")
            {
                MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

                //if a meshCollider already exists, this will be null, so don't overwrite it
                if (meshCollider == null)
                    continue;

                meshCollider.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
                meshCollider.convex = genericGameComponent.Get<bool>("IsConvex");
                meshCollider.isTrigger = genericGameComponent.Get<bool>("IsTrigger");
            }
            else if (genericGameComponent.Name == "Light")
            {
                Light light = gameObject.AddComponent<Light>();

                light.type = genericGameComponent.GetEnum<LightType>("Type");
                light.range = genericGameComponent.GetFloat("Range");
                light.color = genericGameComponent.GetColor("Color");
                light.intensity = genericGameComponent.GetFloat("Intensity");
                light.renderMode = genericGameComponent.GetEnum<LightRenderMode>("Render Mode");
                light.shadows = genericGameComponent.GetEnum<LightShadows>("Shadow Type");
            }

            AssetDatabase.SaveAssets();
        }
    }


    /// <summary>
    /// Because Unity doesn't handle a Scale of 0 very well, we need this function.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private float GetOneIfZero(float value)
    {
        return Math.Abs(value) < float.Epsilon ? 1 : value;
    }
}