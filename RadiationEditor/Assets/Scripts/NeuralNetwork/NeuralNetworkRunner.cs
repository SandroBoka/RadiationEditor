using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NeuralNetworkRunner : MonoBehaviour
{
    [Header("App Paths")]
    [SerializeField] private string appFolderName = "NeuralNetwork";
    [SerializeField] private string windowsAppName = "predict.exe";

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField rcInput; // r/c input (rub ili centar)
    [SerializeField] private TMP_InputField zInput; // Redni broj elementa (Z)
    [SerializeField] private TMP_InputField dInput; // Debljina štita (d)
    [SerializeField] private TMP_InputField eInput; // Energija gama zrake (E)
    [SerializeField] private Button runButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private string title = "NN Predikcija";

    bool isRunning;

    void Awake()
    {
        if (titleText == null)
            titleText = FindByName<TMP_Text>("NNTitle");
        if (rcInput == null)
            rcInput = FindByName<TMP_InputField>("RCInput");
        if (zInput == null)
            zInput = FindByName<TMP_InputField>("ZInput");
        if (dInput == null)
            dInput = FindByName<TMP_InputField>("DInput");
        if (eInput == null)
            eInput = FindByName<TMP_InputField>("EInput");
        if (runButton == null)
            runButton = FindByName<Button>("RunButton");
        if (backButton == null)
            backButton = FindByName<Button>("BackButton");
        if (statusText == null)
            statusText = FindByName<TMP_Text>("StatusText");
        if (outputText == null)
            outputText = FindByName<TMP_Text>("OutputText");

        if (titleText != null)
            titleText.text = title;

        // Configure input fields
        if (rcInput != null)
        {
            rcInput.contentType = TMP_InputField.ContentType.Standard;
            rcInput.characterLimit = 1;
            rcInput.placeholder?.GetComponent<TMP_Text>()?.SetText("r ili c");
        }

        if (zInput != null)
        {
            zInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            zInput.characterLimit = 20;
        }

        if (dInput != null)
        {
            dInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            dInput.characterLimit = 20;
        }

        if (eInput != null)
        {
            eInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            eInput.characterLimit = 20;
        }

        if (runButton != null)
            runButton.onClick.AddListener(BeginRun);
        if (backButton != null)
            backButton.onClick.AddListener(SceneManager.LoadMenu);

        SetStatus("Enter r/c, Z, d, and E values, then click Run.");
        SetOutput("");
    }

    void OnDestroy()
    {
        if (runButton != null)
            runButton.onClick.RemoveListener(BeginRun);
        if (backButton != null)
            backButton.onClick.RemoveListener(SceneManager.LoadMenu);
    }

    void BeginRun()
    {
        if (isRunning)
            return;

        if (!TryParseInputs(out string rc, out float z, out float d, out float e))
            return;

        RunAppAsync(rc, z, d, e);
    }

    bool TryParseInputs(out string rc, out float z, out float d, out float e)
    {
        rc = "";
        z = 0;
        d = 0;
        e = 0;

        // Validate r/c input
        if (rcInput == null || string.IsNullOrWhiteSpace(rcInput.text))
        {
            SetStatus("Please enter 'r' or 'c' for rub/centar.");
            return false;
        }

        string rcText = rcInput.text.Trim().ToLower();
        if (rcText != "r" && rcText != "c")
        {
            SetStatus("First input must be 'r' (rub) or 'c' (centar).");
            return false;
        }
        rc = rcText;

        // Validate Z (redni broj elementa)
        if (zInput == null || string.IsNullOrWhiteSpace(zInput.text))
        {
            SetStatus("Please enter Z (redni broj elementa).");
            return false;
        }

        if (!float.TryParse(zInput.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z))
        {
            SetStatus("Invalid number for Z (redni broj elementa).");
            return false;
        }

        // Validate d (debljina štita)
        if (dInput == null || string.IsNullOrWhiteSpace(dInput.text))
        {
            SetStatus("Please enter d (debljina štita).");
            return false;
        }

        if (!float.TryParse(dInput.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
        {
            SetStatus("Invalid number for d (debljina štita).");
            return false;
        }

        // Validate E (energija gama zrake)
        if (eInput == null || string.IsNullOrWhiteSpace(eInput.text))
        {
            SetStatus("Please enter E (energija gama zrake).");
            return false;
        }

        if (!float.TryParse(eInput.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out e))
        {
            SetStatus("Invalid number for E (energija gama zrake).");
            return false;
        }

        return true;
    }

    async void RunAppAsync(string rc, float z, float d, float e)
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
            // Build arguments: r/c Z d E
            string args = $"{rc} {z.ToString(System.Globalization.CultureInfo.InvariantCulture)} {d.ToString(System.Globalization.CultureInfo.InvariantCulture)} {e.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            var psi = new ProcessStartInfo
            {
                FileName = appPath,
                Arguments = args,
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
