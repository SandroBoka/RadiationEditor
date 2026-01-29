using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using SFB;

public class QadImporter : MonoBehaviour
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
        Debug.Log("here");
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Open QAD INP", "", "inp");
        if (!string.IsNullOrEmpty(path))
            ImportFromPath(path);
#else
        string startDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrEmpty(startDir))
            startDir = Application.persistentDataPath;

        var extensions = new[]
        {
            new ExtensionFilter("QAD INP", "inp"),
            new ExtensionFilter("All Files", "*")
        };

        try
        {
#if UNITY_STANDALONE_OSX
            var paths = StandaloneFileBrowser.OpenFilePanel("Open QAD INP", startDir, extensions, false);
            if (paths != null && paths.Length > 0)
                ImportFromPath(paths[0]);
#else
            StandaloneFileBrowser.OpenFilePanelAsync("Open QAD INP", startDir, extensions, false, paths =>
            {
                if (paths == null || paths.Length == 0)
                    return;

                ImportFromPath(paths[0]);
            });
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError("QAD INP open failed: " + ex.Message);
        }
#endif
    }

    public void ImportFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogWarning("QAD import aborted: path is empty.");
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError("QAD import failed: file not found at " + path);
            return;
        }

        if (ShapeManager.I == null)
        {
            Debug.LogError("QAD import failed: ShapeManager missing.");
            return;
        }

        try
        {
            using var reader = new StreamReader(path);

            if (clearExisting)
                ClearExistingShapes();

            int lineNumber = 1;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("SPH") || line.StartsWith("RPP") || line.StartsWith("RCC"))
                {
                    var fields = ParseQADLine(line);
                    if (fields.Count < ExpectedHeader.Length)
                    {
                        Debug.LogWarning($"QAD row {lineNumber} skipped: expected {ExpectedHeader.Length} columns, got {fields.Count}.");
                        continue;
                    }
                    TryCreateShape(fields, lineNumber);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("QAD import failed: " + ex.Message);
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
            Debug.LogWarning($"QAD row {lineNumber} skipped: unknown shape type '{typeValue}'.");
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
            Debug.LogWarning($"QAD row {lineNumber} skipped: invalid numeric values.");
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

    static bool TryParseFloat(string value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    static List<string> ParseQADLine(string line)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(line))
            return result;

        var t = Split(line);

        void AddCommon(
            string id,
            string type,
            string material,
            string px, string py, string pz,
            string sx, string sy, string sz,
            string rx, string ry, string rz,
            string r, string rX, string rZ,
            string height)
        {
            result.Add(id);
            result.Add(type);
            result.Add(material);

            result.Add(px);
            result.Add(py);
            result.Add(pz);

            result.Add(sx);
            result.Add(sy);
            result.Add(sz);

            result.Add(rx);
            result.Add(ry);
            result.Add(rz);

            result.Add(r);
            result.Add(rX);
            result.Add(rZ);

            result.Add(height);
        }

        if (line.StartsWith("SPH"))
        {
            AddCommon(
                id: t[1],
                type: "Sphere",
                material: "Concrete",
                px: t[2], py: t[3], pz: t[4],
                sx: "1", sy: "1", sz: "1",
                rx: "0", ry: "0", rz: "0",
                r: t[5], rX: "0", rZ: "0",
                height: "0"
            );
        }
        else if (line.StartsWith("RPP"))
        {
            AddCommon(
                id: t[1],
                type: "Cube",
                material: "Concrete",
                px: t[2], py: t[3], pz: t[4],
                sx: t[5], sy: t[6], sz: t[7],
                rx: "0", ry: "0", rz: "0",
                r: "0", rX: "0", rZ: "0",
                height: "0"
            );
        }
        else if (line.StartsWith("RCC"))
        {
            AddCommon(
                id: t[1],
                type: "Cylinder",
                material: "Concrete",
                px: t[2], py: t[3], pz: t[4],
                sx: "1", sy: "1", sz: "1",
                rx: "0", ry: "0", rz: "0",
                r: "0", rX: t[8], rZ: t[8],
                height: t[7]
            );
        }

        return result;
    }


    static string[] Split(string line)
    {
        return line.Split(
            (char[])null,
            StringSplitOptions.RemoveEmptyEntries
        );
    }
}
