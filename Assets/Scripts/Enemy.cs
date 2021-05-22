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
    
    private int hashParamMovSpeed;

    private void Start()
    {
        isNavMeshAgentEnabled = true;
        player = GameObject.Find("Player").transform;
        playerCharacterStats = player.gameObject.GetComponent<CharacterStats>();
        
        hashParamMovSpeed = Animator.StringToHash("MovSpeed");
    }

    private void Update()
    {
        if (characterStats.isDead)
        {
            DisableNavMeshAgent();
            return;
        }

        if (isNavMeshAgentEnabled && playerCharacterStats.health > 0)
            navMeshAgent.destination = player.position;
        
        float movSpeed = characterStats.movSpeed;
        animator.SetFloat(hashParamMovSpeed, movSpeed);
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