using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Reflection;

namespace Urho
{

    public class ResourceCacheEntry<T> where T : Resource
    {

        public ResourceCacheEntry(string category, Func<object, object, T> creator)
        {
            this.category = category;
            this.creator = creator;
            resources = new List<T>();
        }

        public void Clear()
        {
            resources.Clear();
        }

        public void Save(string rootDirectory)
        {
            string directory = rootDirectory + "/" + category;
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            foreach (T resource in resources)
            {
                resource.Save(rootDirectory);
            }
        }

        public T GetResource(object source, object instanceData)
        {
            if (source == null) return null;
            foreach (T resource in resources)
            {
                if (resource.ResourceEquals(source, instanceData)) return resource;
            }
            T result = creator(source, instanceData);
            if (result == null) return null;
            result.name = CreateUniqueName(result.name);
            resources.Add(result);
            return result;
        }

        public T GetResource(object source)
        {
            return GetResource(source, null);
        }

        private string CreateUniqueName(string baseName)
        {
            if (baseName[0] == '.') baseName = "unnamed" + baseName;
            baseName = category + "/" + baseName;
            string result = baseName;
            int i = 1;
            while (resources.Find(resource => resource.name == result) != null)
            {
                String ext = Path.GetExtension(baseName);
                String name = baseName.Remove(baseName.Length - ext.Length);
                result = name + "(" + i + ")" + ext;
                i++;
            }
            return result;
        }

        private List<T> resources;
        private string category;
        private Func<object, object, T> creator;
    }

    public class ResourceCache
    {

        ResourceCache()
        {
            Func<object, object, Texture> textureCreator = (source, instanceData) => new Texture((UnityEngine.Texture)source);
            textures = new ResourceCacheEntry<Texture>("Textures", textureCreator);
            Func<object, object, Material> materialCreator = (source, instanceData) => createMaterial((UnityEngine.Material)source, (UnityEngine.MeshRenderer)instanceData);
            materials = new ResourceCacheEntry<Material>("Materials", materialCreator);
            Func<object, object, Model> modelCreator = (source, instanceData) => new Model((UnityEngine.Mesh)source, (UnityEngine.MeshRenderer)instanceData);
            models = new ResourceCacheEntry<Model>("Models", modelCreator);
        }

        private Material createMaterial(UnityEngine.Material source, UnityEngine.MeshRenderer meshRenderer)
        {
            if (source == null || source.shader == null) return null;
            if (source.shader.name == "Standard") return new StandardMaterial(source, meshRenderer);
            if (source.shader.name == "Standard (Specular setup)") return new StandardMaterial(source, meshRenderer);
            if (source.shader.name == "Skybox/Cubemap") return new SkyboxMaterial(source);
            Debug.LogWarning("Unknown material type: " + source.shader.name);
            return null;
        }

        public void Clear()
        {
            textures.Clear();
            materials.Clear();
            models.Clear();
        }

        public void Save(string rootDirectory)
        {
            textures.Save(rootDirectory);
            materials.Save(rootDirectory);
            models.Save(rootDirectory);
        }

        public Texture GetTexture(UnityEngine.Texture source)
        {
            return textures.GetResource(source);
        }

        public Material GetMaterial(UnityEngine.Material source, UnityEngine.MeshRenderer meshRenderer)
        {
            return materials.GetResource(source, meshRenderer);
        }

        public Material GetMaterial(UnityEngine.Material source)
        {
            return GetMaterial(source, null);
        }

        public Model GetModel(UnityEngine.Mesh source, UnityEngine.MeshRenderer meshRenderer)
        {
            return models.GetResource(source, meshRenderer);
        }

        public void SaveToXml(object obj, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            StreamWriter streamWriter = new StreamWriter(filePath);
            serializer.Serialize(streamWriter, obj, ns);
            streamWriter.Close();
        }

        public static ResourceCache Instance
        {
            get
            {
                if (instance == null) instance = new ResourceCache();
                return instance;
            }
        }

