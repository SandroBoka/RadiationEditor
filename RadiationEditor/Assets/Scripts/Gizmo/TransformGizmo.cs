using UnityEngine;
using UnityEngine.EventSystems;

public class TransformGizmo : MonoBehaviour
{
    [Header("References")]
    public Camera cam;

    [Header("Raycast")]
    public LayerMask gizmoLayer; // postavi na layer "Gizmo"

    [Header("Handles (assign in Inspector)")]
    public Transform handleX;
    public Transform handleY;
    public Transform handleZ;

    [Header("Placement")]
    public float padding = 0.15f;
    public bool scaleWithDistance = true;
    public float distanceScale = 0.08f;

    Transform target;

    bool dragging;
    GizmoAxis activeAxis;

    Vector3 startTargetPos;
    Vector3 startHitPointWorld;

    void Awake()
    {
        // Na startu NEMA targeta -> gizmo mora biti ugašen
        target = null;
        dragging = false;
        gameObject.SetActive(false);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        dragging = false;

        gameObject.SetActive(target != null);

        if (target != null)
            UpdateGizmoPlacement();
    }

    void Update()
    {
        if (!target) return;

        UpdateGizmoPlacement();

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // SAMO gizmo layer
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, gizmoLayer))
            {
                var handle = hit.collider.GetComponent<GizmoHandle>();
                if (handle != null)
                {
                    activeAxis = handle.axis;
                    dragging = true;

                    startTargetPos = target.position;

                    Plane plane = new Plane(cam.transform.forward, startTargetPos);
                    if (plane.Raycast(ray, out float enter))
                        startHitPointWorld = ray.GetPoint(enter);
                }
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(cam.transform.forward, startTargetPos);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 currentHit = ray.GetPoint(enter);
                Vector3 delta = currentHit - startHitPointWorld;

                Vector3 axisDir = activeAxis switch
                {
                    GizmoAxis.X => Vector3.right,
                    GizmoAxis.Y => Vector3.up,
                    _ => Vector3.forward
                };

                float amount = Vector3.Dot(delta, axisDir);
                target.position = startTargetPos + axisDir * amount;
            }
        }

        if (Input.GetMouseButtonUp(0))
            dragging = false;
    }

    void UpdateGizmoPlacement()
    {
        Bounds b = GetTargetBounds();
        Vector3 e = b.extents;

        transform.position = b.center;

        if (handleX != null) handleX.position = b.center + Vector3.right * (e.x + padding);
        if (handleY != null) handleY.position = b.center + Vector3.up * (e.y + padding);
        if (handleZ != null) handleZ.position = b.center + Vector3.forward * (e.z + padding);

        if (scaleWithDistance && cam != null)
        {
            float d = Vector3.Distance(cam.transform.position, b.center);
            float s = Mathf.Max(0.01f, d * distanceScale);
            transform.localScale = Vector3.one * s;
        }
    }

    Bounds GetTargetBounds()
    {
        var renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(target.position, Vector3.one);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        return b;
    }
}
