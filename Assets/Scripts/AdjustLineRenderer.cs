using System.Collections;
using UnityEngine;

public class AdjustLineRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private string mainTexture;
    private Vector2 mainTextureScale;
    private Vector2 mainTextureOffset;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainTexture = "_MainTex";
        mainTextureScale = Vector2.one;
        mainTextureOffset = Vector2.one;

        StartCoroutine(AddOffset(0.1f, 0.025f));
    }

    private IEnumerator AddOffset(float quantity, float interval)
    {
        while (true)
        {
            // Calculo la escala para que la textura siempre tenga el mismo tamaño, y no se estire dependiendo de
            // la longitud del LineRenderer
            float distance = Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1));
            mainTextureScale.x = (distance / 2) / 5;

            // Desplazo la textura para darle sensación de movimiento
            mainTextureOffset.x -= quantity;

            foreach (Material mat in lineRenderer.materials)
            {
                mat.SetTextureScale(mainTexture, mainTextureScale);
                mat.SetTextureOffset(mainTexture, mainTextureOffset);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}