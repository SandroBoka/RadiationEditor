using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField pointsInput;
    [SerializeField] private Button runButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text selectedPathText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private string title = "Ray-Plane Intersection";
    [SerializeField] private int minPoints = 1;

    string selectedPath = "";
    bool isRunning;

    void Awake()
    {
        if (titleText == null)
            titleText = FindByName<TMP_Text>("RayPlaneTitle");
        if (pointsInput == null)
            pointsInput = FindByName<TMP_InputField>("PointsInput");
        if (runButton == null)
            runButton = FindByName<Button>("RunButton");
        if (backButton == null)
            backButton = FindByName<Button>("BackButton");
        if (selectedPathText == null)
            selectedPathText = FindByName<TMP_Text>("SelectedPathText");
        if (statusText == null)
            statusText = FindByName<TMP_Text>("StatusText");
        if (outputText == null)
            outputText = FindByName<TMP_Text>("OutputText");

        if (titleText != null)
            titleText.text = title;

        if (pointsInput != null)
        {
            pointsInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            pointsInput.characterLimit = 9;
        }

        if (runButton != null)
            runButton.onClick.AddListener(BeginSelectAndRun);
        if (backButton != null)
            backButton.onClick.AddListener(SceneManager.LoadMenu);

        SetStatus("Enter number of points and choose a CSV file.");
        SetSelectedPath("-");
        SetOutput("");
    }

    void OnDestroy()
    {
        if (runButton != null)
            runButton.onClick.RemoveListener(BeginSelectAndRun);
        if (backButton != null)
            backButton.onClick.RemoveListener(SceneManager.LoadMenu);
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
                SetStatus("CSV selection canceled.");
                return;
            }

            selectedPath = path;
            RunAppAsync(path, points);
        });
    }

    bool TryParsePoints(out int points)
    {
        points = 0;
        if (pointsInput == null || string.IsNullOrWhiteSpace(pointsInput.text))
        {
            SetStatus("Please enter a valid whole number.");
            return false;
        }

        if (!int.TryParse(pointsInput.text, out points))
        {
            SetStatus("Please enter a valid whole number.");
            return false;
        }

        if (points < minPoints)
        {
            SetStatus($"Number of points must be at least {minPoints}.");
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
            SetStatus("CSV open failed: " + ex.Message);
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
            SetStatus("App not found. " + lookupDetails);
            return;
        }

        isRunning = true;
        SetStatus("Running...");
        SetOutput("");
        SetRunInteractable(false);

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
                SetStatus("Failed to start the process.");
                return;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit());

            string output = stdout.ToString();
            string error = stderr.ToString();
            SetOutput(string.IsNullOrWhiteSpace(output) ? error : output);

            SetStatus(process.ExitCode == 0
                ? "Completed."
                : $"Process exited with code {process.ExitCode}.");

            if (outputText != null && string.IsNullOrWhiteSpace(outputText.text))
                SetStatus(statusText != null ? statusText.text + " No output captured." : "No output captured.");
        }
        catch (Exception ex)
        {
            SetStatus("Run failed: " + ex.Message);
        }
        finally
        {
            isRunning = false;
            SetRunInteractable(true);
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

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = "Status: " + message;
    }

    void SetSelectedPath(string path)
    {
        if (selectedPathText != null)
            selectedPathText.text = "Selected CSV: " + path;
    }

    void SetOutput(string text)
    {
        if (outputText != null)
            outputText.text = string.IsNullOrEmpty(text) ? string.Empty : text;
    }

    void SetRunInteractable(bool isInteractable)
    {
        if (runButton != null)
            runButton.interactable = isInteractable;
    }

    static T FindByName<T>(string objectName) where T : Component
    {
        var obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<T>() : null;
    }
}
