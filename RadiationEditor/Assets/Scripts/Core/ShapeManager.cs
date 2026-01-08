using System.Collections.Generic;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    public static ShapeManager I;

    public MaterialLibrary materialLibrary;
    public List<ShapeData> shapes = new();

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
    }

    public ShapeData CreateShape(ShapeType type, Vector3 position, Material mat)
    {
        GameObject go;

        switch (type)
        {
            case ShapeType.Cube:
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case ShapeType.Sphere:
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case ShapeType.Cylinder:
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            default:
                // sensor kao mala sfera
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.localScale = Vector3.one * 0.2f;
                break;
        }

        go.name = $"{type}_{shapes.Count}";
        go.transform.position = position;

        var data = go.AddComponent<ShapeData>();
        data.type = type;

        var r = go.GetComponent<Renderer>();
        if (r != null && mat != null)
        {
            r.material = mat;
            data.materialName = mat.name;
        }
        else
        {
            data.materialName = "";
        }

        data.RecomputeDerived();
        shapes.Add(data);

        return data;
    }
}
