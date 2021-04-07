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
    [SerializeField] private GameObject sprayPrefab;

    public void HandleInstantiateWall(Dictionary<string, int> elements, string castType, Transform originTransform)
    {
        float distance = 2;
        float duration;

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
                for (float i = 18f; i <= 54f; i += 36f)
                {
                    InstantiateWall(elements, originTransform, distance, i, duration);
                    InstantiateWall(elements, originTransform, distance, -i, duration);
                }

                break;

            case "ARE":
                InstantiateWall(elements, originTransform, distance, 0, duration);
                for (float i = 36f; i <= 144f; i += 36f)
                {
                    InstantiateWall(elements, originTransform, distance, i, duration);
                    InstantiateWall(elements, originTransform, distance, -i, duration);
                }

                InstantiateWall(elements, originTransform, distance, 180, duration);
                break;
        }
    }

    public void HandleInstantiateMines(Dictionary<string, int> elements, string castType, Transform originTransform)
    {
        float distance = 2;

        switch (castType)
        {
            case "FOC":
                for (float i = 18f; i <= 54f; i += 36f)
                {
                    InstantiateMine(elements, originTransform, distance, i);
                    InstantiateMine(elements, originTransform, distance, -i);
                }

                break;

            case "ARE":
                InstantiateMine(elements, originTransform, distance, 0);
                for (float i = 30f; i <= 150f; i += 30f)
                {
                    InstantiateMine(elements, originTransform, distance, i);
                    InstantiateMine(elements, originTransform, distance, -i);
                }

                InstantiateMine(elements, originTransform, distance, 180);
                break;
        }
    }

    public void HandleInstantiateStorm(Dictionary<string, int> elements, string castType, Transform originTransform)
    {
        float distance = 2;
        int count = elements[elements.ElementAt(1).Key];
        float duration = 4 + 2 * (count - 1);

        switch (castType)
        {
            case "FOC":
                for (float i = 11.25f; i <= 56.25f; i += 22.5f)
                {
                    InstantiateStorm(elements, originTransform, distance, i, duration);
                    InstantiateStorm(elements, originTransform, distance, -i, duration);
                }

                break;

            case "ARE":
                InstantiateStorm(elements, originTransform, distance, 0f, duration);
                for (float i = 22.5f; i <= 157.5f; i += 22.5f)
                {
                    InstantiateStorm(elements, originTransform, distance, i, duration);
                    InstantiateStorm(elements, originTransform, distance, -i, duration);
                }

                InstantiateStorm(elements, originTransform, distance, 180f, duration);
                break;
        }
    }

    public void HandleInstantiateRock(Dictionary<string, int> elements, string castType, Transform casterTransform,
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

    public void HandleInstantiateIcicles(Dictionary<string, int> elements, string castType, Transform casterTransform,
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

    public void HandleInstantiateNova(Dictionary<string, int> elements, Transform originTransform)
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

        InstantiateNova(elements, originTransform, size);
    }

    public GameObject HandleInstantiateBeam(Dictionary<string, int> elements, Transform originTransform)
    {
        GameObject newObj = InstantiateBeam(elements, originTransform);
        return newObj;
    }

    public GameObject HandleInstantiateSpray(Dictionary<string, int> elements, Transform originTransform)
    {
        int size = elements.Sum(x => x.Value);
        if (elements.ContainsKey("LIG"))
            size -= elements["LIG"];

        GameObject newObj = InstantiateSpray(elements, originTransform, size);
        return newObj;
    }

    private void InstantiateWall(Dictionary<string, int> elements, Transform originTransform, float distance,
        float rotationAround, float duration)
    {
        Vector3 playerPosition = originTransform.position;
        Vector3 spawnPos = playerPosition + originTransform.forward * distance;

        GameObject newObj = Instantiate(wallPrefab, spawnPos, originTransform.rotation);
        newObj.transform.RotateAround(playerPosition, Vector3.up, rotationAround);
        newObj.GetComponent<DissapearIn>().duration = duration;

        // Le paso al wall los elementos que tendrá
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "SHI")
                .ToDictionary(e => e.Key, e => e.Value);
        newObj.GetComponent<Wall>().elements = subDictElements;

        ApplyMaterialWall(newObj, elements);
    }

    public GameObject InstantiateWallAura(Transform parentTransform, Dictionary<string, int> elements)
    {
        string moreRepeatedElement = elements.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        int countMoreRepeatedElement = elements[moreRepeatedElement];
        float duration = 5 * countMoreRepeatedElement;

        Vector3 parentPosition = parentTransform.position;

        GameObject newObj = Instantiate(wallAuraPrefab, parentPosition, parentTransform.rotation);
        newObj.transform.parent = parentTransform;
        newObj.GetComponent<DissapearIn>().duration = duration;

        // Le paso al wallAura los elementos que tendrá
        newObj.GetComponent<WallAura>().elements = elements;

        ApplyMaterialWallAura(newObj, elements);
        return newObj;
    }

    private void InstantiateMine(Dictionary<string, int> elements, Transform originTransform, float distance,
        float rotationAround)
    {
        Vector3 playerPosition = originTransform.position;
        Vector3 spawnPos = playerPosition + originTransform.forward * distance;
        spawnPos = new Vector3(spawnPos.x, 0.1f, spawnPos.z);

        GameObject newObj = Instantiate(minePrefab, spawnPos, originTransform.rotation);
        newObj.transform.RotateAround(playerPosition, Vector3.up, rotationAround);

        // Le paso a la mina los elementos con los que explotará
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "SHI")
                .ToDictionary(e => e.Key, e => e.Value);
        newObj.GetComponent<Mine>().elements = subDictElements;

        ApplyMaterialMine(newObj, elements);
    }

    private void InstantiateStorm(Dictionary<string, int> elements, Transform originTransform, float distance,
        float rotationAround, float duration)
    {
        Vector3 playerPosition = originTransform.position;
        Vector3 spawnPos = playerPosition + originTransform.forward * distance;

        GameObject newObj = Instantiate(stormPrefab, spawnPos, originTransform.rotation);
        newObj.transform.RotateAround(playerPosition, Vector3.up, rotationAround);
        newObj.GetComponent<DissapearIn>().duration = duration;

        // Le paso al storm los elementos que tendrá
        Dictionary<string, int> subDictElements =
            elements.Where(e => e.Key != "SHI")
                .ToDictionary(e => e.Key, e => e.Value);
        newObj.GetComponent<Storm>().elements = subDictElements;

        ApplyMaterialStorm(newObj, elements);
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
    private void InstantiateNova(Dictionary<string, int> elements, Transform originTransform, int size)
    {
        float scale = size * 4f;
        Vector3 originPosition = originTransform.position;
        Vector3 spawnPos = new Vector3(originPosition.x, 0.2f, originPosition.z);
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        GameObject newObj = Instantiate(novaPrefab, spawnPos, spawnRot);
        newObj.transform.localScale = new Vector3(scale, 0.2f, scale);

        // Le paso a la nova los elementos que tendrá
        newObj.GetComponent<Nova>().elements = elements;

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
}