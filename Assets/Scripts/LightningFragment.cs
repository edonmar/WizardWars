using UnityEngine;

public class LightningFragment : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;
    public Color color;
 
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetLineRendererColor();
    }

    private void Update()
    {
        DrawLine();
    }

    private void SetLineRendererColor()
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void DrawLine()
    {
        Vector3[] points = new Vector3[2];
        points[0] = startTransform.position;
        points[1] = endTransform.position;
        lineRenderer.SetPositions(points);
    }
}