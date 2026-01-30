using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using SFB;

public class CsvImporter : MonoBehaviour
{
    public bool clearExisting = true;

    static readonly string[] ExpectedHeader =
    {
        "id",
        "type",
        "material",
        "px",
        "py",
        "pz",
        "sx",
        "sy",
        "sz",
        "rx",
        "ry",
        "rz",
        "radius",
        "radiusX",
        "radiusZ",
        "height"
    };

    public void OpenAndImport()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Open CSV", "", "csv");
        if (!string.IsNullOrEmpty(path))
            ImportFromPath(path);
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
            if (paths != null && paths.Length > 0)
                ImportFromPath(paths[0]);
#else
            StandaloneFileBrowser.OpenFilePanelAsync("Open CSV", startDir, extensions, false, paths =>
            {
                if (paths == null || paths.Length == 0)
                    return;

                ImportFromPath(paths[0]);
            });
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError("CSV open failed: " + ex.Message);
        }
#endif
    }

    public void ImportFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning("CSV import aborted: path is empty.");
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError("CSV import failed: file not found at " + path);
            return;
        }

        if (ShapeManager.I == null)
        {
            Debug.LogError("CSV import failed: ShapeManager is missing.");
            return;
        }

        try
        {
            using var reader = new StreamReader(path);
            string headerLine = ReadNextNonEmptyLine(reader);
            if (string.IsNullOrEmpty(headerLine))
            {
                Debug.LogError("CSV import failed: file is empty.");
                return;
            }

            var header = ParseCsvLine(headerLine);
            if (!IsHeaderValid(header))
            {
                Debug.LogError("CSV import failed: header does not match exported format.");
                return;
            }

            if (clearExisting)
                ClearExistingShapes();

            int lineNumber = 1;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = ParseCsvLine(line);
                if (fields.Count < ExpectedHeader.Length)
                {
                    Debug.LogWarning($"CSV row {lineNumber} skipped: expected {ExpectedHeader.Length} columns, got {fields.Count}.");
                    continue;
                }

                TryCreateShape(fields, lineNumber);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("CSV import failed: " + ex.Message);
        }
    }

    void ClearExistingShapes()
    {
        ShapeManager.I.shapes.RemoveAll(s => s == null);
        for (int i = ShapeManager.I.shapes.Count - 1; i >= 0; i--)
        {
            var shape = ShapeManager.I.shapes[i];
            if (shape != null)
                Destroy(shape.gameObject);
        }
        ShapeManager.I.shapes.Clear();
        FindObjectOfType<SelectionManager>()?.ClearSelection();
    }

    bool TryCreateShape(IReadOnlyList<string> fields, int lineNumber)
    {
        string typeValue = fields[1].Trim();
        if (!Enum.TryParse(typeValue, true, out ShapeType type))
        {
            Debug.LogWarning($"CSV row {lineNumber} skipped: unknown shape type '{typeValue}'.");
            return false;
        }

        if (!TryParseFloat(fields[3], out float px) ||
            !TryParseFloat(fields[4], out float py) ||
            !TryParseFloat(fields[5], out float pz) ||
            !TryParseFloat(fields[6], out float sx) ||
            !TryParseFloat(fields[7], out float sy) ||
            !TryParseFloat(fields[8], out float sz) ||
            !TryParseFloat(fields[9], out float rx) ||
            !TryParseFloat(fields[10], out float ry) ||
            !TryParseFloat(fields[11], out float rz))
        {
            Debug.LogWarning($"CSV row {lineNumber} skipped: invalid numeric values.");
            return false;
        }

        Vector3 position = new Vector3(px, py, pz);
        Vector3 scale = new Vector3(sx, sy, sz);
        Vector3 rotation = new Vector3(rx, ry, rz);

        string materialName = fields[2].Trim();
        Material mat = FindMaterial(materialName);

        ShapeData data = ShapeManager.I.CreateShape(type, position, mat);
        data.transform.localScale = scale;
        data.transform.rotation = Quaternion.Euler(rotation);

        if (!string.IsNullOrEmpty(materialName))
            data.materialName = materialName;

        data.RecomputeDerived();
        return true;
    }

    Material FindMaterial(string materialName)
    {
        if (string.IsNullOrEmpty(materialName))
            return null;

        var lib = ShapeManager.I.materialLibrary;
        if (lib == null || lib.materials == null)
            return null;

        foreach (var mat in lib.materials)
        {
            if (mat == null)
                continue;
            if (string.Equals(mat.name, materialName, StringComparison.OrdinalIgnoreCase))
                return mat;
        }

        return null;
    }

    static string ReadNextNonEmptyLine(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }
        return null;
    }

    static bool IsHeaderValid(IReadOnlyList<string> header)
    {
        if (header == null || header.Count != ExpectedHeader.Length)
            return false;

        for (int i = 0; i < ExpectedHeader.Length; i++)
        {
            string value = header[i].Trim();
            if (!string.Equals(value, ExpectedHeader[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    static bool TryParseFloat(string value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null)
            return result;

        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }

}
