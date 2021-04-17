using UnityEngine;

public class LightningFragment : MonoBehaviour
{
    public GameObject lightningManager;
    public LightningManager lightningManagerScript;

    public Transform startTransform;
    public Transform endTransform;
    public Color color;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lightningManagerScript = lightningManager.GetComponent<LightningManager>();
        lineRenderer = GetComponent<LineRenderer>();
        SetLineRendererColor();
    }

    private void Update()
    {
        if (startTransform == null || endTransform == null)
            lightningManagerScript.ResetLightningChain();
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