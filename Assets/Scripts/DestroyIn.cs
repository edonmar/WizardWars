using System.Collections;
using UnityEngine;

public class DestroyIn : MonoBehaviour
{
    public float duration;

    private void Start()
    {
        StartCoroutine(DestroyInMethod());
    }

    private IEnumerator DestroyInMethod()
    {
        yield return new WaitForSeconds(duration);

        switch (gameObject.tag)
        {
            case "Wall":
                gameObject.GetComponent<Wall>().DestroyThis();
                break;
            case "WallAura":
                gameObject.GetComponent<WallAura>().DestroyThis();
                break;
            case "Storm":
                gameObject.GetComponent<Storm>().DestroyThis();
                break;
            case "Nova":
                gameObject.GetComponent<Nova>().DestroyThis();
                break;
            case "Beam":
                gameObject.GetComponent<Beam>().DestroyThis();
                break;
            case "Lightning":
                gameObject.GetComponent<LightningManager>().DestroyThis();
                break;
            case "Spray":
                gameObject.GetComponent<Spray>().DestroyThis();
                break;
            default:
                Destroy(gameObject);
                break;
        }
    }
}