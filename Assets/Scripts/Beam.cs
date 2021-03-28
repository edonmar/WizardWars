using UnityEngine;

public class Beam : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, transform.position);
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            if (hit.collider)
                lineRenderer.SetPosition(1, hit.point);
        }
        else
            lineRenderer.SetPosition(1, transform.forward * 5000);
    }
}