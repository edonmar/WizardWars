using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private NavMeshAgent navMeshAgent;

    private bool isNavMeshAgentEnabled;
    private Transform player;
    private CharacterStats playerCharacterStats;
    private bool canHit;

    private bool isPushed;
    private IEnumerator pushCoroutine;
    private float pushTime;
    private float pushTimeRemaining;
    private float radius;

    private int hashParamMovSpeed;
    private int hashStatusAttack1h1;

    private void Awake()
    {
        isNavMeshAgentEnabled = true;
        player = GameObject.Find("Player").transform;
        playerCharacterStats = player.gameObject.GetComponent<CharacterStats>();
        canHit = true;
        isPushed = false;
        pushTime = 0.5f;
        radius = GetComponent<NavMeshAgent>().radius;

        hashParamMovSpeed = Animator.StringToHash("MovSpeed");
        hashStatusAttack1h1 = Animator.StringToHash("attack1h1");

        // Desactivo el script para que más tarde sea activado cuando el jugador entre en la misma habitación
        enabled = false;
    }

    private void Update()
    {
        if (characterStats.isDead || playerCharacterStats.isDead)
        {
            DisableEnemy();
            return;
        }

        // Seguir al jugador
        if (isNavMeshAgentEnabled && playerCharacterStats.health > 0)
            navMeshAgent.destination = player.position;

        // Animación de movimiento
        float movSpeed = navMeshAgent.velocity == Vector3.zero ? 0 : characterStats.movSpeed;
        animator.SetFloat(hashParamMovSpeed, movSpeed);

        if (CanHitPlayer())
        {
            HitPlayer();
            StartCoroutine(HitTimer(characterStats.meleeAttackSpeed));
            animator.Play(hashStatusAttack1h1);
        }
    }

    private bool CanHitPlayer()
    {
        if (!canHit)
            return false;

        if (characterStats.isFrozen || characterStats.isStunned)
            return false;

        if (playerCharacterStats.isDead)
            return false;

        if (Vector3.Distance(player.position, transform.position) > navMeshAgent.stoppingDistance)
            return false;

        return true;
    }

    private void HitPlayer()
    {
        Dictionary<string, int> elements = characterStats.meleeDmgTypes;
        playerCharacterStats.TakeSpell(elements);
    }

    private IEnumerator HitTimer(float hitRate)
    {
        canHit = false;
        yield return new WaitForSeconds(hitRate);
        canHit = true;
    }

    public void EnableNavMeshAgent()
    {
        navMeshAgent.enabled = true;
        isNavMeshAgentEnabled = true;
    }

    public void DisableNavMeshAgent()
    {
        navMeshAgent.enabled = false;
        isNavMeshAgentEnabled = false;
    }

    private void DisableEnemy()
    {
        if (navMeshAgent.enabled)
        {
            // Si no se cumple esta condición, es que han caído al vacío y no están sobre el NavMesh
            if (Math.Abs(transform.position.y) < 1)
                navMeshAgent.isStopped = true;
        }

        animator.SetFloat(hashParamMovSpeed, 0);
        enabled = false;
    }

    private IEnumerator PushCoroutine()
    {
        pushTimeRemaining = pushTime;
        while (pushTimeRemaining > 0)
        {
            pushTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        DispelPush();
    }

    public void ApplyPush()
    {
        if (isPushed)
            pushTimeRemaining = pushTime;
        else
        {
            isPushed = true;
            pushCoroutine = PushCoroutine();
            StartCoroutine(pushCoroutine);
            DisableNavMeshAgent();
        }
    }

    private void DispelPush()
    {
        if (!isPushed)
            return;

        isPushed = false;
        StopCoroutine(pushCoroutine);
        Vector3 enemyPos = transform.position;
        if (HasFootOnFloor(enemyPos, radius) || HasFootOnIce(enemyPos, radius))
            EnableNavMeshAgent();
    }

    public bool HasFootOnFloor(Vector3 center, float radius)
    {
        if (center.y < -0.5 || center.y > 0.5)
            return false;
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        return hitColliders.Select(hitCollider => hitCollider.gameObject)
            .Any(hitObject => hitObject.CompareTag("Floor"));
    }

    private bool HasFootOnIce(Vector3 center, float radius)
    {
        if (center.y < -0.5 || center.y > 0.5)
            return false;
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        return hitColliders.Select(hitCollider => hitCollider.gameObject)
            .Any(hitObject => hitObject.CompareTag("FloorIce"));
    }

    public IEnumerator CheckFootOnIceIn(float time, Vector3 center, float radius)
    {
        yield return new WaitForSeconds(time);
        if (HasFootOnIce(center, radius))
            EnableNavMeshAgent();
    }
}