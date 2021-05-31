using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WaterIceCube : MonoBehaviour
{
    private SpellManager spellManager;

    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [SerializeField] private GameObject iceCube;

    private bool isFrozen;
    private HashSet<Collider> enemiesColliding; // Lista con enemigos están actualmente encima del cubo

    private IEnumerator freezeCoroutine;
    private float freezeTime;
    private float freezeTimeRemaining;

    private void Awake()
    {
        spellManager = GameObject.Find("Manager").GetComponent<SpellManager>();
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

        string effect = "";
        if (otherElements.ContainsKey("COL"))
            effect = "COL";
        if (isFrozen && otherElements.ContainsKey("FIR"))
            effect = "FIR";

        if (effect == "")
            return;

        /*
        Vector3 position = transform.position + new Vector3(0, 0.5f, 0);
        if (Physics.Linecast(position, other.gameObject.transform.position, spellManager.layerMaskBarriers))
            return;
        */

        switch (effect)
        {
            case "COL":
                Freeze();
                break;
            case "FIR":
                UnFreeze();
                break;
        }
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
        if (isFrozen)
            freezeTimeRemaining = freezeTime;
        else
        {
            navMeshObstacle.enabled = false;
            iceCube.SetActive(true);
            isFrozen = true;

            freezeCoroutine = FreezeCoroutine();
            StartCoroutine(freezeCoroutine);
        }
    }

    private void UnFreeze()
    {
        DisableEnemiesCollidingNavMesh();
        StopCoroutine(freezeCoroutine);

        navMeshObstacle.enabled = true;
        iceCube.SetActive(false);
        isFrozen = false;
    }

    // Al descongelar el cubo, desactivo el NavMeshAgent de todos los enemigos que estén encima para que se hundan
    private void DisableEnemiesCollidingNavMesh()
    {
        foreach (Collider c in enemiesColliding)
        {
            if (c == null)
                continue;

            Enemy enemyScript = c.gameObject.GetComponent<Enemy>();
            Vector3 gameObjectPos = c.gameObject.transform.position;
            float radius = c.GetComponent<NavMeshAgent>().radius;

            // Si desactivara el NavMeshAgent pero no se hundiera porque estuviera pisando suelo u otro hielo, el
            // enemigo quedaría inmóvil para siempre. Para evitarlo:

            // 1 - Si está pisando el hielo pero también una baldosa, no le desactivo el NavMeshAgent
            if (enemyScript.HasFootOnFloor(gameObjectPos, radius))
                continue;

            // Si está pisando sólo el hielo y no el hielo más una baldosa:
            // 2 - Desactivo su NavMeshAgent
            enemyScript.DisableNavMeshAgent();

            // 3 - No sé si está pisando otro hielo o no, así que espero un momento y luego compruebo si está pisando
            //     otro hielo. Debo esperar un momento porque normalmente se descongelará más de un hielo al mismo
            //     tiempo, y quiero esperar a que estén todos descongelados antes de realizar la comprobación.
            //     Además, el NavMeshAgent lo empujaría rápidamente si no lo dejara desactivado por poco tiempo.
            // 4 - Si está está pisando otro hielo, le vuelvo a activar el NavMeshAgent
            StartCoroutine(enemyScript.CheckFootOnIceIn(0.5f, gameObjectPos, radius));
        }
    }
}