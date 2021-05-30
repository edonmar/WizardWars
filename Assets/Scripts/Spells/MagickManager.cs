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
    [SerializeField] private GameObject thunderBoltPrefab;

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

        // Hailstorm
        // Llueven bolas de hielo, frío y agua explosivas por toda la habitación
        elementList = new List<string> {"COL", "ICE", "COL"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("Hailstorm", tuple);

        // Haste
        // Aumenta la velocidad de movimiento del jugador durante unos segundos
        elementList = new List<string> {"LIG", "ARC", "FIR"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("Haste", tuple);

        // Immolation aura
        // Durante los siguientes segundos, se producen explosiones de fuego alrededor del jugador
        elementList = new List<string> {"FIR", "STE", "FIR"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("ImmolationAura", tuple);

        // Levitation
        // Durante los siguientes segundos, el jugador levita (se desactiva su gravedad) y no puede caer al vacío
        elementList = new List<string> {"STE", "ARC", "STE"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("Levitation", tuple);

        // Meteor shower
        // Llueven bolas de fuego explosivas por toda la habitación
        elementList = new List<string> {"FIR", "EAR", "STE", "EAR", "FIR"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("MeteorShower", tuple);

        // Plasma bomb
        // Cae una bola explosiva en una posición aleatoria cercana al jugador
        elementList = new List<string> {"FIR", "STE", "ARC"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("PlasmaBomb", tuple);

        // Sacrifice
        // Reduce la vida del jugador a 1, pero hace mucho daño alrededor
        elementList = new List<string> {"ARC", "ARC", "ARC", "LIG"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("Sacrifice", tuple);

        // Summon death
        // Mata instantáneamente al personaje con menor porcentaje de vida que haya en la habitación
        // (puede ser el jugador)
        elementList = new List<string> {"ARC", "COL", "ICE", "COL", "ARC"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("SummonDeath", tuple);

        // Thunder bolt
        // Cae un trueno en un enemigo aleatorio
        elementList = new List<string> {"STE", "LIG", "ARC", "LIG"};
        tuple = Tuple.Create(elementList, true);
        magickDict.Add("ThunderBolt", tuple);
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

    public void CastMagick(string magickName, GameObject caster)
    {
        switch (magickName)
        {
            case "Hailstorm":
                CastHailStorm();
                break;
            case "Haste":
                CastHaste(caster);
                break;
            case "ImmolationAura":
                CastImmolationAura(caster);
                break;
            case "Levitation":
                CastLevitation(caster);
                break;
            case "MeteorShower":
                CastMeteorShower();
                break;
            case "PlasmaBomb":
                CastPlasmaBomb(caster);
                break;
            case "Sacrifice":
                CastSacrifice(caster);
                break;
            case "SummonDeath":
                CastSummonDeath(caster);
                break;
            case "ThunderBolt":
                CastThunderBolt();
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

    private void CastHaste(GameObject caster)
    {
        CharacterStats characterStats = caster.GetComponent<CharacterStats>();
        characterStats.ApplyHaste(10);
    }

    private void CastImmolationAura(GameObject caster)
    {
        int size = 3;
        float rate = 0.5f;
        Dictionary<string, int> elements = new Dictionary<string, int> {{"FIR", 5}};
        StartCoroutine(PeriodicNovas(elements, caster.transform, "character", size, rate));
    }

    private void CastLevitation(GameObject caster)
    {
        Rigidbody casterRb = caster.GetComponent<Rigidbody>();
        casterRb.useGravity = false;
        StartCoroutine(ActivateGravityIn(casterRb, 10));
    }

    private void CastMeteorShower()
    {
        float rate = 0.1f;
        Dictionary<string, int> elements = new Dictionary<string, int> {{"EAR", 3}, {"FIR", 2}};
        StartCoroutine(RockRain(elements, rate));
    }

    private void CastPlasmaBomb(GameObject caster)
    {
        Dictionary<string, int> elements = new Dictionary<string, int>
        {
            {"EAR", 3}, {"ARC", 2}, {"FIR", 25}, {"STE", 25}
        };
        int size = elements["EAR"];
        List<Color> trailColors = spellManager.GetTrailColorsRock(elements);

        Vector3 casterPos = caster.transform.position;

        float minX = casterPos.x - 5;
        float maxX = casterPos.x + 5;
        float minZ = casterPos.z - 5;
        float maxZ = casterPos.z + 5;

        float posX = Random.Range(minX, maxX);
        float posZ = Random.Range(minZ, maxZ);

        Transform magickTransform = Instantiate(magickTransformPrefab, casterPos, quaternion.identity);
        magickTransform.position = new Vector3(posX, 5, posZ);
        spellManager.InstantiateRock(elements, magickTransform, size, 0, 1, "SEL", trailColors);
        Destroy(magickTransform.gameObject);
    }

    private void CastSacrifice(GameObject caster)
    {
        CharacterStats characterStats = caster.GetComponent<CharacterStats>();
        int health = characterStats.health;
        characterStats.ModifyHealth(-(health - 1));

        int size = 5;
        Dictionary<string, int> elements = new Dictionary<string, int> {{"ARC", 60}, {"LIG", 60}};
        spellManager.InstantiateNova(elements, caster.transform, "character", size);
    }

    private void CastSummonDeath(GameObject caster)
    {
        Transform room = stageManager.currentRoom.transform;
        Transform enemies = room.Find("Enemies");

        CharacterStats playerCharacterStats = caster.GetComponent<CharacterStats>();
        float minPerc = HealthPercentage(playerCharacterStats);
        CharacterStats targetStats = playerCharacterStats;

        foreach (Transform e in enemies)
        {
            CharacterStats enemyCharacterStats = e.GetComponent<CharacterStats>();
            if (enemyCharacterStats.isDead)
                continue;
            float healthPerc = HealthPercentage(enemyCharacterStats);
            if (healthPerc <= minPerc)
                targetStats = enemyCharacterStats;
        }

        targetStats.Die();

        float HealthPercentage(CharacterStats characterStats)
        {
            int health = characterStats.health;
            int maxHealth = characterStats.maxHealth;
            return health * 100 / (float) maxHealth;
        }
    }

    private void CastThunderBolt()
    {
        // Obtengo un enemigo aleatorio de todos los enemigos de la habitación actual
        // Si no hay enemigos, no hace nada
        Transform room = stageManager.currentRoom.transform;
        Transform enemies = room.Find("Enemies");

        List<Transform> enemiesTransform =
            enemies.Cast<Transform>().Where(e => !e.GetComponent<CharacterStats>().isDead).ToList();
        int enemyCount = enemiesTransform.Count;

        int randomEnemy = Random.Range(0, enemyCount);
        GameObject target = enemiesTransform[randomEnemy].gameObject;

        // Hago que reciba el hechizo
        CharacterStats targetCharacterStats = target.GetComponent<CharacterStats>();
        Dictionary<string, int> dmgTypesDict = new Dictionary<string, int> {{"LIG", 4500}};
        targetCharacterStats.TakeSpell(dmgTypesDict);

        // Añado el efecto visual
        Transform targetTransform = target.transform;
        GameObject thunderBolt = Instantiate(thunderBoltPrefab, targetTransform.position, quaternion.identity);
        ThunderBolt thunderBoltScript = thunderBolt.GetComponent<ThunderBolt>();
        thunderBoltScript.targetTransform = targetTransform;
        thunderBoltScript.color = spellManager.matLightning.color;
    }

    private IEnumerator ActivateGravityIn(Rigidbody casterRb, float time)
    {
        yield return new WaitForSeconds(time);
        CharacterStats characterStats = casterRb.gameObject.GetComponent<CharacterStats>();
        if (!characterStats.isDead)
            casterRb.useGravity = true;
    }

    private IEnumerator PeriodicNovas(Dictionary<string, int> elements, Transform originTransform, string originType,
        int size, float rate)
    {
        CharacterStats characterStats = originTransform.gameObject.GetComponent<CharacterStats>();

        float timeRemaining = 5;
        float rateTimer = 0;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            rateTimer -= Time.deltaTime;
            if (rateTimer <= 0)
            {
                if (characterStats.isDead) // Si el personaje que la está lanzando muere, temrino la corutina
                    break;
                rateTimer = rate;
                spellManager.InstantiateNova(elements, originTransform, originType, size);
            }

            yield return null;
        }
    }

    private IEnumerator RockRain(Dictionary<string, int> elements, float rate)
    {
        float timeRemaining = 10;
        float rateTimer = 0;

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