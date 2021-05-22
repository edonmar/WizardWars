using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WaterIceCube : MonoBehaviour
{
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [SerializeField] private GameObject waterCube;
    [SerializeField] private GameObject iceCube;

    private bool isFrozen;
    private HashSet<Collider> enemiesColliding; // Lista con enemigos están actualmente encima del cubo

    private IEnumerator freezeCoroutine;
    private float freezeTime;
    private float freezeTimeRemaining;

    private void Start()
    {
        isFrozen = false;
        freezeTime = 10;
        enemiesColliding = new HashSet<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckOtherAffectThis(other);

        if (other.CompareTag("Enemy"))
            enemiesColliding.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
            enemiesColliding.Remove(other);
    }

    // Novas y Sprays pueden congelar o descongelar el cubo
    private void CheckOtherAffectThis(Collider other)
    {
        Dictionary<string, int> otherElements = new Dictionary<string, int>();

        // Obtengo una lista con los elementos del hechizo que ha provocado el trigger
        // Sólo si ese hechizo es Nova o Spray
        otherElements = other.tag switch
        {
            "Nova" => other.GetComponent<Nova>().elements,
            "Spray" => other.GetComponent<Spray>().elements,
            _ => otherElements
        };

        if (!isFrozen && otherElements.ContainsKey("COL"))
            Freeze();
        if (isFrozen && otherElements.ContainsKey("FIR"))
            UnFreeze();
    }

    private IEnumerator FreezeCoroutine()
    {
        freezeTimeRemaining = freezeTime;
        while (freezeTimeRemaining > 0)
        {
            freezeTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        UnFreeze();
    }

    private void Freeze()
    {
        navMeshObstacle.enabled = false;
        waterCube.SetActive(false);
        iceCube.SetActive(true);
        isFrozen = true;

        freezeCoroutine = FreezeCoroutine();
        StartCoroutine(freezeCoroutine);
    }

    private void UnFreeze()
    {
        DisableEnemiesCollidingNavMesh();
        StopCoroutine(freezeCoroutine);

        navMeshObstacle.enabled = true;
        waterCube.SetActive(true);
        iceCube.SetActive(false);
        isFrozen = false;
    }

    // Al descongelar el cubo, desactivo el NavMeshAgent de todos los enemigos que estén encima para que se hundan
    private void DisableEnemiesCollidingNavMesh()
    {
        foreach (Enemy enemyScript in enemiesColliding.Select(c => c.gameObject.GetComponent<Enemy>()))
            enemyScript.DisableNavMeshAgent();
    }
}