using UnityEngine;

public class ShapeData : MonoBehaviour
{
    public ShapeType type;
    public string materialName;

    // Sphere / Sensor
    public float radius;

    // Cylinder
    public float radiusX;
    public float radiusZ;
    public float height;

    public void RecomputeDerived()
    {
        Vector3 s = transform.lossyScale;

        // reset
        radius = 0f;
        radiusX = 0f;
        radiusZ = 0f;
        height = 0f;

        switch (type)
        {
            case ShapeType.Sphere:
            case ShapeType.Sensor:
                // Unity sphere default radius = 0.5
                radius = 0.5f * s.x;
                break;

            case ShapeType.Cylinder:
                // Unity cylinder:
                // radius = 0.5, height = 2
                radiusX = 0.5f * s.x;
                radiusZ = 0.5f * s.z;
                height  = 2f * s.y;
                break;
        }
    }
}
