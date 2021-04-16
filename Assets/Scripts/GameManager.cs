using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public Material matWater;
    public Material matLife;
    public Material matShield;
    public Material matCold;
    public Material matLightning;
    public Material matArcane;
    public Material matEarth;
    public Material matFire;
    public Material matIce;
    public Material matSteam;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallAuraPrefab;
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject stormPrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject iciclePrefab;
    [SerializeField] private GameObject novaPrefab;
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject sprayPrefab;

    public IEnumerator CastWalls(Dictionary<string, int> elements, string castType,
        Transform originTransform)
    {
        Vector3 originPos = originTransform.position;
        Vector3 originRot = originTransform.localEulerAngles;
        float radius;
        float duration;
        int numberOfObjects;
        int numOfIterations;
        bool evenNumberOfObjects;
        int countEarth = 0;
        int countIce = 0;

        if (elements.ContainsKey("EAR"))
            countEarth = elements["EAR"];
        if (elements.ContainsKey("ICE"))
            countIce = elements["ICE"];

        if (countEarth != 0)
            duration = 20 + 10 * (countEarth - 1);
        else
            duration = countIce;

        switch (castType)
        {
            case "FOC":
                radius = 2f;
                numberOfObjects = 4;

                evenNumberOfObjects = numberOfObjects % 2 == 0;
                numOfIterations = numberOfObjects / 2;
                if (!evenNumberOfObjects)
                    numOfIterations++;

                for (int i = 0; i < numOfIterations; i++)
                {
                    GameObject newObj = InstantiateObjCircularly(wallPrefab, "arc", false, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewWall(newObj);

                    if (i == 0 && !evenNumberOfObjects)
                        continue;

                    newObj = InstantiateObjCircularly(wallPrefab, "arc", true, originPos, originRot,
                        numberOfObjects, radius, i);
                    InitialStepsNewWall(newObj);
                }

                break;

            case "ARE":
                radius = 2f;
                numberOfObjects = 10;
                float secondsToWait = 0.5f / numberOfObjects;

                evenNumberOfObjects = numberOfObjects % 2 == 0;
                numOfIterations = numberOfObjects / 2 + 1;

                for (int i = 0; i < numOfIterations; i++)
                {
                    GameObject newObj = InstantiateObjCircularly(wallPrefab, "circle", false, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewWall(newObj);

                    if (i == 0 || evenNumberOfObjects && i == numOfIterations - 1)
                        continue;

                    newObj = InstantiateObjCircularly(wallPrefab, "circle", true, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewWall(newObj);

                    yield return new WaitForSeconds(secondsToWait);
                }

                break;
        }

        void InitialStepsNewWall(GameObject newObj)
        {
            newObj.GetComponent<DestroyIn>().duration = duration;

            // Le paso al wall los elementos que tendrá
            Dictionary<string, int> subDictElements =
                elements.Where(e => e.Key != "SHI")
                    .ToDictionary(e => e.Key, e => e.Value);
            newObj.GetComponent<Wall>().elements = subDictElements;

            ApplyMaterialWall(newObj, elements);
        }
    }

    public IEnumerator CastMines(Dictionary<string, int> elements, string castType,
        Transform originTransform)
    {
        Vector3 originPos = originTransform.position;
        Vector3 originRot = originTransform.localEulerAngles;
        float radius;
        int numberOfObjects;
        int numOfIterations;
        bool evenNumberOfObjects;

        switch (castType)
        {
            case "FOC":
                radius = 2f;
                numberOfObjects = 4;

                evenNumberOfObjects = numberOfObjects % 2 == 0;
                numOfIterations = numberOfObjects / 2;
                if (!evenNumberOfObjects)
                    numOfIterations++;

                for (int i = 0; i < numOfIterations; i++)
                {
                    GameObject newObj = InstantiateObjCircularly(minePrefab, "arc", false, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewMine(newObj);

                    if (i == 0 && !evenNumberOfObjects)
                        continue;

                    newObj = InstantiateObjCircularly(minePrefab, "arc", true, originPos, originRot,
                        numberOfObjects, radius, i);
                    InitialStepsNewMine(newObj);
                }

                break;

            case "ARE":
                radius = 2f;
                numberOfObjects = 12;
                float secondsToWait = 0.5f / numberOfObjects;

                evenNumberOfObjects = numberOfObjects % 2 == 0;
                numOfIterations = numberOfObjects / 2 + 1;

                for (int i = 0; i < numOfIterations; i++)
                {
                    GameObject newObj = InstantiateObjCircularly(minePrefab, "circle", false, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewMine(newObj);

                    if (i == 0 || evenNumberOfObjects && i == numOfIterations - 1)
                        continue;

                    newObj = InstantiateObjCircularly(minePrefab, "circle", true, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewMine(newObj);

                    yield return new WaitForSeconds(secondsToWait);
                }

                break;
        }

        void InitialStepsNewMine(GameObject newObj)
        {
            // Le paso a la mina los elementos con los que explotará
            Dictionary<string, int> subDictElements =
                elements.Where(e => e.Key != "SHI")
                    .ToDictionary(e => e.Key, e => e.Value);
            newObj.GetComponent<Mine>().elements = subDictElements;

            ApplyMaterialMine(newObj, elements);
        }
    }

    public IEnumerator CastStorms(Dictionary<string, int> elements, string castType,
        Transform originTransform)
    {
        Vector3 originPos = originTransform.position;
        Vector3 originRot = originTransform.localEulerAngles;
        float radius;
        int count = elements[elements.ElementAt(1).Key];
        float duration = 4 + 2 * (count - 1);
        int numberOfObjects;
        int numOfIterations;
        bool evenNumberOfObjects;

        switch (castType)
        {
            case "FOC":
                radius = 2f;
                numberOfObjects = 6;

                evenNumberOfObjects = numberOfObjects % 2 == 0;
                numOfIterations = numberOfObjects / 2;
                if (!evenNumberOfObjects)
                    numOfIterations++;

                for (int i = 0; i < numOfIterations; i++)
                {
                    GameObject newObj = InstantiateObjCircularly(stormPrefab, "arc", false, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewStorm(newObj);

                    if (i == 0 && !evenNumberOfObjects)
                        continue;

                    newObj = InstantiateObjCircularly(stormPrefab, "arc", true, originPos, originRot,
                        numberOfObjects, radius, i);
                    InitialStepsNewStorm(newObj);
                }

                break;

            case "ARE":
                radius = 2f;
                numberOfObjects = 16;
                float secondsToWait = 0.5f / numberOfObjects;

                evenNumberOfObjects = numberOfObjects % 2 == 0;
                numOfIterations = numberOfObjects / 2 + 1;

                for (int i = 0; i < numOfIterations; i++)
                {
                    GameObject newObj = InstantiateObjCircularly(stormPrefab, "circle", false, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewStorm(newObj);

                    if (i == 0 || evenNumberOfObjects && i == numOfIterations - 1)
                        continue;

                    newObj = InstantiateObjCircularly(stormPrefab, "circle", true, originPos,
                        originRot, numberOfObjects, radius, i);
                    InitialStepsNewStorm(newObj);

                    yield return new WaitForSeconds(secondsToWait);
                }

                break;
        }

        void InitialStepsNewStorm(GameObject newObj)
        {
            newObj.GetComponent<DestroyIn>().duration = duration;

            // Le paso al storm los elementos que tendrá
            Dictionary<string, int> subDictElements =
                elements.Where(e => e.Key != "SHI")
                    .ToDictionary(e => e.Key, e => e.Value);
            newObj.GetComponent<Storm>().elements = subDictElements;

            ApplyMaterialStorm(newObj, elements);
        }
    }

    public void CastRock(Dictionary<string, int> elements, string castType, Transform casterTransform,
        Transform casterShootPointTransform)
    {
        int countEarth = 0;
        int countIce = 0;

        if (elements.ContainsKey("EAR"))
            countEarth = elements["EAR"];
        if (elements.ContainsKey("ICE"))
            countIce = elements["ICE"];

        int size = countEarth + countIce;
        switch (castType)
        {
            case "FOC":
                InstantiateRock(elements, casterShootPointTransform, size, 1000, castType);
                break;
            case "SEL":
                InstantiateRock(elements, casterTransform, size, 1000, castType);
                break;
        }
    }

    public void CastIcicles(Dictionary<string, int> elements, string castType, Transform casterTransform,
        Transform casterShootPointTransform)
    {
        int quantity = 3 * elements["ICE"];

        switch (castType)
        {
            case "FOC":
                for (int i = 0; i < quantity; i++)
                    InstantiateIcicleFocus(elements, casterShootPointTransform, 1000);
                break;

            case "SEL":
                for (int i = 0; i < quantity; i++)
                    InstantiateIcicleSelfCast(elements, casterTransform, 1000);
                break;
        }
    }

    public void CastNova(Dictionary<string, int> elements, Transform originTransform, string originType)
    {
        string mainElement;
        string firstElement = elements.ElementAt(0).Key;

        if (firstElement == "EAR" || firstElement == "ICE")
        {
            int countEarth = 0;
            int countIce = 0;

            if (elements.ContainsKey("EAR"))
                countEarth = elements["EAR"];
            if (elements.ContainsKey("ICE"))
                countIce = elements["ICE"];

            mainElement = countEarth > countIce ? "EAR" : "ICE";
        }
        else
            mainElement = firstElement;

        int size = elements[mainElement];

        InstantiateNova(elements, originTransform, originType, size);
    }

    public GameObject CastBeam(Dictionary<string, int> elements, Transform originTransform)
    {
        GameObject newObj = InstantiateBeam(elements, originTransform);
        return newObj;
    }

    public GameObject CastLightning(Dictionary<string, int> elements, Transform originTransform, GameObject caster)
    {
        int ligCount = elements["LIG"];
        float size = 3 + (ligCount - 1) * 1;

        GameObject newObj = InstantiateLightning(elements, originTransform, caster, size);
        return newObj;
    }

    public GameObject CastSpray(Dictionary<string, int> elements, Transform originTransform)
    {
        int size = elements.Sum(x => x.Value);
        if (elements.ContainsKey("LIG"))
            size -= elements["LIG"];

        GameObject newObj = InstantiateSpray(elements, originTransform, size);
        return newObj;
    }

    // Instancia un objeto en el círculo que rodea a otro
    private GameObject InstantiateObjCircularly(GameObject prefab, string shape, bool mirrored,
        Vector3 originPos, Vector3 originRot, int numberOfObjects, float radius, int i)
    {
        float angleCircle = 0; // La rotación del objeto alrededor del círculo
        float centerAngle = originRot.y * Mathf.PI / 180; // La rotación del objeto que hace de centro del círculo

        switch (shape)
        {
            // En forma de círculo completo
            case "circle":
                angleCircle = i * 2 * Mathf.PI / numberOfObjects;
                break;

            // En forma de arco
            case "arc":
                if (numberOfObjects % 2 == 0)
                    angleCircle = (i + 0.5f) * 3f / 4f * Mathf.PI / numberOfObjects;
                else
                    angleCircle = i * 3f / 4f * Mathf.PI / numberOfObjects;
                break;
        }

        // Si mirrored es true, invierte el ángulo
        if (mirrored)
            angleCircle *= -1;

        // Rotación del objeto + rotación del centro + reajuste
        float finalAngle = angleCircle - centerAngle + 90 * Mathf.PI / 180;

        // Posición donde se creará el objeto
        float x = Mathf.Cos(finalAngle) * radius;
        float z = Mathf.Sin(finalAngle) * radius;
        float y = prefab.name == "Mine" ? -0.5f : 0;

        Vector3 spawnPos = originPos + new Vector3(x, y, z);

        // La rotación será mirando en la dirección contraria al centro del círculo
        float angleDegrees = -finalAngle * Mathf.Rad2Deg + 90;
        Quaternion spawnRot = Quaternion.Euler(0, angleDegrees, 0);

        return Instantiate(prefab, spawnPos, spawnRot);
    }

    public GameObject InstantiateWallAura(Transform parentTransform, Dictionary<string, int> elements)
    {
        Vector3 parentPosition = parentTransform.position;
        GameObject newObj = Instantiate(wallAuraPrefab, parentPosition, parentTransform.rotation);
        newObj.transform.parent = parentTransform;

        // Si el WallAura contiene LIF o ARC, desactivo su script de destruirse automáticamente
        if (elements.ContainsKey("LIF") || elements.ContainsKey("ARC"))
            Destroy(newObj.GetComponent<DestroyIn>());
        else // Si no contiene LIF ni ARC, calculo el tiempo que tardará en destruirse y lo aplico al script
        {
            string moreRepeatedElement = elements.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            int countMoreRepeatedElement = elements[moreRepeatedElement];
            float duration = 5 * countMoreRepeatedElement;
            newObj.GetComponent<DestroyIn>().duration = duration;
        }

        // Le paso al wallAura los elementos que tendrá
        newObj.GetComponent<WallAura>().elements = elements;

        ApplyMaterialWallAura(newObj, elements);

        return newObj;
    }

    private void InstantiateRock(Dictionary<string, int> elements, Transform originTransform, float size, float force,
        string castType)
    {
        float scale = 0.25f + size * 0.1375f;
        Vector3 shootPointPosition = originTransform.position;
        Vector3 spawnPos = shootPointPosition;

        switch (castType)
        {
            case "FOC":
                spawnPos += originTransform.forward * (scale / 2);
                break;
            case "SEL":
                // Muevo la roca un poco en los ejes X y Z para evitar que se quede en equilibrio encima del jugador
                float posX;
                float posZ;

                posX = Random.Range(-0.01f, 0.01f);
                if (posX == 0)
                    posX = 0.01f;

                posZ = Random.Range(-0.01f, 0.01f);
                if (posZ == 0)
                    posZ = 0.01f;

                spawnPos += new Vector3(posX, 5, posZ);
                break;
        }

        GameObject newObj = Instantiate(rockPrefab, spawnPos, originTransform.rotation);
        newObj.transform.localScale = new Vector3(scale, scale, scale);
        Rigidbody rb = newObj.GetComponent<Rigidbody>();

        switch (castType)
        {
            case "FOC":
                rb.AddRelativeForce(Vector3.forward * force);
                break;
            case "SEL":
                rb.AddRelativeForce(Vector3.down * force);
                break;
        }

        // Le paso a la roca los elementos con los que explotará
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "EAR" && e.Key != "ICE")
                .ToDictionary(e => e.Key, e => e.Value);
        newObj.GetComponent<Rock>().elements = subDictElements;

        ApplyMaterialRock(newObj, elements);
    }

    private void InstantiateIcicleFocus(Dictionary<string, int> elements, Transform originTransform, float force)
    {
        Vector3 shootPointPosition = originTransform.position;
        Vector3 spawnPos = shootPointPosition + originTransform.forward * iciclePrefab.transform.localScale.y;
        Vector3 shootPointRotation = originTransform.eulerAngles;
        float spreadingRotation = Random.Range(-45f, 45f);
        spreadingRotation += shootPointRotation.y;
        Quaternion spawnRot = Quaternion.Euler(90, spreadingRotation, shootPointRotation.z);

        GameObject newObj = Instantiate(iciclePrefab, spawnPos, spawnRot);
        Rigidbody rb = newObj.GetComponent<Rigidbody>();
        rb.AddRelativeForce(Vector3.up * force);

        // Le paso al icicle los elementos que tendrá
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "ICE")
                .ToDictionary(e => e.Key, e => e.Value);
        newObj.GetComponent<Icicle>().elements = subDictElements;
    }

    private void InstantiateIcicleSelfCast(Dictionary<string, int> elements, Transform originTransform, float force)
    {
        Vector3 shootPointPosition = originTransform.position;
        Vector3 spawnPos = shootPointPosition + new Vector3(Random.Range(-2f, 2f), 3, Random.Range(-2f, 2f));
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        GameObject newObj = Instantiate(iciclePrefab, spawnPos, spawnRot);
        Rigidbody rb = newObj.GetComponent<Rigidbody>();
        rb.AddRelativeForce(Vector3.down * force);

        // Le paso al icicle los elementos que tendrá
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "ICE")
                .ToDictionary(e => e.Key, e => e.Value);
        newObj.GetComponent<Icicle>().elements = subDictElements;
    }

    // Si el origen de la nova es el jugador
    public void InstantiateNova(Dictionary<string, int> elements, Transform originTransform, string originType,
        int size)
    {
        float scale = size * 4f;
        Vector3 originPosition = originTransform.position;
        Vector3 spawnPos = new Vector3(originPosition.x, 0.2f, originPosition.z);
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        GameObject newObj = Instantiate(novaPrefab, spawnPos, spawnRot);
        newObj.transform.localScale = new Vector3(scale, 0.2f, scale);

        // Le paso a la nova los elementos que tendrá
        Nova nova = newObj.GetComponent<Nova>();
        nova.elements = elements;
        nova.originType = originType;
        nova.caster = originType == "character" ? originTransform.gameObject : null;

        ApplyMaterialNova(newObj, elements);
    }

    private GameObject InstantiateBeam(Dictionary<string, int> elements, Transform originTransform)
    {
        GameObject newObj = Instantiate(beamPrefab, originTransform.position, originTransform.rotation);
        newObj.transform.SetParent(originTransform);

        // Le paso al beam los elementos que tendrá
        newObj.GetComponent<Beam>().elements = elements;

        ApplyMaterialBeam(newObj, elements);

        return newObj;
    }

    private GameObject InstantiateLightning(Dictionary<string, int> elements, Transform originTransform,
        GameObject caster, float size)
    {
        GameObject newObj = Instantiate(lightningPrefab, originTransform.position, originTransform.rotation);
        newObj.transform.SetParent(originTransform);

        // Le paso al lightning los elementos que tendrá
        LightningManager lightningManagerScript = newObj.GetComponent<LightningManager>();
        lightningManagerScript.elements = elements;
        lightningManagerScript.caster = caster;
        lightningManagerScript.size = size;
        lightningManagerScript.color = GetLightningColor(elements);

        return newObj;
    }

    private GameObject InstantiateSpray(Dictionary<string, int> elements, Transform originTransform, int size)
    {
        float scale = 2 + (size - 1) * 1;
        Vector3 spawnPos = originTransform.position + originTransform.forward * (scale / 2);

        GameObject newObj = Instantiate(sprayPrefab, spawnPos, originTransform.rotation);
        Transform activeSprayTransform = newObj.transform;
        Vector3 activeSprayLocalScale = activeSprayTransform.localScale;
        activeSprayTransform.localScale = new Vector3(activeSprayLocalScale.x, activeSprayLocalScale.y, scale);
        activeSprayTransform.SetParent(originTransform);

        // Le paso al spray los elementos que tendrá
        newObj.GetComponent<Spray>().elements = elements;

        ApplyMaterialSpray(newObj, elements);

        return newObj;
    }

    private void ApplyMaterialWall(GameObject newObj, Dictionary<string, int> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.ContainsKey("EAR") ? matEarth : matIce;
    }

    private void ApplyMaterialWallAura(GameObject newObj, Dictionary<string, int> elements)
    {
        MeshRenderer meshRenderer = newObj.GetComponent<MeshRenderer>();

        meshRenderer.material = elements.ElementAt(0).Key switch
        {
            "WAT" => matWater,
            "LIF" => matLife,
            "COL" => matCold,
            "LIG" => matLightning,
            "ARC" => matArcane,
            "FIR" => matFire,
            "STE" => matSteam,
            _ => meshRenderer.material
        };

        Color color = meshRenderer.material.color;
        meshRenderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    private void ApplyMaterialMine(GameObject newObj, Dictionary<string, int> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.ElementAt(1).Key switch
        {
            "ARC" => matArcane,
            "LIF" => matLife,
            _ => newObj.GetComponent<MeshRenderer>().material
        };
    }

    private void ApplyMaterialStorm(GameObject newObj, Dictionary<string, int> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.ElementAt(1).Key switch
        {
            "WAT" => matWater,
            "COL" => matCold,
            "LIG" => matLightning,
            "FIR" => matFire,
            "STE" => matSteam,
            _ => newObj.GetComponent<MeshRenderer>().material
        };
    }

    private void ApplyMaterialRock(GameObject newObj, Dictionary<string, int> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.ContainsKey("ICE") ? matIce : matEarth;
    }

    private void ApplyMaterialNova(GameObject newObj, Dictionary<string, int> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.ElementAt(0).Key switch
        {
            "WAT" => matWater,
            "LIF" => matLife,
            "COL" => matCold,
            "ARC" => matArcane,
            "EAR" => matEarth,
            "FIR" => matFire,
            "ICE" => matIce,
            "STE" => matSteam,
            _ => newObj.GetComponent<MeshRenderer>().material
        };
    }

    private void ApplyMaterialBeam(GameObject newObj, Dictionary<string, int> elements)
    {
        Color color = elements.ElementAt(0).Key switch
        {
            "LIF" => Color.green,
            "ARC" => Color.red,
            _ => Color.white
        };

        LineRenderer lineRenderer = newObj.GetComponent<LineRenderer>();
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private Color GetLightningColor(Dictionary<string, int> elements)
    {
        Color color;

        if (elements.ContainsKey("WAT"))
            color = matWater.color;
        else if (elements.ContainsKey("COL"))
            color = matCold.color;
        else if (elements.ContainsKey("FIR"))
            color = matFire.color;
        else
            color = matLightning.color;

        return color;
    }

    private void ApplyMaterialSpray(GameObject newObj, Dictionary<string, int> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.ElementAt(0).Key switch
        {
            "WAT" => matWater,
            "COL" => matCold,
            "FIR" => matFire,
            "STE" => matSteam,
            _ => newObj.GetComponent<MeshRenderer>().material
        };
    }

    public void CheckAndDestroyOverlappingSpells(GameObject originSpell, float radius)
    {
        Vector3 objPosition = originSpell.transform.position;
        Vector3 sphereCenter = new Vector3(objPosition.x, 0, objPosition.z);
        List<GameObject> overlappingSpells = OverlappingSpells(sphereCenter, radius);
        DestroyOverlappingSpells(overlappingSpells, originSpell);
    }

    // Obtiene una lista con todos los objetos de tipo Wall, Mine o Storm que ocupen la misma posición que este objeto
    private List<GameObject> OverlappingSpells(Vector3 center, float radius)
    {
        List<GameObject> overlappingSpells = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

        foreach (Collider hitCollider in hitColliders)
        {
            GameObject hitObject = hitCollider.gameObject;
            switch (hitObject.tag)
            {
                case "Wall":
                case "Mine":
                case "Storm":
                    overlappingSpells.Add(hitObject);
                    break;
            }
        }

        return overlappingSpells;
    }

    private void DestroyOverlappingSpells(List<GameObject> overlappingSpells, GameObject originSpell)
    {
        foreach (GameObject spell in overlappingSpells.Where(spell => spell != originSpell))
        {
            switch (spell.tag)
            {
                case "Wall":
                    spell.GetComponent<Wall>().DestroyThis();
                    break;
                case "Mine":
                    if (!spell.GetComponent<Mine>().destroyed)
                        spell.GetComponent<Mine>().DestroyThis();
                    break;
                case "Storm":
                    spell.GetComponent<Storm>().DestroyThis();
                    break;
            }
        }
    }
}