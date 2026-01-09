using UnityEngine;
using UnityEngine.EventSystems;

public class HudSpawner : MonoBehaviour
{
    public Camera cam;
    public TransformHud hud;

    public float spawnOffset = 0.05f; // to not be in the ground

    public void SpawnCube() => Spawn(ShapeType.Cube);
    public void SpawnSphere() => Spawn(ShapeType.Sphere);
    public void SpawnCylinder() => Spawn(ShapeType.Cylinder);
    public void SpawnSensor() => Spawn(ShapeType.Sensor);

    void Spawn(ShapeType type)
    {
        var lib = ShapeManager.I.materialLibrary;
        Material mat = null;

        if (lib != null && lib.materials != null && lib.materials.Length > 0)
        {
            int idx = hud.materialDropdown.value;
            if (idx >= 0 && idx < lib.materials.Length)
                mat = lib.materials[idx];
        }

        Vector3 pos = cam.transform.position + cam.transform.forward * 5f;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            pos = hit.point + Vector3.up * spawnOffset;
        }

        ShapeManager.I.CreateShape(type, pos, mat);
    }
}
