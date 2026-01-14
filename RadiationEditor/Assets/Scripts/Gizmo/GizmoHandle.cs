using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GizmoHandle : MonoBehaviour
{
    public GizmoAxis axis;

    static Mesh arrowMeshX;
    static Mesh arrowMeshY;
    static Mesh arrowMeshZ;

    static Material matX;
    static Material matY;
    static Material matZ;

    void Awake()
    {
        ApplyVisuals();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        ApplyVisuals();
    }
#endif

    void ApplyVisuals()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter != null)
            meshFilter.sharedMesh = GetArrowMesh(axis);

        if (meshRenderer != null)
            meshRenderer.sharedMaterial = GetAxisMaterial(axis);
    }

    static Mesh GetArrowMesh(GizmoAxis axis)
    {
        switch (axis)
        {
            case GizmoAxis.X:
                return arrowMeshX ??= BuildArrowMesh(GizmoAxis.X);
            case GizmoAxis.Y:
                return arrowMeshY ??= BuildArrowMesh(GizmoAxis.Y);
            default:
                return arrowMeshZ ??= BuildArrowMesh(GizmoAxis.Z);
        }
    }

    static Material GetAxisMaterial(GizmoAxis axis)
    {
        switch (axis)
        {
            case GizmoAxis.X:
                return matX ??= CreateMaterial(new Color(0.9f, 0.2f, 0.2f, 1f));
            case GizmoAxis.Y:
                return matY ??= CreateMaterial(new Color(0.2f, 0.9f, 0.2f, 1f));
            default:
                return matZ ??= CreateMaterial(new Color(0.2f, 0.4f, 0.95f, 1f));
        }
    }

    static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
            shader = Shader.Find("Standard");

        var mat = new Material(shader)
        {
            color = color,
            hideFlags = HideFlags.HideAndDontSave
        };
        return mat;
    }

    static Mesh BuildArrowMesh(GizmoAxis axis)
    {
        const float minX = -0.5f;
        const float maxX = 0.5f;
        const float headStart = 0.2f;
        const float half = 0.35f;

        var vertices = new List<Vector3>(48);
        var triangles = new List<int>(72);

        Vector3 v0 = AxisVertex(axis, minX, -half, -half);
        Vector3 v1 = AxisVertex(axis, minX, -half, half);
        Vector3 v2 = AxisVertex(axis, minX, half, half);
        Vector3 v3 = AxisVertex(axis, minX, half, -half);

        Vector3 v4 = AxisVertex(axis, headStart, -half, -half);
        Vector3 v5 = AxisVertex(axis, headStart, -half, half);
        Vector3 v6 = AxisVertex(axis, headStart, half, half);
        Vector3 v7 = AxisVertex(axis, headStart, half, -half);

        Vector3 tip = AxisVertex(axis, maxX, 0f, 0f);

        AddQuad(vertices, triangles, v0, v1, v2, v3); // back (-axis)
        AddQuad(vertices, triangles, v4, v7, v6, v5); // front (+axis)
        AddQuad(vertices, triangles, v3, v2, v6, v7); // top (+Y)
        AddQuad(vertices, triangles, v0, v4, v5, v1); // bottom (-Y)
        AddQuad(vertices, triangles, v1, v5, v6, v2); // front (+Z)
        AddQuad(vertices, triangles, v0, v3, v7, v4); // back (-Z)

        AddTriangle(vertices, triangles, v5, v4, tip); // bottom side
        AddTriangle(vertices, triangles, v7, v6, tip); // top side
        AddTriangle(vertices, triangles, v6, v5, tip); // front side
        AddTriangle(vertices, triangles, v4, v7, tip); // back side

        var mesh = new Mesh
        {
            name = $"GizmoArrow_{axis}",
            hideFlags = HideFlags.HideAndDontSave
        };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static Vector3 AxisVertex(GizmoAxis axis, float x, float y, float z)
    {
        return axis switch
        {
            GizmoAxis.X => new Vector3(x, y, z),
            GizmoAxis.Y => new Vector3(-y, x, z),
            _ => new Vector3(-z, y, x)
        };
    }

    static void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int start = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        triangles.Add(start);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
    }

    static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        int start = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(start);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
    }
}
