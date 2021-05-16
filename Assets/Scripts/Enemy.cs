using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    private bool isNavMeshAgentEnabled;
    private Transform player;
    private CharacterStats playerCharacterStats;

    private void Start()
    {
        isNavMeshAgentEnabled = true;
        player = GameObject.Find("Player").transform;
        playerCharacterStats = player.gameObject.GetComponent<CharacterStats>();
    }

    private void Update()
    {
        if (isNavMeshAgentEnabled && playerCharacterStats.health > 0)
            navMeshAgent.destination = player.position;
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
}