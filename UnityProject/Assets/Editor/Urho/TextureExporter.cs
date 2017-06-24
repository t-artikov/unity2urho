using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;

public class TextureExporter
{

    public static void Init()
    {
        DebugFunction debugFunction = delegate(string str)
        {
            Debug.LogWarning("TextureExporter: " + str);
        };
        SetDebugFunction(Marshal.GetFunctionPointerForDelegate(debugFunction));
    }

    public static void AddTexture(string filePath, Texture texture)
    {
        AddTexture(filePath, texture.GetNativeTexturePtr());
        GL.IssuePluginEvent(GetRenderEventFunction(), 1);
    }

    public static void WaitFinish(string message)
    {
        EditorApplication.CallbackFunction callback = null;
        callback = delegate()
        {
            int count = GetUnfinishedTextureCount();
            if (count == 0)
            {
                EditorApplication.update -= callback;
                Debug.Log(message);
            }
        };
        EditorApplication.update += callback;
    }

    [DllImport("TextureExporterPlugin")]
    public static extern int GetUnfinishedTextureCount();

    [DllImport("TextureExporterPlugin")]
    private static extern void AddTexture([MarshalAs(UnmanagedType.LPWStr)] string filePath, System.IntPtr texture);

    [DllImport("TextureExporterPlugin")]
    private static extern void SetDebugFunction(System.IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DebugFunction([MarshalAs(UnmanagedType.LPWStr)] string str);

    [DllImport("TextureExporterPlugin")]
    private static extern System.IntPtr GetRenderEventFunction();
}