        private static ResourceCache instance;
        private ResourceCacheEntry<Texture> textures;
        private ResourceCacheEntry<Material> materials;
        private ResourceCacheEntry<Model> models;
    }

    public class Resource
    {

        public Resource()
        {
        }

        public Resource(object source, object instanceData, string name)
        {
            this.source = source;
            this.instanceData = instanceData;
            this.name = name;
        }

        public Resource(object source, string name) :
            this(source, null, name)
        { }

        public virtual bool ResourceEquals(object otherSource, object otherInstanceData)
        {
            return source == otherSource && instanceData == otherInstanceData;
        }

        public virtual void Save(string rootDirectory)
        {
        }

        public virtual void Validate()
        {
        }

        [XmlIgnore]
        public string name;
        [XmlIgnore]
        public object source;
        [XmlIgnore]
        public object instanceData;
    }

    [XmlType("cubemap")]
    public class Cubemap
    {

        public class CubemapImage
        {
            [XmlAttribute]
            public string name;
        }

        public Cubemap()
        { }

        public Cubemap(Texture texture)
        {
            image = new CubemapImage();
            image.name = Path.GetFileName(texture.name);
        }

        [XmlElement]
        public CubemapImage image;
    };

    public class TextureWrapSettings
    {
        TextureWrapSettings()
        {
        }
        public TextureWrapSettings(int coord, UnityEngine.Texture source)
        {
            switch (coord)
            {
                case 0: this.coord = "u"; break;
                case 1: this.coord = "v"; break;
                case 2: this.coord = "w"; break;
            }
            switch (source.wrapMode)
            {
                case TextureWrapMode.Clamp: mode = "clamp"; break;
                default: mode = "wrap"; break;
            }
        }

        [XmlAttribute]
        public string coord;
        [XmlAttribute]
        public string mode;
    }

    public class TextureFilterSettings
    {
        TextureFilterSettings()
        {
        }
        public TextureFilterSettings(UnityEngine.Texture source)
        {
            switch (source.filterMode)
            {
                case FilterMode.Point: mode = "nearest"; break;
                case FilterMode.Trilinear: mode = "trilinear"; break;
                default: mode = "bilinear"; break;
            }
        }
        [XmlAttribute]
        public string mode;
    }

    public class TextureSrgbSettings
    {
        TextureSrgbSettings()
        {
        }
        public TextureSrgbSettings(UnityEngine.Texture source)
        {
            enable = false; // TODO
        }
        [XmlAttribute]
        public bool enable;
    }

    [XmlType("texture")]
    public class Texture : Resource
    {

        public Texture()
        {
        }

        public Texture(UnityEngine.Texture source) :
            base(source, source.name + ".dds")
        {
            wrapSettings = new TextureWrapSettings[2];
            wrapSettings[0] = new TextureWrapSettings(0, source);
            wrapSettings[1] = new TextureWrapSettings(1, source);
            filterSettings = new TextureFilterSettings(source);
            srgbSettings = new TextureSrgbSettings(source);
        }

        public override void Save(string rootDirectory)
        {
            string filePath = rootDirectory + "/" + name;
            UnityEngine.Texture texture = (UnityEngine.Texture)source;
            TextureExporter.AddTexture(filePath, texture);
            if (isCubemap)
            {
                Cubemap cubemap = new Cubemap(this);
                string cubemapFilename = filePath.Remove(filePath.Length - Path.GetExtension(filePath).Length) + "_cube.xml";
                ResourceCache.Instance.SaveToXml(cubemap, cubemapFilename);
            }
            string descFilePath = filePath.Remove(filePath.Length - Path.GetExtension(filePath).Length) + ".xml";
            ResourceCache.Instance.SaveToXml(this, descFilePath);
        }

        public bool isCubemap
        {
            get
            {
                return source is UnityEngine.Cubemap;
            }
        }

