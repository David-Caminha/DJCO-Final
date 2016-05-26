using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Sceelix.GameComponents.Data
{
    public class GenericData
    {
        private String _name;
        private Dictionary<String, Object> _properties = new Dictionary<String, Object>();
        //private readonly Dictionary<String, List<GenericData>> _subData = new Dictionary<String, List<GenericData>>();

        public GenericData(string name)
        {
            _name = name;
        }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Dictionary<String, Object> Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }



        public void Set<T>(String name, T value)
        {
            /*GenericData genericData = value as GenericData;
            if (genericData != null)
            {
                if (!_properties.ContainsKey(name))
                    _properties.Add(name, new List<GenericData>() { genericData });
                else
                    _properties[name] = value;
            }
            else*/
            {
                if (!_properties.ContainsKey(name))
                    _properties.Add(name, value);
                else
                    _properties[name] = value;
            }
        }

        public Object Get(String name)
        {
            Object value;

            if (_properties.TryGetValue(name, out value))
                return value;

            return new object();
        }

        public T Get<T>(String name)
        {
            Object value;

            if (_properties.TryGetValue(name, out value))
            {
                if (value is T)
                    return (T)value;

                //this avoid issues such as JSON.NET converting ints to longs
                return (T)Convert.ChangeType(value, typeof(T));
            }
                

            return default(T);
        }

        public TR Get<T, TR>(String name, Func<T, TR> action)
        {
            Object value;

            if (_properties.TryGetValue(name, out value))
                return action((T)value);

            return default(TR);
        }

        public T Get<T>(String name, T defaultValue)
        {
            Object value;

            if (_properties.TryGetValue(name, out value))
            {
                return (T)value;
            }

            return defaultValue;
        }


        /*public List<GenericData> this[String name]
        {
            get
            {
                List<GenericData> value;

                if (_subData.TryGetValue(name, out value))
                    return value;

                return null;
            }
            set
            {
                if (!_subData.ContainsKey(name))
                    _subData.Add(name, value);
                else
                    _subData[name] = value;
            }
        }
         */

        public T GetEnum<T>(string key)
        {
            String enumString = Get<String>(key);
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public Color GetColor(string key)
        {
            return Get<float[], Color>(key, x => new Color(x[0], x[1], x[2],x[3]));
        }

        public float GetFloat(string key)
        {
            return Convert.ToSingle(Get(key));
        }

        public bool ContainsKey(string key)
        {
            return Properties.ContainsKey(key);
        }

        public Texture2D GetTexture(string key)
        {
            Texture2D texture2D = new Texture2D(1, 1);

            byte[] bytes = Get<byte[]>(key);
            texture2D.LoadImage(bytes);

            return texture2D;
        }

        public Vector3 GetVector3(string key)
        {
            return Get<float[], Vector3>(key, x => new Vector3(x[0], x[1], x[2]));
        }

        public Quaternion GetQuartenion(string key)
        {
            return Get<float[], Quaternion>(key, x => Quaternion.Euler(x[0], x[1], x[2]));
        }
    }
}
