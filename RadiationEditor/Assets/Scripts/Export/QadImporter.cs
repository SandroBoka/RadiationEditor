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

        static string F(float v)
        {
            return v.ToString("G", CultureInfo.InvariantCulture);
        }

        bool TryGet(string value, out float parsed)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
        }

        void AddCommon(
            string id,
            string type,
            string material,
            Vector3 position,
            Vector3 scale,
            Vector3 rotation,
            float r,
            float rX,
            float rZ,
            float height)
        {
            result.Add(id);
            result.Add(type);
            result.Add(material);

            result.Add(F(position.x));
            result.Add(F(position.y));
            result.Add(F(position.z));

            result.Add(F(scale.x));
            result.Add(F(scale.y));
            result.Add(F(scale.z));

            result.Add(F(rotation.x));
            result.Add(F(rotation.y));
            result.Add(F(rotation.z));

            result.Add(F(r));
            result.Add(F(rX));
            result.Add(F(rZ));

            result.Add(F(height));
        }

        if (line.StartsWith("SPH"))
        {
            if (t.Length < 6)
                return result;
            if (!TryGet(t[2], out float x) ||
                !TryGet(t[3], out float y) ||
                !TryGet(t[4], out float z) ||
                !TryGet(t[5], out float radius))
                return result;

            Vector3 position = new Vector3(x, y, z);
            Vector3 scale = Vector3.one * (radius * 2f);
            Vector3 rotation = Vector3.zero;
            AddCommon(
                id: t[1],
                type: "Sphere",
                material: "Concrete",
                position: position,
                scale: scale,
                rotation: rotation,
                r: radius,
                rX: 0f,
                rZ: 0f,
                height: 0f
            );
        }
        else if (line.StartsWith("RPP"))
        {
            if (t.Length < 8)
                return result;
            if (!TryGet(t[2], out float x1) ||
                !TryGet(t[3], out float x2) ||
                !TryGet(t[4], out float y1) ||
                !TryGet(t[5], out float y2) ||
                !TryGet(t[6], out float z1) ||
                !TryGet(t[7], out float z2))
                return result;

            float minX = Mathf.Min(x1, x2);
            float maxX = Mathf.Max(x1, x2);
            float minY = Mathf.Min(y1, y2);
            float maxY = Mathf.Max(y1, y2);
            float minZ = Mathf.Min(z1, z2);
            float maxZ = Mathf.Max(z1, z2);

            Vector3 position = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
            Vector3 scale = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            Vector3 rotation = Vector3.zero;
            AddCommon(
                id: t[1],
                type: "Cube",
                material: "Concrete",
                position: position,
                scale: scale,
                rotation: rotation,
                r: 0f,
                rX: 0f,
                rZ: 0f,
                height: 0f
            );
        }
        else if (line.StartsWith("RCC"))
        {
            if (t.Length < 9)
                return result;
            if (!TryGet(t[2], out float x) ||
                !TryGet(t[3], out float y) ||
                !TryGet(t[4], out float z) ||
                !TryGet(t[5], out float dx) ||
                !TryGet(t[6], out float dy) ||
                !TryGet(t[7], out float dz) ||
                !TryGet(t[8], out float radius))
                return result;

            Vector3 axis = new Vector3(dx, dy, dz);
            float height = axis.magnitude;
            Vector3 position = new Vector3(x, y, z) + axis * 0.5f;
            Vector3 scale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Vector3 rotation = axis.sqrMagnitude > 0f
                ? Quaternion.FromToRotation(Vector3.up, axis.normalized).eulerAngles
                : Vector3.zero;
            AddCommon(
                id: t[1],
                type: "Cylinder",
                material: "Concrete",
                position: position,
                scale: scale,
                rotation: rotation,
                r: 0f,
                rX: radius,
                rZ: radius,
                height: height
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