        [XmlElement("address")]
        public TextureWrapSettings[] wrapSettings;

        [XmlElement("filter")]
        public TextureFilterSettings filterSettings;

        [XmlElement("srgb")]
        public TextureSrgbSettings srgbSettings;
    }

    public class TechniqueRef
    {

        public TechniqueRef()
        {
        }

        public TechniqueRef(string name, int quality)
        {
            this.name = name;
            this.quality = quality;
        }

        [XmlAttribute]
        public string name;

        [XmlAttribute]
        public int quality;
    }

    public class TextureUnit
    {

        public TextureUnit()
        { }

        public TextureUnit(string unit, Texture texture)
        {
            this.unit = unit;
            name = texture.name;
            if (texture.isCubemap)
            {
                name = name.Remove(name.Length - Path.GetExtension(name).Length) + "_cube.xml";
            }
        }

        [XmlAttribute]
        public string unit;

        [XmlAttribute]
        public string name;
    }

    [XmlType("material")]
    public class Material : Resource
    {
        public Material()
        {
        }

        public Material(UnityEngine.Material source, MeshRenderer instanceData) :
            base(source, instanceData, source.name + ".xml")
        { }

        public Material(UnityEngine.Material source) :
            this(source, null)
        { }

        public override void Save(string rootDirectory)
        {
            ResourceCache.Instance.SaveToXml(this, rootDirectory + "/" + name);
        }
    }

    [XmlType("material")]
    public class StandardMaterial : Material
    {

        public StandardMaterial()
        {
        }

        public StandardMaterial(UnityEngine.Material source, UnityEngine.MeshRenderer instanceData) :
            base(source, instanceData)
        {
            diffuseColor = source.color;
            uvOffset = source.mainTextureOffset;
            uvScale = source.mainTextureScale;
            diffuseTexture = ResourceCache.Instance.GetTexture(source.mainTexture);
            if (diffuseTexture == null) diffuseTexture = ResourceCache.Instance.GetTexture(UnityEngine.Texture2D.whiteTexture);
            normalTexture = ResourceCache.Instance.GetTexture(source.GetTexture("_BumpMap"));
            if (instanceData.lightmapIndex >= 0)
            {
                lightmapTexture = ResourceCache.Instance.GetTexture(LightmapSettings.lightmaps[instanceData.lightmapIndex].lightmapColor);
            }
        }

        public override bool ResourceEquals(object otherSource, object otherInstanceData)
        {
            if (source != otherSource) return false;
            UnityEngine.MeshRenderer m1 = (UnityEngine.MeshRenderer)instanceData;
            UnityEngine.MeshRenderer m2 = (UnityEngine.MeshRenderer)otherInstanceData;
            return m1.lightmapIndex == m2.lightmapIndex;
        }

        [XmlElement("technique")]
        public TechniqueRef[] techniques
        {
            get
            {
                string technique = "Techniques/UnityDiff";
                if (normalTexture != null) technique += "NormalPacked";
                if (lightmapTexture != null) technique += "LightMap";

                technique += ".xml";
                return new TechniqueRef[] {
                    new TechniqueRef(technique, 1)
                };
            }
            set { }
        }

        [XmlElement("texture")]
        public TextureUnit[] textures
        {
            get
            {
                return new TextureUnit[] {
                    (diffuseTexture != null) ? new TextureUnit("diffuse", diffuseTexture) : null,
                    (normalTexture != null) ? new TextureUnit("normal", normalTexture) : null,
                    (lightmapTexture != null) ? new TextureUnit("emissive", lightmapTexture) : null
                };
            }
            set { }
        }

