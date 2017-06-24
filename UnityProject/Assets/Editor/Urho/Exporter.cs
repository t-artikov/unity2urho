using UnityEngine;
using UnityEditor;
using System.IO;

namespace Urho
{

    class Exporter
    {
        public class Settings
        {
            public string filePath;
        }
        public Exporter(Settings settings)
        {
            this.settings = settings;
        }

        public void Export()
        {
            TextureExporter.Init();
            IdManager.Instance.Clear();
            ResourceCache.Instance.Clear();
            Scene scene = new Urho.Scene();
            scene.Validate();
            ResourceCache.Instance.SaveToXml(scene, settings.filePath);
            ResourceCache.Instance.Save(Path.GetDirectoryName(settings.filePath));
            TextureExporter.WaitFinish("Export to Urho3D completed");
        }

        private Settings settings;
    }

}
