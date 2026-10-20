using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public static class ScreenshotUtility
    {
        private const string Folder = "Assets/Screenshots";

        [MenuItem("Tools/Capture Game View Screenshot %#s")]
        private static void Capture()
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            string fileName = $"Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            string relativePath = $"{Folder}/{fileName}";
            string fullPath = Path.GetFullPath(relativePath);

            ScreenCapture.CaptureScreenshot(fullPath);

            AssetDatabase.Refresh();

            Debug.Log($"Game View screenshot saved: {relativePath}");
        }
    }
}