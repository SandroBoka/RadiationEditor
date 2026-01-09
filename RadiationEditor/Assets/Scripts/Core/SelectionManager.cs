using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public Camera cam;
    public TransformGizmo gizmo;
    public TransformHud hud;

    public ShapeData Selected { get; private set; }
    public LayerMask selectableLayers = ~0; // default: sve

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI click should not select
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // if RMB then we do not select
            if (Input.GetMouseButton(1))
                return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayers))
            {
                var data = hit.collider.GetComponentInParent<ShapeData>();
                Select(data);
            }
            else
            {
                Select(null);
            }
        }
    }

    void Select(ShapeData data)
    {
        Selected = data;
        gizmo.SetTarget(data ? data.transform : null);
        hud.SetTarget(data);
    }
}
