using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Urho
{

    class ExporterMenuItem
    {
        [MenuItem("Export/To Urho3D...")]
        static void Export()
        {
            EditorSceneManager.SaveOpenScenes();
            string defaultFileName = (prevFilePath != null) ? Path.GetFileName(prevFilePath) : "scene.xml";
            string defaultDirectory = (prevFilePath != null) ? Path.GetDirectoryName(prevFilePath) : "";
            string filePath = EditorUtility.SaveFilePanel("Export to Urho3D", defaultDirectory, defaultFileName, "xml");
            if (filePath.Length == 0) return;
            Urho.Exporter.Settings settings = new Urho.Exporter.Settings();
            settings.filePath = filePath;
            Urho.Exporter exporter = new Urho.Exporter(settings);
            exporter.Export();
            prevFilePath = filePath;
        }
        private static string prevFilePath;
    }

}

