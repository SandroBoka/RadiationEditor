using System;
using System.IO;
using UnityEngine;

public class PdfOpener : MonoBehaviour
{
    [SerializeField] private string relativePath = "data/pdf/placeholder.pdf";

    public void OpenPdf()
    {
        string path = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (!File.Exists(path))
        {
            path = Path.Combine(Application.dataPath, relativePath);
        }

        if (!File.Exists(path))
        {
            Debug.LogWarning($"PDF not found at '{path}'.");
            return;
        }

        Application.OpenURL(new Uri(path).AbsoluteUri);
    }
}
