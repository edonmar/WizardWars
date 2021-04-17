using UnityEngine;

public class LightningFragment : MonoBehaviour
{
    public GameObject lightning;
    public Lightning lightningScript;

    public Transform startTransform;
    public Transform endTransform;
    public Color color;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lightningScript = lightning.GetComponent<Lightning>();
        lineRenderer = GetComponent<LineRenderer>();
        SetLineRendererColor();
    }

    private void Update()
    {
        if (startTransform == null || endTransform == null)
            lightningScript.ResetLightningChain();
        else
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