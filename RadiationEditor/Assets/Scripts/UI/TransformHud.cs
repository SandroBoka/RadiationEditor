using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransformHud : MonoBehaviour
{
    [Header("Panels")]
    public GameObject selectionPanel;

    [Header("Position")]
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;

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
        // Position listeners
        posX.onEndEdit.AddListener(_ => ApplyPosition());
        posY.onEndEdit.AddListener(_ => ApplyPosition());
        posZ.onEndEdit.AddListener(_ => ApplyPosition());

        // Scale listeners
        scaleX.onEndEdit.AddListener(_ => ApplyTransform());
        scaleY.onEndEdit.AddListener(_ => ApplyTransform());
        scaleZ.onEndEdit.AddListener(_ => ApplyTransform());

        // Rotation listeners
        rotX.onEndEdit.AddListener(_ => ApplyTransform());
        rotY.onEndEdit.AddListener(_ => ApplyTransform());
        rotZ.onEndEdit.AddListener(_ => ApplyTransform());

        // Material listener
        materialDropdown.onValueChanged.AddListener(_ => ApplyMaterial());

        FillMaterials();
        SetTarget(null);
    }

void Update()
{
    if (!target) return;

    // Ako korisnik trenutno tipka u bilo koje polje, ne prepisuj UI
    bool editingPosition =
        (posX != null && posX.isFocused) ||
        (posY != null && posY.isFocused) ||
        (posZ != null && posZ.isFocused);

    bool editingScale =
        (scaleX != null && scaleX.isFocused) ||
        (scaleY != null && scaleY.isFocused) ||
        (scaleZ != null && scaleZ.isFocused);

    bool editingRotation =
        (rotX != null && rotX.isFocused) ||
        (rotY != null && rotY.isFocused) ||
        (rotZ != null && rotZ.isFocused);

    if (!editingPosition)
    {
        Vector3 p = target.transform.position;
        posX.text = p.x.ToString("0.###");
        posY.text = p.y.ToString("0.###");
        posZ.text = p.z.ToString("0.###");
    }

    if (!editingScale)
    {
        Vector3 s = target.transform.localScale;
        scaleX.text = s.x.ToString("0.###");
        scaleY.text = s.y.ToString("0.###");
        scaleZ.text = s.z.ToString("0.###");
    }

    if (!editingRotation)
    {
        Vector3 e = target.transform.eulerAngles;
        rotX.text = e.x.ToString("0.###");
        rotY.text = e.y.ToString("0.###");
        rotZ.text = e.z.ToString("0.###");
    }
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

        // Position
        var p = target.transform.position;
        posX.text = p.x.ToString("0.###");
        posY.text = p.y.ToString("0.###");
        posZ.text = p.z.ToString("0.###");

        // Scale
        var s = target.transform.localScale;
        scaleX.text = s.x.ToString("0.###");
        scaleY.text = s.y.ToString("0.###");
        scaleZ.text = s.z.ToString("0.###");

        // Rotation
        var e = target.transform.eulerAngles;
        rotX.text = e.x.ToString("0.###");
        rotY.text = e.y.ToString("0.###");
        rotZ.text = e.z.ToString("0.###");

        // Set dropdown to current material
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

    public void DeleteSelected()
    {
        if (!target) return;

        ShapeData toDelete = target;

        ShapeManager.I.shapes.Remove(toDelete);

        Destroy(toDelete.gameObject);

        FindObjectOfType<SelectionManager>()?.ClearSelection();

        if (selectionPanel != null)
            selectionPanel.SetActive(false);
    }

    void ApplyPosition()
    {
        if (!target) return;

        if (float.TryParse(posX.text, out float px) &&
            float.TryParse(posY.text, out float py) &&
            float.TryParse(posZ.text, out float pz))
        {
            target.transform.position = new Vector3(px, py, pz);
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
