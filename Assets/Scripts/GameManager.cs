using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
}