        [XmlElement("parameter")]
        public Attribute[] attributes
        {
            get
            {
                bool hasDiffuseColor = diffuseColor != Color.white;
                bool hasUTransform = (uvScale.x != 1) || (uvOffset.x != 0);
                bool hasVTransform = (uvScale.y != 1) || (uvOffset.y != 0);
                return new Attribute[] {
					hasDiffuseColor ? new Attribute("MatDiffColor", diffuseColor) : null,
                    hasUTransform ? new Attribute("UOffset", new Vector4(uvScale.x, 0, 0, uvOffset.x)) : null,
                    hasVTransform ? new Attribute("VOffset", new Vector4(0, uvScale.y, 0, uvOffset.y)) : null
				};
            }
            set { }
        }

        [XmlIgnore]
        public Color diffuseColor;

        [XmlIgnore]
        public Vector2 uvOffset;

        [XmlIgnore]
        public Vector2 uvScale;

        [XmlIgnore]
        public Texture diffuseTexture;

        [XmlIgnore]
        public Texture normalTexture;

        [XmlIgnore]
        public Texture lightmapTexture;

    }

    [XmlType("material")]
    public class SkyboxMaterial : Material
    {

        public class CullSettings
        {
            public CullSettings()
            {
                value = "none";
            }
            [XmlAttribute]
            public string value;
        }

        public SkyboxMaterial()
        {
        }

        public SkyboxMaterial(UnityEngine.Material source) :
            base(source)
        {
            texture = ResourceCache.Instance.GetTexture(source.GetTexture("_Tex"));
            cullSettings = new CullSettings();
        }

        [XmlElement("technique")]
        public TechniqueRef[] techniques
        {
            get
            {
                string technique = "Techniques/DiffSkybox.xml";
                return new TechniqueRef[] {
                    new TechniqueRef(technique, 1)
                };
            }
            set { }
        }

        [XmlElement("texture")]
        public TextureUnit[] textures
        {
            get
            {
                return new TextureUnit[] {
                    (texture != null) ? new TextureUnit("diffuse", texture) : null
                };
            }
            set { }
        }

        [XmlIgnore]
        public Texture texture;

        [XmlElement("cull")]
        public CullSettings cullSettings;
    }

    [Flags]
    enum VertexFormat
    {
        None = 0,
        Position = 1,
        Normal = 2,
        Color = 4,
        Texcoord1 = 8,
        Texcoord2 = 16,
        CubeTexcoord1 = 32,
        CubeTexcoord2 = 64,
        Tangent = 128
    }

    public class Model : Resource
    {
        public Model(UnityEngine.Mesh source, UnityEngine.MeshRenderer instanceData) :
            base(source, instanceData, source.name + ".mdl")
        { }

        public override bool ResourceEquals(object otherSource, object otherInstanceData)
        {
            if (source != otherSource) return false;
            UnityEngine.MeshRenderer m1 = (UnityEngine.MeshRenderer)instanceData;
            UnityEngine.MeshRenderer m2 = (UnityEngine.MeshRenderer)otherInstanceData;
            return m1.lightmapIndex < 0 && m2.lightmapIndex < 0;
        }

