using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerShootPoint;

    [SerializeField] private Material matWater;
    [SerializeField] private Material matLife;
    [SerializeField] private Material matShield;
    [SerializeField] private Material matCold;
    [SerializeField] private Material matLightning;
    [SerializeField] private Material matArcane;
    [SerializeField] private Material matEarth;
    [SerializeField] private Material matFire;
    [SerializeField] private Material matIce;
    [SerializeField] private Material matSteam;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallAuraPrefab;
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject stormPrefab;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject iciclePrefab;
    [SerializeField] private GameObject novaPrefab;

    public void HandleIntantiateWall(List<string> elements, string castType)
    {
        float distance = 2;
        float duration;

        int countEarth = elements.Count(x => x.Equals("EAR"));
        int countIce = elements.Count(x => x.Equals("ICE"));

        if (countEarth != 0)
            duration = 20 + 10 * (countEarth - 1);
        else
            duration = countIce;

        switch (castType)
        {
            case "FOC":
                for (float i = 18f; i <= 54f; i += 36f)
                {
                    InstantiateWall(elements, playerTransform, distance, i, duration);
                    InstantiateWall(elements, playerTransform, distance, -i, duration);
                }

                break;

            case "ARE":
                InstantiateWall(elements, playerTransform, distance, 0, duration);
                for (float i = 36f; i <= 144f; i += 36f)
                {
                    InstantiateWall(elements, playerTransform, distance, i, duration);
                    InstantiateWall(elements, playerTransform, distance, -i, duration);
                }

                InstantiateWall(elements, playerTransform, distance, 180, duration);
                break;
        }
    }

    public void HandleIntantiateMines(List<string> elements, string castType)
    {
        float distance = 2;

        switch (castType)
        {
            case "FOC":
                for (float i = 18f; i <= 54f; i += 36f)
                {
                    InstantiateMine(elements, playerTransform, distance, i);
                    InstantiateMine(elements, playerTransform, distance, -i);
                }

                break;

            case "ARE":
                InstantiateMine(elements, playerTransform, distance, 0);
                for (float i = 30f; i <= 150f; i += 30f)
                {
                    InstantiateMine(elements, playerTransform, distance, i);
                    InstantiateMine(elements, playerTransform, distance, -i);
                }

                InstantiateMine(elements, playerTransform, distance, 180);
                break;
        }
    }

    public void HandleIntantiateStorm(List<string> elements, string castType)
    {
        float distance = 2;
        int count = elements.Count(x => x.Equals(elements[1]));
        float duration = 4 + 2 * (count - 1);

        switch (castType)
        {
            case "FOC":
                for (float i = 11.25f; i <= 56.25f; i += 22.5f)
                {
                    InstantiateStorm(elements, playerTransform, distance, i, duration);
                    InstantiateStorm(elements, playerTransform, distance, -i, duration);
                }

                break;

            case "ARE":
                InstantiateStorm(elements, playerTransform, distance, 0f, duration);
                for (float i = 22.5f; i <= 157.5f; i += 22.5f)
                {
                    InstantiateStorm(elements, playerTransform, distance, i, duration);
                    InstantiateStorm(elements, playerTransform, distance, -i, duration);
                }

                InstantiateStorm(elements, playerTransform, distance, 180f, duration);
                break;
        }
    }

    public void HandleIntantiateRock(List<string> elements, string castType)
    {
        int size = elements.Count(x => x.Equals("EAR")) + elements.Count(x => x.Equals("ICE"));
        switch (castType)
        {
            case "FOC":
                InstantiateRock(elements, playerShootPoint, size, 1000, castType);
                break;
            case "SEL":
                InstantiateRock(elements, playerTransform, size, 1000, castType);
                break;
        }
    }

    public void HandleIntantiateIcicles(List<string> elements, string castType)
    {
        int quantity = 3 * elements.Count(x => x.Equals("ICE"));

        switch (castType)
        {
            case "FOC":
                for (int i = 0; i < quantity; i++)
                    InstantiateIcicleFocus(playerShootPoint, 1000);
                break;

            case "SEL":
                for (int i = 0; i < quantity; i++)
                    InstantiateIcicleSelfCast(playerTransform, 1000);
                break;
        }
    }

    public void HandleIntantiateNova(List<string> elements)
    {
        string mainElement;

        if (elements[0] == "EAR" || elements[0] == "ICE")
        {
            int countEarth = elements.Count(x => x.Equals("EAR"));
            int countIce = elements.Count(x => x.Equals("ICE"));
            mainElement = countEarth > countIce ? "EAR" : "ICE";
        }
        else
            mainElement = elements[0];

        int size = elements.Count(x => x.Equals(mainElement));

        InstantiateNova(elements, playerTransform, size);
    }

    private void InstantiateWall(List<string> elements, Transform originTransform, float distance,
        float rotationAround, float duration)
    {
        Vector3 playerPosition = originTransform.position;
        Vector3 spawnPos = playerPosition + originTransform.forward * distance;

        GameObject newObj = Instantiate(wallPrefab, spawnPos, originTransform.rotation);
        newObj.transform.RotateAround(playerPosition, Vector3.up, rotationAround);
        newObj.GetComponent<DissapearIn>().duration = duration;

        ApplyMaterialWall(newObj, elements);

        List<string> subListElements = elements.Where(e => e != "SHI" && e != "EAR" && e != "ICE").ToList();
        if (subListElements.Count > 0)
            InstantiateWallAura(newObj.transform, subListElements);
    }

    private void InstantiateWallAura(Transform parentTransform, List<string> elements)
    {
        string moreRepeatedElement = elements.GroupBy(x => x)
            .OrderByDescending(x => x.Count())
            .First().Key;
        int countMoreRepeatedElement = elements.Count(x => x.Equals(moreRepeatedElement));
        float duration = 5 * countMoreRepeatedElement;

        Vector3 parentPosition = parentTransform.position;

        GameObject newObj = Instantiate(wallAuraPrefab, parentPosition, parentTransform.rotation);
        newObj.transform.parent = parentTransform;
        newObj.GetComponent<DissapearIn>().duration = duration;

        ApplyMaterialWallAura(newObj, elements);
    }

    private void InstantiateMine(List<string> elements, Transform originTransform, float distance,
        float rotationAround)
    {
        Vector3 playerPosition = originTransform.position;
        Vector3 spawnPos = playerPosition + originTransform.forward * distance;
        spawnPos = new Vector3(spawnPos.x, 0.1f, spawnPos.z);

        GameObject newObj = Instantiate(minePrefab, spawnPos, originTransform.rotation);
        newObj.transform.RotateAround(playerPosition, Vector3.up, rotationAround);

        ApplyMaterialMine(newObj, elements);
    }

    private void InstantiateStorm(List<string> elements, Transform originTransform, float distance,
        float rotationAround, float duration)
    {
        Vector3 playerPosition = originTransform.position;
        Vector3 spawnPos = playerPosition + originTransform.forward * distance;

        GameObject newObj = Instantiate(stormPrefab, spawnPos, originTransform.rotation);
        newObj.transform.RotateAround(playerPosition, Vector3.up, rotationAround);
        newObj.GetComponent<DissapearIn>().duration = duration;

        ApplyMaterialStorm(newObj, elements);
    }

    private void InstantiateRock(List<string> elements, Transform originTransform, float size, float force,
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

        ApplyMaterialRock(newObj, elements);

        // Le paso a la roca los elementos con los que explotará
        List<string> subListElements = elements.Where(e => e != "EAR" && e != "ICE").ToList();
        newObj.GetComponent<Rock>().elements = subListElements;
    }

    private void InstantiateIcicleFocus(Transform originTransform, float force)
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
    }

    private void InstantiateIcicleSelfCast(Transform originTransform, float force)
    {
        Vector3 shootPointPosition = originTransform.position;
        Vector3 spawnPos = shootPointPosition + new Vector3(Random.Range(-2f, 2f), 3, Random.Range(-2f, 2f));
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        GameObject newObj = Instantiate(iciclePrefab, spawnPos, spawnRot);
        Rigidbody rb = newObj.GetComponent<Rigidbody>();
        rb.AddRelativeForce(Vector3.down * force);
    }

    // Si el origen de la nova es el jugador
    private void InstantiateNova(List<string> elements, Transform originTransform, int size)
    {
        float scale = size * 4f;
        Vector3 originPosition = originTransform.position;
        Vector3 spawnPos = new Vector3(originPosition.x, 0.2f, originPosition.z);
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        GameObject newObj = Instantiate(novaPrefab, spawnPos, spawnRot);
        newObj.transform.localScale = new Vector3(scale, 0.2f, scale);

        ApplyMaterialNova(newObj, elements);
    }

    // Si el origen de la nova es una roca explosiva
    public void HandleIntantiateNova(List<string> elements, Transform rockOfOrigin)
    {
        string mainElement = elements[0];
        int size = elements.Count(x => x.Equals(mainElement));

        InstantiateNova(elements, rockOfOrigin, size);
    }

    private void ApplyMaterialWall(GameObject newObj, List<string> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.Contains("EAR") ? matEarth : matIce;
    }

    private void ApplyMaterialWallAura(GameObject newObj, List<string> elements)
    {
        MeshRenderer meshRenderer = newObj.GetComponent<MeshRenderer>();

        meshRenderer.material = elements[0] switch
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

    private void ApplyMaterialMine(GameObject newObj, List<string> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements[1] switch
        {
            "ARC" => matArcane,
            "LIF" => matLife,
            _ => newObj.GetComponent<MeshRenderer>().material
        };
    }

    private void ApplyMaterialStorm(GameObject newObj, List<string> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements[1] switch
        {
            "WAT" => matWater,
            "COL" => matCold,
            "LIG" => matLightning,
            "FIR" => matFire,
            "STE" => matSteam,
            _ => newObj.GetComponent<MeshRenderer>().material
        };
    }

    private void ApplyMaterialRock(GameObject newObj, List<string> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements.Contains("ICE") ? matIce : matEarth;
    }

    private void ApplyMaterialNova(GameObject newObj, List<string> elements)
    {
        newObj.GetComponent<MeshRenderer>().material = elements[0] switch
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
}