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

public class NNPredictRunner : MonoBehaviour
{
    [Header("App")]
    [SerializeField] private string appFolderName = "Predict";
    [SerializeField] private string windowsAppName = "predict.exe";
    [SerializeField] private string macAppName = "predictMac"; // ako ikad zatreba

    [Header("UI")]
    [SerializeField] private TMP_Dropdown modeDropdown; // c / r
    [SerializeField] private TMP_InputField inputA;
    [SerializeField] private TMP_InputField inputB;
    [SerializeField] private TMP_InputField inputC;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private Button runButton;
    [SerializeField] private Button backButton;

    bool isRunning;

    void Awake()
    {
        if (runButton != null)
            runButton.onClick.AddListener(RunPredict);
        if (backButton == null)
            backButton = FindByName<Button>("BackButton");
        if (backButton != null)
            backButton.onClick.AddListener(SceneManager.LoadMenu);

        if (inputA != null)
            inputA.contentType = TMP_InputField.ContentType.DecimalNumber;
        if (inputB != null)
            inputB.contentType = TMP_InputField.ContentType.DecimalNumber;
        if (inputC != null)
            inputC.contentType = TMP_InputField.ContentType.DecimalNumber;
    }

    void OnDestroy()
    {
        if (runButton != null)
            runButton.onClick.RemoveListener(RunPredict);
        if (backButton != null)
            backButton.onClick.RemoveListener(SceneManager.LoadMenu);
    }

    async void RunPredict()
    {
        if (isRunning)
            return;

        if (!TryParseInputs(out string mode, out float a, out float b, out float c))
            return;

        string appPath = ResolveAppPath();
        if (!File.Exists(appPath))
        {
            SetStatus("predict.exe not found.");
            return;
        }

        isRunning = true;
        runButton.interactable = false;
        SetStatus("Running...");
        SetOutput("");

        try
        {
            string arguments = $"{mode} {a} {b} {c}";

            var psi = new ProcessStartInfo
            {
                FileName = appPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(appPath)
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    stdout.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    stderr.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit());

            string output = stdout.ToString();
            string error = stderr.ToString();

            SetOutput(!string.IsNullOrWhiteSpace(output) ? output : error);
            SetStatus(process.ExitCode == 0
                ? "Completed."
                : $"Exited with code {process.ExitCode}");
        }
        catch (Exception ex)
        {
            SetStatus("Failed: " + ex.Message);
        }
        finally
        {
            isRunning = false;
            runButton.interactable = true;
        }
    }

    bool TryParseInputs(out string mode, out float a, out float b, out float c)
    {
        mode = modeDropdown.options[modeDropdown.value].text.ToLower();

        if (mode != "c" && mode != "r")
        {
            SetStatus("Mode must be c or r.");
            a = b = c = 0;
            return false;
        }

        if (!float.TryParse(inputA.text, out a) ||
            !float.TryParse(inputB.text, out b) ||
            !float.TryParse(inputC.text, out c))
        {
            SetStatus("Please enter valid float values.");
            a = b = c = 0;
            return false;
        }

        return true;
    }

    string ResolveAppPath()
    {
        string exeName =
#if UNITY_STANDALONE_OSX
            macAppName;
#else
            windowsAppName;
#endif

        return Path.Combine(Application.streamingAssetsPath, appFolderName, exeName);
    }

    void SetStatus(string msg)
    {
        statusText.text = "Status: " + msg;
    }

    void SetOutput(string msg)
    {
        outputText.text = msg ?? "";
    }

    T FindByName<T>(string name) where T : Component
    {
        var rootObjects = gameObject.scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            var found = FindInChildren<T>(root.transform, name);
            if (found != null)
                return found;
        }
        return null;
    }

    T FindInChildren<T>(Transform parent, string name) where T : Component
    {
        if (parent.name == name)
            return parent.GetComponent<T>();

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            var found = FindInChildren<T>(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
