using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public LayerMask gizmoLayer;
    public LayerMask selectableLayers;

    public Camera cam;
    public TransformGizmo gizmo;
    public TransformHud hud;

    public ShapeData Selected { get; private set; }

    void Awake()
    {
        Select(null); // na startu ništa nije selektirano
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(1))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // 1) klik na gizmo -> NE MIJENJAJ selekciju
        if (Physics.Raycast(ray, out _, 1000f, gizmoLayer))
            return;

        // 2) selekcija shapea
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

    void Select(ShapeData data)
    {
        Selected = data;
        gizmo.SetTarget(data ? data.transform : null);
        hud.SetTarget(data);
    }
}
