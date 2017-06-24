using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.Rendering;
using System;

namespace Urho
{

    [XmlInclude(typeof(Octree))]
    [XmlInclude(typeof(DebugRenderer))]
    [XmlInclude(typeof(StaticModel))]
    [XmlInclude(typeof(Camera))]
    [XmlInclude(typeof(Light))]
    [XmlInclude(typeof(Zone))]
    [XmlInclude(typeof(Skybox))]
    public class Component
    {

        public Component()
        {
        }

        public Component(UnityEngine.Component source)
        {
            id = IdManager.Instance.CreateComponentId();
            UnityEngine.Behaviour behaviour = source as UnityEngine.Behaviour;
            enabled = behaviour ? behaviour.enabled : true;
        }

        public virtual Attribute[] GetAttributes()
        {
            return new Attribute[] { };
        }

        public virtual void Validate()
        { }

        [XmlAttribute]
        public int id;

        [XmlAttribute("type")]
        public string Type
        {
            get { return GetType().Name; }
            set { }
        }

        [XmlElement("attribute")]
        public Attribute[] attributes
        {
            get
            {
                Attribute[] a = GetAttributes();
                if (!enabled)
                {
                    Attribute[] result = new Attribute[a.Length + 1];
                    result[0] = new Attribute("Is Enabled", enabled);
                    a.CopyTo(result, 1);
                    return result;
                }
                return a;
            }
            set { }
        }

        [XmlIgnore]
        public bool enabled;
    }


    public class Octree : Component
    {
        public Octree() :
            base(null)
        { }
    }


    public class DebugRenderer : Component
    {
        public DebugRenderer() :
            base(null)
        { }
    }


    public class StaticModel : Component
    {
        public StaticModel() :
            base(null)
        { }

        public StaticModel(UnityEngine.MeshRenderer renderer, UnityEngine.MeshFilter filter) :
            base(renderer)
        {
            material = ResourceCache.Instance.GetMaterial(renderer.sharedMaterial, renderer);
            model = ResourceCache.Instance.GetModel(filter.sharedMesh, renderer);
            castShadows = renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off;
            if (renderer.lightmapIndex >= 0) lightMask = (int)LightmapBakeType.Realtime;
            else lightMask = (int)LightmapBakeType.Realtime | (int)LightmapBakeType.Mixed;
        }

        public override Attribute[] GetAttributes()
        {
            return new Attribute[] {
                (model != null) ? new Attribute("Model", "Model;" + model.name) : null,
				(material != null) ? new Attribute("Material", "Material;" + material.name) : null,
                new Attribute("Cast Shadows", castShadows),              
                new Attribute("Light Mask", lightMask)
			};
        }

        [XmlIgnore]
        public Material material;

        [XmlIgnore]
        public Model model;

        [XmlIgnore]
        public bool castShadows;

        [XmlIgnore]
        int lightMask;
    }


    public class Camera : Component
    {
        public Camera() :
            base(null)
        { }

        public Camera(UnityEngine.Camera source) :
            base(source)
        {
            fov = source.fieldOfView;
            nearClip = source.nearClipPlane;
            farClip = source.farClipPlane;
        }

        public override Attribute[] GetAttributes()
        {
            return new Attribute[] {
				new Attribute("FOV", fov),
				new Attribute("Near Clip", nearClip),
				new Attribute("Far Clip", farClip)
			};
        }

        [XmlIgnore]
        public float fov;
        [XmlIgnore]
        public float nearClip;
        [XmlIgnore]
        public float farClip;
    }

    public class Light : Component
    {

        public Light() :
            base(null)
        { }

        public Light(UnityEngine.Light source) :
            base(source)
        {
            type = lightTypeToString(source.type);
            color = source.color;
            brightness = source.intensity;
            range = source.range;
            fov = source.spotAngle;
            castShadows = (source.shadows != LightShadows.None);
            mask = (int)source.lightmapBakeType;
        }

        public override Attribute[] GetAttributes()
        {
            return new Attribute[] {
				new Attribute("Light Type", type),
				new Attribute("Color", color),
				new Attribute("Brightness Multiplier", brightness),
				new Attribute("Range", range),
				new Attribute("Spot FOV", fov),
				new Attribute("Cast Shadows", castShadows),
                new Attribute("Light Mask", mask)
			};
        }

        private static string lightTypeToString(LightType lightType)
        {
            switch (lightType)
            {
                case LightType.Directional: return "Directional";
                case LightType.Spot: return "Spot";
                case LightType.Point: return "Point";
            }
            return "Point";
        }


        [XmlIgnore]
        public string type;
        [XmlIgnore]
        public Color color;
        [XmlIgnore]
        public float brightness;
        [XmlIgnore]
        public float range;
        [XmlIgnore]
        public float fov;
        [XmlIgnore]
        public bool castShadows;
        [XmlIgnore]
        int mask;
    }

    public class Zone : Component
    {
        public Zone() :
            base(null)
        {
            ambientColor = new Color(0, 0, 0); // TODO
            bounds = new Bounds(new Vector3(), new Vector3(10000, 10000, 10000));
            fogColor = Color.black;
            fogStart = 10000;
            fogEnd = 10001;
        }

        public override Attribute[] GetAttributes()
        {
            return new Attribute[] {
				new Attribute("Bounding Box Min", bounds.min),
                new Attribute("Bounding Box Max", bounds.max),
                new Attribute("Ambient Color", ambientColor),
                new Attribute("Fog Color", fogColor),
                new Attribute("Fog Start", fogStart),
                new Attribute("Fog End", fogEnd)
			};
        }

        [XmlIgnore]
        Bounds bounds;

        [XmlIgnore]
        Color ambientColor;

        [XmlIgnore]
        Color fogColor;

        [XmlIgnore]
        float fogStart;

        [XmlIgnore]
        float fogEnd;
    }

    public class Skybox : Component
    {
        public Skybox() :
            base(null)
        {
            UnityEngine.Material unityMaterial = new UnityEngine.Material(Shader.Find("Skybox/Cubemap"));
            unityMaterial.name = "Sky";
            unityMaterial.SetTexture("_Tex", renderSky());
            material = ResourceCache.Instance.GetMaterial(unityMaterial, null);
        }

        public override Attribute[] GetAttributes()
        {
            return new Attribute[] {
                new Attribute("Model", "Model;DefaultModels/Box.mdl"),
                (material != null) ? new Attribute("Material", "Material;" + material.name) : null
			};
        }

        private UnityEngine.Cubemap renderSky()
        {
            UnityEngine.Cubemap cubemap = new UnityEngine.Cubemap(1024, TextureFormat.ARGB32, false);
            cubemap.name = "Sky";
            GameObject go = new GameObject("CubemapCamera");
            UnityEngine.Camera camera = go.AddComponent<UnityEngine.Camera>();
            camera.cullingMask = 0;
            camera.RenderToCubemap(cubemap);
            GameObject.DestroyImmediate(go);
            return cubemap;
        }

        [XmlIgnore]
        Material material;
    }
}
