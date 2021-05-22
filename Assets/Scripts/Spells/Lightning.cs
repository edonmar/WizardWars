using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    [SerializeField] private GameObject lightningFragmentPrefab;

    public Dictionary<string, int> elements;
    private Dictionary<string, int> dmgTypes;
    private LayerMask layerMask;
    public GameObject caster;
    public string castType;
    public float size;
    public Color color;

    private List<GameObject> chainedCharacters; // Personajes por los que pasará el rayo, siendo el primero el lanzador

    // del hechizo y el último el último enemigo golpeado por el rayo
    private List<GameObject> lightningFragments; // LineRenderer que formarán los fragmentos de la cadena de relámpagos

    // Puntos que rodean al lanzador del hechizo, un metro delante, detrás, derecha e izquierda
    private Vector3 frontPosition;
    private Vector3 backPosition;
    private Vector3 rightPosition;
    private Vector3 leftPosition;

    private char direction; // Dirección en la que el lanzador del hechizo lanzará el rayo
    // (delante, derecha, detrás, izquierda)

    private void Start()
    {
        chainedCharacters = new List<GameObject>();
        lightningFragments = new List<GameObject>();
        layerMask = LayerMask.GetMask("TerrainWall", "Barrier");

        GetPositionsAroundCaster();
        // Si el tipo de lanzamiento es de área, empiezo en L para que el primer golpe sea F
        direction = castType == "FOC" ? 'F' : 'L';
        CreateLightningChain();
        dmgTypes = GetDamageTypesDictionary();
        StartCoroutine(HitTimer(0.25f));
    }

    private void GetPositionsAroundCaster()
    {
        Transform thisTransform = transform;
        Vector3 thisPosition = thisTransform.position;
        Vector3 thisForward = thisTransform.forward;
        Vector3 thisRight = thisTransform.right;

        frontPosition = thisPosition + thisForward;
        backPosition = thisPosition - thisForward;
        rightPosition = thisPosition + thisRight;
        leftPosition = thisPosition - thisRight;
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

        int waterCount = elements.ContainsKey("WAT") ? elements["WAT"] : 0;
        int coldCount = elements.ContainsKey("COL") ? elements["COL"] : 0;
        int lightningCount = elements.ContainsKey("LIG") ? elements["LIG"] : 0;
        int fireCount = elements.ContainsKey("FIR") ? elements["FIR"] : 0;

        if (waterCount > 0)
            dmgTypesDict.Add("WAT", 32 + 10 * (waterCount - 1));
        if (coldCount > 0)
            dmgTypesDict.Add("COL", 0);
        if (lightningCount > 0)
            dmgTypesDict.Add("LIG", 32 + 10 * (lightningCount - 1));
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
            GameObject nearestCharacter = GetNextCharacter(chainedCharacters[pos].transform.position);
            if (nearestCharacter == null)
                break;

            chainedCharacters.Add(nearestCharacter);
            pos++;
        } while (true);
    }

    private GameObject GetNextCharacter(Vector3 characterPos)
    {
        // Obtiene una lista con los personajes dentro del radio pasado como parámetro
        List<GameObject> nearCharacters = GetNearCharacters(characterPos, size);

        // Si es el primer fragmento de la cadena, elimina de la lista los personajes que NO estén enfrente
        // del lanzador del hechizo
        if (chainedCharacters.Count == 1)
            nearCharacters = GetNearCharactersInDirection(nearCharacters);

        // Elimina de la lista los personajes por los que ya haya pasado el rayo
        nearCharacters = nearCharacters.Except(chainedCharacters).ToList();

        // Elimina de la lista los personajes que tienen una pared entre ellos y el personaje actual
        nearCharacters = GetNonCoveredCharacters(characterPos, nearCharacters);

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

    private List<GameObject> GetNearCharactersInDirection(List<GameObject> nearCharacters)
    {
        return (from c in nearCharacters
            let characterPosition = c.transform.position
            where Vector3.Distance(characterPosition, GetPositionAtDirection()) <=
                  Vector3.Distance(characterPosition, GetPositionAtOppositeDirection())
            select c).ToList();
    }

    private List<GameObject> GetNonCoveredCharacters(Vector3 characterPos, List<GameObject> nearCharacters)
    {
        return nearCharacters.Where(c => !Physics.Linecast(characterPos, c.transform.position, layerMask)).ToList();
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
        lightningFragmentScript.lightning = gameObject;
        lightningFragmentScript.startTransform = startTransform;
        lightningFragmentScript.endTransform = endTransform;
        lightningFragmentScript.color = color;
        return lightningFragment;
    }

    // Si uno de los personajes por los que pasaba la cadena ha muerto, hago que la cadena de relámpagos vuelva a
    // calcular desde el principio por qué personajes debe pasar
    public void ResetLightningChain()
    {
        if (lightningFragments.Count > 0)
        {
            foreach (GameObject fragment in lightningFragments)
                Destroy(fragment);
        }

        lightningFragments = new List<GameObject>();
        CreateLightningChain();
    }

    private IEnumerator HitTimer(float hitRate)
    {
        while (true)
        {
            // Si el tipo de lanzamiento es enfocado, se mantiene un rayo hacia delante
            // Si es área, el rayo va girando en el sentido de las agujas del reloj
            if (castType == "ARE")
            {
                ChangeDirection();
                ResetLightningChain();
            }

            for (int i = 1; i < chainedCharacters.Count; i++)
                HitCharacter(chainedCharacters[i]);

            yield return new WaitForSeconds(hitRate);
        }
    }

    private void HitCharacter(GameObject character)
    {
        CharacterStats characterStats = character.GetComponent<CharacterStats>();
        if (!characterStats.isDead)
            characterStats.TakeSpell(dmgTypes);
    }

    private void ChangeDirection()
    {
        char newDirection = direction switch
        {
            'F' => 'R',
            'R' => 'B',
            'B' => 'L',
            'L' => 'F',
            _ => '\0'
        };

        direction = newDirection;
    }

    private Vector3 GetPositionAtDirection()
    {
        Vector3 position = direction switch
        {
            'F' => frontPosition,
            'R' => rightPosition,
            'B' => backPosition,
            'L' => leftPosition,
            _ => default
        };

        return position;
    }

    private Vector3 GetPositionAtOppositeDirection()
    {
        Vector3 position = direction switch
        {
            'F' => backPosition,
            'R' => leftPosition,
            'B' => frontPosition,
            'L' => rightPosition,
            _ => default
        };

        return position;
    }

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}