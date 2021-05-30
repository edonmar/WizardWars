using UnityEngine;

public class ThunderBolt : MonoBehaviour
{
    public Transform targetTransform;
    public Color color;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetLineRendererColor();
        DrawLine();
    }

    private void SetLineRendererColor()
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void DrawLine()
    {
        Vector3 targetPos = targetTransform.position;
        Vector3[] points = new Vector3[2];
        points[0] = targetPos + new Vector3(0, 20, 0);
        points[1] = targetPos;
        lineRenderer.SetPositions(points);
    }
}