using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Urho
{

    public class IdManager
    {
        public void Clear()
        {
            nodeId = 0;
            componentId = 0;
        }

        public int CreateNodeId()
        {
            nodeId++;
            return nodeId;
        }

        public int CreateComponentId()
        {
            componentId++;
            return componentId;
        }

        public static IdManager Instance
        {
            get
            {
                if (instance == null) instance = new IdManager();
                return instance;
            }
        }

        private static IdManager instance;
        private int nodeId;
        private int componentId;
    }

    public class Attribute
    {

        public Attribute()
        {
        }

        public Attribute(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        [XmlAttribute("value")]
        public string valueString
        {
            get
            {
                if (value is bool)
                {
                    bool v = (bool)value;
                    return v ? "true" : "false";
                }
                if (value is Color)
                {
                    Color v = (Color)value;
                    return v.r + " " + v.g + " " + v.b + " " + v.a;
                }
                if (value is Vector2)
                {
                    Vector2 v = (Vector2)value;
                    return v.x + " " + v.y;
                }
                if (value is Vector3)
                {
                    Vector3 v = (Vector3)value;
                    return v.x + " " + v.y + " " + v.z;
                }
                if (value is Vector4)
                {
                    Vector4 v = (Vector4)value;
                    return v.x + " " + v.y + " " + v.z + " " + v.w;
                }
                if (value is Quaternion)
                {
                    Quaternion v = (Quaternion)value;
                    return v.w + " " + v.x + " " + v.y + " " + v.z;
                }
                return value.ToString();
            }
            set { }
        }

        [XmlAttribute("name")]
        public string name;

        [XmlIgnore]
        public object value;
    }

    public class Node
    {

        public Node()
        {
            id = IdManager.Instance.CreateNodeId();
            components = new List<Component>();
            nodes = new List<Node>();
            enabled = true;
        }

        public Node(GameObject source)
        {
            id = IdManager.Instance.CreateNodeId();
            name = source.name;
            enabled = source.activeInHierarchy;
            position = source.transform.localPosition;
            rotation = source.transform.localRotation;
            scale = source.transform.localScale;

            components = new List<Component>();
            UnityEngine.Light light = source.GetComponent<UnityEngine.Light>();
            if (light) components.Add(new Light(light));

            UnityEngine.Camera camera = source.GetComponent<UnityEngine.Camera>();
            if (camera) components.Add(new Camera(camera));

            UnityEngine.MeshRenderer meshRenderer = source.GetComponent<UnityEngine.MeshRenderer>();
            UnityEngine.MeshFilter meshFilter = source.GetComponent<UnityEngine.MeshFilter>();
            if (meshRenderer && meshFilter) components.Add(new StaticModel(meshRenderer, meshFilter));

            nodes = new List<Node>();
            foreach (UnityEngine.Transform child in source.transform)
            {
                nodes.Add(new Node(child.gameObject));
            }
        }

        public virtual Attribute[] GetAttributes()
        {
            return new Attribute[] {
				new Attribute("Name", name),
				new Attribute("Is Enabled", enabled),
				new Attribute("Position", position),
				new Attribute("Rotation", rotation),
				new Attribute("Scale", scale)
			};
        }

        public virtual void Validate()
        {
            foreach (Node node in nodes)
            {
                node.Validate();
            }
            foreach (Component component in components)
            {
                component.Validate();
            }
        }

        [XmlAttribute]
        public int id;

        [XmlElement("node")]
        public List<Node> nodes;

        [XmlElement("component")]
        public List<Component> components;

        [XmlElement("attribute")]
        public Attribute[] attributes
        {
            get { return GetAttributes(); }
            set { }
        }

        [XmlIgnore]
        public bool enabled;

        [XmlIgnore]
        public string name;

        [XmlIgnore]
        public Vector3 position;

        [XmlIgnore]
        public Quaternion rotation;

        [XmlIgnore]
        public Vector3 scale;
    }


    [XmlType("scene")]
    public class Scene : Node
    {
        public Scene()
        {
            components.Add(new Octree());
            components.Add(new DebugRenderer());
            components.Add(new Zone());
            components.Add(new Skybox());

            List<GameObject> rootObjects = new List<GameObject>();
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);

            foreach (GameObject node in rootObjects)
            {
                nodes.Add(new Node(node));
            }
        }

        public override Attribute[] GetAttributes()
        {
            return new Attribute[] { };
        }

    }

}

