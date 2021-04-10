using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightningManager : MonoBehaviour
{
    [SerializeField] private GameObject lightningFragmentPrefab;

    public Dictionary<string, int> elements;
    public GameObject caster;
    public float size;
    public Color color;

    private List<GameObject> chainedCharacters; // Personajes por los que pasará el rayo, siendo el primero el lanzador
    // del hechizo y el último el último enemigo golpeado por el rayo

    private void Start()
    {
        GetChainedCharacters();
        if (chainedCharacters.Count > 1)
            CreateLightningFragments();
    }

    // Obtiene todos los personajes por los que pasará el rayo
    private void GetChainedCharacters()
    {
        chainedCharacters = new List<GameObject> {caster};
        int pos = 0;

        do
        {
            GameObject nearestCharacter = GetNearestCharacter(chainedCharacters[pos].transform.position);
            if (nearestCharacter == null)
                break;

            chainedCharacters.Add(nearestCharacter);
            pos++;
        } while (true);
    }

    private GameObject GetNearestCharacter(Vector3 characterPos)
    {
        // Obtiene una lista con los personajes dentro del radio pasado como parámetro
        List<GameObject> nearCharacters = NearCharacters(characterPos, size);
        // Elimina de la lista los personajes por los que ya haya pasado el rayo
        nearCharacters = nearCharacters.Except(chainedCharacters).ToList();
        // Devuelve el personaje más cercano de los que queden sin golpear por el rayo
        GameObject nearestCharacter = nearCharacters.Count == 0
            ? null
            : NearestCharacter(nearCharacters, characterPos);
        return nearestCharacter;
    }

    private List<GameObject> NearCharacters(Vector3 center, float radius)
    {
        List<GameObject> nearCharacters = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

        foreach (Collider hitCollider in hitColliders)
        {
            GameObject hitObject = hitCollider.gameObject;
            switch (hitObject.tag)
            {
                case "Player":
                case "Enemy":
                    nearCharacters.Add(hitObject);
                    break;
            }
        }

        return nearCharacters;
    }

    private GameObject NearestCharacter(List<GameObject> nearCharacters, Vector3 characterPos)
    {
        return nearCharacters
            .OrderBy(c => (c.transform.position - characterPos).sqrMagnitude)
            .FirstOrDefault();
    }

    // Crea un LightningFragment para cada dos personajes que estén unidos por el rayo
    private void CreateLightningFragments()
    {
        for (int i = 0; i < chainedCharacters.Count - 1; i++)
            InstantiateLightningFragment(chainedCharacters[i].transform, chainedCharacters[i + 1].transform);
    }

    private void InstantiateLightningFragment(Transform startTransform, Transform endTransform)
    {
        GameObject lightningFragment = Instantiate(lightningFragmentPrefab, gameObject.transform, true);
        LightningFragment lightningFragmentScript = lightningFragment.GetComponent<LightningFragment>();
        lightningFragmentScript.startTransform = startTransform;
        lightningFragmentScript.endTransform = endTransform;
        lightningFragmentScript.color = color;
    }
}