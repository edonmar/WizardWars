using System;
using System.Collections;
using System.Collections.Generic;
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

    private int hashParamMovSpeed;
    private int hashStatusAttack1h1;

    private void Awake()
    {
        isNavMeshAgentEnabled = true;
        player = GameObject.Find("Player").transform;
        playerCharacterStats = player.gameObject.GetComponent<CharacterStats>();
        canHit = true;

        hashParamMovSpeed = Animator.StringToHash("MovSpeed");
        hashStatusAttack1h1 = Animator.StringToHash("attack1h1");
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
        // Si no se cumple esta condición, es que han caído al vacío y no están sobre el NavMesh
        if (Math.Abs(transform.position.y) < 1)
            navMeshAgent.isStopped = true;
        animator.SetFloat(hashParamMovSpeed, 0);
        enabled = false;
    }
}