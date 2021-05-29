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
            case "MeteorShower":
                StartCoroutine(MeteorShower());
                break;
        }
    }

    private IEnumerator MeteorShower()
    {
        float timeRemaining = 10;
        float hitTimer = 0.1f;
        float hitTimerRemaining = 0.1f;

        Vector3 roomPos = stageManager.currentRoom.transform.position;
        float minX = roomPos.x - 10;
        float maxX = roomPos.x + 10;
        float minZ = roomPos.z - 10;
        float maxZ = roomPos.z + 10;

        Transform magickTransform = Instantiate(magickTransformPrefab, roomPos, quaternion.identity);

        Dictionary<string, int> elements = new Dictionary<string, int> {{"EAR", 3}, {"FIR", 2}};
        int size = elements["EAR"];
        List<Color> trailColors = spellManager.GetTrailColorsRock(elements);

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            hitTimerRemaining -= Time.deltaTime;
            if (hitTimerRemaining <= 0)
            {
                hitTimerRemaining = hitTimer;

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