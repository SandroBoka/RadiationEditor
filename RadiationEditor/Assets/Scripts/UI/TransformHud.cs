using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransformHud : MonoBehaviour
{
    [Header("Panels")]
    public GameObject selectionPanel;

[Header("Scale")]
public TMP_InputField scaleX;
public TMP_InputField scaleY;
public TMP_InputField scaleZ;

[Header("Rotation")]
public TMP_InputField rotX;
public TMP_InputField rotY;
public TMP_InputField rotZ;

    [Header("Materials")]
    public TMP_Dropdown materialDropdown;

    ShapeData target;

    void Start()
    {
        // kad korisnik završi unos
        scaleX.onEndEdit.AddListener(_ => ApplyTransform());
        scaleY.onEndEdit.AddListener(_ => ApplyTransform());
        scaleZ.onEndEdit.AddListener(_ => ApplyTransform());

        rotX.onEndEdit.AddListener(_ => ApplyTransform());
        rotY.onEndEdit.AddListener(_ => ApplyTransform());
        rotZ.onEndEdit.AddListener(_ => ApplyTransform());

        materialDropdown.onValueChanged.AddListener(_ => ApplyMaterial());

        FillMaterials();
        SetTarget(null);
    }

    void FillMaterials()
    {
        materialDropdown.ClearOptions();

        var lib = ShapeManager.I.materialLibrary;
        var options = new System.Collections.Generic.List<string>();

        if (lib != null && lib.materials != null)
        {
            foreach (var m in lib.materials)
                options.Add(m != null ? m.name : "(null)");
        }

        materialDropdown.AddOptions(options);
    }

    public void SetTarget(ShapeData data)
    {
        target = data;
        if (selectionPanel != null)
            selectionPanel.SetActive(target != null);

        if (!target) return;

        var s = target.transform.localScale;
        scaleX.text = s.x.ToString("0.###");
        scaleY.text = s.y.ToString("0.###");
        scaleZ.text = s.z.ToString("0.###");

        var e = target.transform.eulerAngles;
        rotX.text = e.x.ToString("0.###");
        rotY.text = e.y.ToString("0.###");
        rotZ.text = e.z.ToString("0.###");

        // pokušaj postaviti dropdown na materijal koji ima objekt
        var lib = ShapeManager.I.materialLibrary;
        if (lib != null)
        {
            for (int i = 0; i < lib.materials.Length; i++)
            {
                if (lib.materials[i] != null && lib.materials[i].name == target.materialName)
                {
                    materialDropdown.value = i;
                    break;
                }
            }
        }
    }

    void ApplyTransform()
    {
        if (!target) return;

        if (float.TryParse(scaleX.text, out float sx) &&
            float.TryParse(scaleY.text, out float sy) &&
            float.TryParse(scaleZ.text, out float sz))
        {
            target.transform.localScale = new Vector3(sx, sy, sz);
        }

        if (float.TryParse(rotX.text, out float rx) &&
            float.TryParse(rotY.text, out float ry) &&
            float.TryParse(rotZ.text, out float rz))
        {
            target.transform.rotation = Quaternion.Euler(rx, ry, rz);
        }

        target.RecomputeDerived();
    }

    void ApplyMaterial()
    {
        if (!target) return;

        var lib = ShapeManager.I.materialLibrary;
        if (lib == null || lib.materials == null) return;

        int i = materialDropdown.value;
        if (i < 0 || i >= lib.materials.Length) return;

        var mat = lib.materials[i];
        var r = target.GetComponent<Renderer>();
        if (r != null && mat != null)
        {
            r.material = mat;
            target.materialName = mat.name;
        }
    }
}
