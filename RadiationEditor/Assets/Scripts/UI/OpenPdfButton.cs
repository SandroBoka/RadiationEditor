using System;
using System.IO;
using System.Diagnostics;
using UnityEngine;

public class OpenPdfButton : MonoBehaviour
{
    [SerializeField] private string relativePath = "data/pdf/Primjena plazme u industriji pročišćavanja otpadnih voda.docx.pdf";

    public void OpenPdf()
    {
        string path = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (!File.Exists(path))
        {
            path = Path.Combine(Application.dataPath, relativePath);
        }

        if (!File.Exists(path))
        {
            UnityEngine.Debug.LogWarning($"PDF not found at '{path}'.");
            return;
        }

        try
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            string fullPath = Path.GetFullPath(path);
            try
            {
                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            }
            catch
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{fullPath}\"") { UseShellExecute = true });
            }
#else
            Application.OpenURL(new Uri(path).AbsoluteUri);
#endif
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning("Failed to open PDF: " + ex.Message);
        }
    }
}
