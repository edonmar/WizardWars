using System.Collections;
using UnityEngine;

public class LineRendererOffset : MonoBehaviour
{
    private LineRenderer lineRednerer;
    private string mainTexture;
    private Vector2 mainTextureOffset;

    private void Start()
    {
        lineRednerer = GetComponent<LineRenderer>();
        mainTexture = "_MainTex";
        mainTextureOffset = Vector2.one;

        StartCoroutine(AddOffset(0.1f, 0.025f));
    }

    private IEnumerator AddOffset(float quantity, float interval)
    {
        float offset = 0;
        while (true)
        {
            offset -= quantity;
            mainTextureOffset.x = offset;
            foreach (Material mat in lineRednerer.materials)
                mat.SetTextureOffset(mainTexture, mainTextureOffset);

            yield return new WaitForSeconds(interval);
        }
    }
}