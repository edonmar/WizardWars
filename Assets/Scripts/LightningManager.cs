using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightningManager : MonoBehaviour
{
    [SerializeField] private GameObject lightningFragmentPrefab;

    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    public GameObject caster;
    public float size;
    public Color color;

    private List<GameObject> chainedCharacters; // Personajes por los que pasará el rayo, siendo el primero el lanzador
    // del hechizo y el último el último enemigo golpeado por el rayo
    private List<GameObject> lightningFragments; // LineRenderer que formarán los fragmentos de la cadena de relámpagos

    private Vector3 frontPosition; // Punto un metro delante del lanzador del hechizo
    private Vector3 backPosition;  // Punto un metro detrás del lanzador del hechizo

    private void Start()
    {
        GetFrontAndBackPositions();
        CreateLightningChain();
        dmgTypes = GetDamageTypesDictionary();
        StartCoroutine(HitTimer(0.25f));
    }

    private void GetFrontAndBackPositions()
    {
        Transform thisTransform = transform;
        Vector3 thisPosition = thisTransform.position;
        Vector3 thisForward = thisTransform.forward;

        frontPosition = thisPosition + thisForward;
        backPosition = thisPosition - thisForward;
    }

    private void CreateLightningChain()
    {
        GetChainedCharacters();
        if (chainedCharacters.Count > 1)
            CreateLightningFragments();
    }

    private Dictionary<string, int> GetDamageTypesDictionary()
    {
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int>();

        int waterCount = 0;
        int coldCount = 0;
        int lightningCount = 0;
        int fireCount = 0;

        if (elements.ContainsKey("WAT"))
            waterCount = elements["WAT"];
        if (elements.ContainsKey("COL"))
            coldCount = elements["COL"];
        if (elements.ContainsKey("LIG"))
            lightningCount = elements["LIG"];
        if (elements.ContainsKey("FIR"))
            fireCount = elements["FIR"];

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 32 + 10 * waterCount);
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 0);
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 32 + 10 * lightningCount);
        if (fireCount > 0)
            dmgTypesDict.Add("FIR", 0);

        return dmgTypesDict;
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
        List<GameObject> nearCharacters = GetNearCharacters(characterPos, size);

        // Si es el primer fragmento de la cadena, elimina de la lista los personajes que NO estén enfrente
        // del lanzador del hechizo
        if (chainedCharacters.Count == 1)
            nearCharacters = GetNearCharactersInFrontOfCaster(nearCharacters);

        // Elimina de la lista los personajes por los que ya haya pasado el rayo
        nearCharacters = nearCharacters.Except(chainedCharacters).ToList();

        // Devuelve el personaje más cercano de los que queden sin golpear por el rayo
        GameObject nearestCharacter = nearCharacters.Count == 0
            ? null
            : NearestCharacter(nearCharacters, characterPos);
        return nearestCharacter;
    }

    private List<GameObject> GetNearCharacters(Vector3 center, float radius)
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

    private List<GameObject> GetNearCharactersInFrontOfCaster(List<GameObject> nearCharacters)
    {
        return (from c in nearCharacters
            let characterPosition = c.transform.position
            where Vector3.Distance(characterPosition, frontPosition) <=
                  Vector3.Distance(characterPosition, backPosition)
            select c).ToList();
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
        lightningFragments = new List<GameObject>();
        for (int i = 0; i < chainedCharacters.Count - 1; i++)
            lightningFragments.Add(InstantiateLightningFragment(chainedCharacters[i].transform,
                chainedCharacters[i + 1].transform));
    }

    private GameObject InstantiateLightningFragment(Transform startTransform, Transform endTransform)
    {
        GameObject lightningFragment = Instantiate(lightningFragmentPrefab, gameObject.transform, true);
        LightningFragment lightningFragmentScript = lightningFragment.GetComponent<LightningFragment>();
        lightningFragmentScript.lightningManager = gameObject;
        lightningFragmentScript.startTransform = startTransform;
        lightningFragmentScript.endTransform = endTransform;
        lightningFragmentScript.color = color;
        return lightningFragment;
    }

    // Si uno de los personajes por los que pasaba la cadena ha muerto, hago que la cadena de relámpagos vuelva a
    // calcular desde el principio por qué personajes debe pasar
    public void ResetLightningChain()
    {
        foreach (GameObject fragment in lightningFragments)
            Destroy(fragment);
        lightningFragments = new List<GameObject>();
        CreateLightningChain();
    }

    private void Hit()
    {
        for (int i = 1; i < chainedCharacters.Count; i++)
        {
            CharacterStats characterStats = chainedCharacters[i].GetComponent<CharacterStats>();
            if (characterStats.health != 0)
                characterStats.TakeSpell(dmgTypes);
        }
    }

    private IEnumerator HitTimer(float hitRate)
    {
        while (true)
        {
            Hit();
            yield return new WaitForSeconds(hitRate);
        }
    }
}