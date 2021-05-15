using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    private Transform player;
    private CharacterStats playerCharacterStats;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        playerCharacterStats = player.gameObject.GetComponent<CharacterStats>();
    }

    void Update()
    {
        if (playerCharacterStats.health > 0)
            navMeshAgent.destination = player.position;
    }
}