        public override void Save(string rootDirectory)
        {
            string filePath = rootDirectory + "/" + name;
            BinaryWriter writer = new BinaryWriter(new BufferedStream(new FileStream(filePath, FileMode.Create), 1024 * 100));
            UnityEngine.Mesh mesh = (UnityEngine.Mesh)source;
            UnityEngine.MeshRenderer meshRenderer = (UnityEngine.MeshRenderer)instanceData;
            int meshVertexCount = mesh.vertexCount;
            Vector3[] meshVertices = mesh.vertices;
            Vector3[] meshNormals = mesh.normals;
            Color[] meshColors = mesh.colors;
            Vector2[] meshUv = mesh.uv;
            Vector2[] meshUv2 = mesh.uv2;
            Vector4 uv2Transform = meshRenderer.lightmapScaleOffset;
            Vector4[] meshTangents = mesh.tangents;
            int[] meshTriangles = mesh.triangles;

            writer.Write(new byte[] { // Identifier "UMDL"
				(byte)'U', 
				(byte)'M',
				(byte)'D', 
				(byte)'L'
			});

            writer.Write(1U); // Number of vertex buffers
            // For each vertex buffer: 
            {
                writer.Write((uint)meshVertexCount); // Vertex count

                bool hasNormals = meshNormals.Length > 0;
                bool hasColors = false; // TODO
                bool hasTextcoord1 = meshUv.Length > 0;
                bool hasTextcoord2 = meshUv2.Length > 0 && meshRenderer.lightmapIndex >= 0;
                bool hasTangents = meshTangents.Length > 0;
                VertexFormat format = VertexFormat.None;
                format |= VertexFormat.Position;
                if (hasNormals) format |= VertexFormat.Normal;
                if (hasColors) format |= VertexFormat.Color;
                if (hasTextcoord1) format |= VertexFormat.Texcoord1;
                if (hasTextcoord2) format |= VertexFormat.Texcoord2;
                if (hasTangents) format |= VertexFormat.Tangent;
                writer.Write((uint)format); // Vertex element mask

                writer.Write(0U); // Morphable vertex range start index
                writer.Write(0U); // Morphable vertex count

                for (int i = 0; i < meshVertexCount; i++)
                { // Vertex data (vertex count * vertex size)  
                    writer.Write(meshVertices[i].x);
                    writer.Write(meshVertices[i].y);
                    writer.Write(meshVertices[i].z);
                    if (hasNormals)
                    {
                        writer.Write(meshNormals[i].x);
                        writer.Write(meshNormals[i].y);
                        writer.Write(meshNormals[i].z);
                    }
                    if (hasColors)
                    {
                        writer.Write(meshColors[i].r);
                        writer.Write(meshColors[i].g);
                        writer.Write(meshColors[i].b);
                        writer.Write(meshColors[i].a);
                    }
                    if (hasTextcoord1)
                    {
                        writer.Write(meshUv[i].x);
                        writer.Write(meshUv[i].y);
                    }
                    if (hasTextcoord2)
                    {
                        writer.Write(meshUv2[i].x * uv2Transform.x + uv2Transform.z);
                        writer.Write(meshUv2[i].y * uv2Transform.y + uv2Transform.w);
                    }
                    if (hasTangents)
                    {
                        writer.Write(meshTangents[i].x);
                        writer.Write(meshTangents[i].y);
                        writer.Write(meshTangents[i].z);
                        writer.Write(meshTangents[i].w);
                    }
                }
            }

            writer.Write(1U); // Number of index buffers
            // For each index buffer:
            {
                writer.Write((uint)meshTriangles.Length); // Index count
                writer.Write((uint)4U); // Index size
                for (int i = 0; i < meshTriangles.Length; i++)
                { // Index data (index count * index size)
                    writer.Write((uint)meshTriangles[i]);
                }
            }

            writer.Write(1U); // Number of geometries
            //For each geometry:
            {
                writer.Write(0U); // Number of bone mapping entries
                writer.Write(1U); // Number of LOD levels
                // For each LOD level:
                {
                    writer.Write(0.0f); // LOD distance
                    writer.Write(0U); // Primitive type (0 = triangle list, 1 = line list)
                    writer.Write(0U); // Vertex buffer index
                    writer.Write(0U); // Index buffer index
                    writer.Write(0U); // Draw range: index start
                    writer.Write((uint)meshTriangles.Length); // Draw range: index count
                }
            }


            writer.Write(0U); // Number of vertex morphs
            writer.Write(0U); // Number of bones

            // Bounding box data
            writer.Write(mesh.bounds.min.x);
            writer.Write(mesh.bounds.min.y);
            writer.Write(mesh.bounds.min.z);
            writer.Write(mesh.bounds.max.x);
            writer.Write(mesh.bounds.max.y);
            writer.Write(mesh.bounds.max.z);

            // For each geometry:
            {
                //Geometry center
                writer.Write(0.0f);
                writer.Write(0.0f);
                writer.Write(0.0f);
            }

            writer.Close();
        }
    }

}