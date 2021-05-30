using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagickManager : MonoBehaviour
{
    private SpellManager spellManager;
    private StageManager stageManager;

    [SerializeField] private Transform magickTransformPrefab;

    // Diccionario donde se guardarán todos los magicks existentes
    // La clave (string) será el nombre del magick
    // El valor será una tupla con:
    // List<string> es la lista de elementos que hacen de combinación al magick
    // Bool, true significa que está desbloqueado y se puede usar
    private Dictionary<string, Tuple<List<string>, bool>> magickDict;

    private void Start()
    {
        GameObject manager = GameObject.Find("Manager");
        spellManager = manager.GetComponent<SpellManager>();
        stageManager = manager.GetComponent<StageManager>();
        MakeMagickDict();
    }

    private void MakeMagickDict()
    {
        magickDict = new Dictionary<string, Tuple<List<string>, bool>>();
        List<string> elementList;
        Tuple<List<string>, bool> tuple;

        // Meteor shower
        // Llueven bolas de fuego explosivas por toda la habitación
        elementList = new List<string> {"FIR", "EAR", "STE", "EAR", "FIR"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("MeteorShower", tuple);

        // Hailstorm
        // Llueven bolas de hielo, frío y agua explosivas por toda la habitación
        elementList = new List<string> {"COL", "ICE", "COL"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("Hailstorm", tuple);
    }

    // El nombre del magick correspondiente a una lista de elementos determinada
    // Un string vacío significa que no hay magick para esa combinación, o el que hay no está desbloqueado
    public string WrittenMagick(List<string> elementList)
    {
        string magickName = "";

        foreach (KeyValuePair<string, Tuple<List<string>, bool>> magick in magickDict)
        {
            if (!elementList.SequenceEqual(magick.Value.Item1))
                continue;

            if (magick.Value.Item2)
                magickName = magick.Key;
            break;
        }

        return magickName;
    }

    public void CastMagick(string magickName)
    {
        switch (magickName)
        {
            case "Hailstorm":
                CastHailStorm();
                break;
            case "MeteorShower":
                CastMeteorShower();
                break;
        }
    }

    private void CastHailStorm()
    {
        float rate = 0.2f;
        Dictionary<string, int> elements = new Dictionary<string, int> {{"EAR", 2}, {"ICE", 1}, {"COL", 2}};
        StartCoroutine(RockRain(elements, rate));
        elements = new Dictionary<string, int> {{"EAR", 2}, {"ICE", 1}, {"WAT", 2}};
        StartCoroutine(RockRain(elements, rate));
    }

    private void CastMeteorShower()
    {
        float rate = 0.1f;
        Dictionary<string, int> elements = new Dictionary<string, int> {{"EAR", 3}, {"FIR", 2}};
        StartCoroutine(RockRain(elements, rate));
    }

    private IEnumerator RockRain(Dictionary<string, int> elements, float rate)
    {
        float timeRemaining = 10;
        float rateTimer = rate;

        Vector3 roomPos = stageManager.currentRoom.transform.position;
        float minX = roomPos.x - 10;
        float maxX = roomPos.x + 10;
        float minZ = roomPos.z - 10;
        float maxZ = roomPos.z + 10;

        Transform magickTransform = Instantiate(magickTransformPrefab, roomPos, quaternion.identity);

        int size = elements["EAR"];
        List<Color> trailColors = spellManager.GetTrailColorsRock(elements);

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            rateTimer -= Time.deltaTime;
            if (rateTimer <= 0)
            {
                rateTimer = rate;

                float posX = Random.Range(minX, maxX);
                float posZ = Random.Range(minZ, maxZ);
                float rotX = Random.Range(-30, 30);
                float rotZ = Random.Range(-30, 30);

                magickTransform.position = new Vector3(posX, 5, posZ);
                magickTransform.rotation = Quaternion.Euler(rotX, 0, rotZ);

                spellManager.InstantiateRock(elements, magickTransform, size, 1000, 1, "SEL", trailColors);
            }

            yield return null;
        }

        Destroy(magickTransform.gameObject);
    }
}