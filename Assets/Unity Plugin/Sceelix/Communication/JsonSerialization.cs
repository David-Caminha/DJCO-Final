using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;


public class JsonSerialization
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace, Binder = new JsonBinder() };

        /// <summary>
        /// Saves an object to a json file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="path">Path of the file.</param>
        /// <param name="obj">Object to be saved.</param>
        /// <param name="typeNameHandling">Type name handling (None, Object,All).</param>
        public static void ToFile<T>(String path, T obj)
        {
            String json = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads an object from a json file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="path">Path to the file.</param>
        /// <param name="typeNameHandling">Type name handling (None, Object,All).</param>
        /// <returns>Object of the indicated type.</returns>
        public static T FromFile<T>(String path)
        {
            String jsonText = File.ReadAllText(path);

            //var settings = new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.Auto, ObjectCreationHandling = ObjectCreationHandling.Replace };
            
            return JsonConvert.DeserializeObject<T>(jsonText, settings);
        }


        /// <summary>
        /// Saves an object to a json file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="obj">Object to be saved.</param>
        public static String ToString<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }

        /// <summary>
        /// Loads an object from a json file.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="jsonText"></param>
        /// <returns>Object of the indicated type.</returns>
        public static T FromString<T>(String jsonText)
        {
            return JsonConvert.DeserializeObject<T>(jsonText, settings);
        }
    }

    //Looks like Unity does not support the .NET 4 version of SerializationBinder which has the BindToName function
    internal class JsonBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            //Debug.Log(Type.GetType(typeName));
             
            return Type.GetType(typeName);
        }
    }