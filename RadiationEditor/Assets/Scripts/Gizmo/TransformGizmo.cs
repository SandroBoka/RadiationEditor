using UnityEngine;
using UnityEngine.EventSystems;

public class TransformGizmo : MonoBehaviour
{
    public Camera cam;

    Transform target;

    bool dragging;
    GizmoAxis activeAxis;

    Vector3 startTargetPos;
    Vector3 startHitPointWorld;

    public void SetTarget(Transform t)
    {
        target = t;
        gameObject.SetActive(target != null);
        dragging = false;

        if (target != null)
            transform.position = target.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        // gizmo follows the target
        transform.position = target.position;

        // start drag
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var handle = hit.collider.GetComponent<GizmoHandle>();
                if (handle != null)
                {
                    activeAxis = handle.axis;
                    dragging = true;

                    startTargetPos = target.position;

                    // ravnina kroz target, okomita na kameru
                    Plane plane = new Plane(cam.transform.forward, startTargetPos);
                    if (plane.Raycast(ray, out float enter))
                        startHitPointWorld = ray.GetPoint(enter);
                }
            }
        }

        // drag update
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

        // end drag
        if (Input.GetMouseButtonUp(0))
            dragging = false;
    }
}
