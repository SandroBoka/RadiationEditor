using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SFB;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RayPlaneIntersectionRunner : MonoBehaviour
{
    [Header("App Paths")]
    [SerializeField] private string appFolderName = "RayPlaneIntersection";
    [SerializeField] private string macAppName = "appMac";
    [SerializeField] private string windowsAppName = "app.exe";

    [Header("UI")]
    [SerializeField] private string title = "Ray-Plane Intersection";
    [SerializeField] private int minPoints = 1;

    string pointsText = "";
    string selectedPath = "";
    string statusMessage = "Enter number of points and choose a CSV file.";
    string outputText = "";
    Vector2 outputScroll;
    bool isRunning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureRunnerInScene()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "RayPlaneIntersection")
            return;

        if (FindObjectOfType<RayPlaneIntersectionRunner>() != null)
            return;

        var go = new GameObject("RayPlaneIntersectionRunner");
        go.AddComponent<RayPlaneIntersectionRunner>();
    }

    void OnGUI()
    {
        float width = Mathf.Min(700f, Screen.width - 40f);
        float height = Mathf.Min(560f, Screen.height - 40f);
        Rect panelRect = new Rect(20f, 20f, width, height);

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        GUILayout.BeginArea(panelRect, GUI.skin.box);
        GUILayout.Label(title, headerStyle);
        GUILayout.Space(8f);

        GUILayout.Label("Number of points to generate:");
        pointsText = GUILayout.TextField(pointsText, GUILayout.Height(26f));

        GUILayout.Space(6f);
        GUI.enabled = !isRunning;
        if (GUILayout.Button(isRunning ? "Running..." : "Select CSV & Run", GUILayout.Height(32f)))
        {
            BeginSelectAndRun();
        }
        GUI.enabled = true;

        GUILayout.Space(6f);
        GUILayout.Label("Selected CSV: " + (string.IsNullOrEmpty(selectedPath) ? "-" : selectedPath));
        GUILayout.Label("Status: " + statusMessage);

        GUILayout.Space(10f);
        GUILayout.Label("Output:");
        outputScroll = GUILayout.BeginScrollView(outputScroll, GUILayout.ExpandHeight(true));
        GUILayout.TextArea(outputText ?? string.Empty, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.Space(6f);
        if (GUILayout.Button("Go Back", GUILayout.Height(30f)))
            SceneManager.LoadMenu();

        GUILayout.EndArea();
    }

    void BeginSelectAndRun()
    {
        if (isRunning)
            return;

        if (!TryParsePoints(out int points))
            return;

        OpenCsvFile(path =>
        {
            if (string.IsNullOrEmpty(path))
            {
                statusMessage = "CSV selection canceled.";
                return;
            }

            selectedPath = path;
            RunAppAsync(path, points);
        });
    }

    bool TryParsePoints(out int points)
    {
        if (!int.TryParse(pointsText, out points))
        {
            statusMessage = "Please enter a valid whole number.";
            return false;
        }

        if (points < minPoints)
        {
            statusMessage = $"Number of points must be at least {minPoints}.";
            return false;
        }

        return true;
    }

    void OpenCsvFile(Action<string> onSelected)
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Open CSV", "", "csv");
        onSelected?.Invoke(path);
#else
        string startDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrEmpty(startDir))
            startDir = Application.persistentDataPath;

        var extensions = new[]
        {
            new ExtensionFilter("CSV", "csv"),
            new ExtensionFilter("All Files", "*")
        };

        try
        {
#if UNITY_STANDALONE_OSX
            var paths = StandaloneFileBrowser.OpenFilePanel("Open CSV", startDir, extensions, false);
            onSelected?.Invoke(paths != null && paths.Length > 0 ? paths[0] : "");
#else
            StandaloneFileBrowser.OpenFilePanelAsync("Open CSV", startDir, extensions, false, paths =>
            {
                onSelected?.Invoke(paths != null && paths.Length > 0 ? paths[0] : "");
            });
#endif
        }
        catch (Exception ex)
        {
            statusMessage = "CSV open failed: " + ex.Message;
        }
#endif
    }

    async void RunAppAsync(string csvPath, int points)
    {
        if (isRunning)
            return;

        string appPath = ResolveAppPath(out string lookupDetails);
        if (string.IsNullOrEmpty(appPath) || !File.Exists(appPath))
        {
            statusMessage = "App not found. " + lookupDetails;
            return;
        }

        isRunning = true;
        statusMessage = "Running...";
        outputText = string.Empty;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = appPath,
                Arguments = $"-p \"{csvPath}\" -n {points}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(appPath)
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            using var process = new Process { StartInfo = psi };
            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    stdout.AppendLine(args.Data);
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    stderr.AppendLine(args.Data);
            };

            if (!process.Start())
            {
                statusMessage = "Failed to start the process.";
                return;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit());

            string output = stdout.ToString();
            string error = stderr.ToString();
            outputText = string.IsNullOrWhiteSpace(output) ? error : output;

            statusMessage = process.ExitCode == 0
                ? "Completed."
                : $"Process exited with code {process.ExitCode}.";

            if (string.IsNullOrWhiteSpace(outputText))
                statusMessage += " No output captured.";
        }
        catch (Exception ex)
        {
            statusMessage = "Run failed: " + ex.Message;
        }
        finally
        {
            isRunning = false;
        }
    }

    string ResolveAppPath(out string lookupDetails)
    {
        string appName = GetAppNameForPlatform();
        if (string.IsNullOrEmpty(appName))
        {
            lookupDetails = "Unsupported platform.";
            return string.Empty;
        }

        var candidates = new List<string>();
        if (!string.IsNullOrEmpty(Application.streamingAssetsPath))
            candidates.Add(Path.Combine(Application.streamingAssetsPath, appFolderName, appName));
        if (!string.IsNullOrEmpty(Application.dataPath))
            candidates.Add(Path.Combine(Application.dataPath, appFolderName, appName));

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                lookupDetails = "";
                return candidate;
            }
        }

        lookupDetails = "Looked in: " + string.Join("; ", candidates);
        return candidates.Count > 0 ? candidates[0] : string.Empty;
    }

    string GetAppNameForPlatform()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return macAppName;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return windowsAppName;
            default:
                return string.Empty;
        }
    }
}
