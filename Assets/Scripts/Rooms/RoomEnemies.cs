using UnityEngine;

public class RoomEnemies : MonoBehaviour
{
    [SerializeField] private Room roomScript;
    [HideInInspector] public int enemyCount;

    private void Start()
    {
        CountEnemies();
    }

    public int CountEnemies()
    {
        enemyCount = transform.childCount;
        return enemyCount;
    }

    public void EnemyKilled()
    {
        enemyCount--;
        if (enemyCount == 0)
            roomScript.OpenDoors();
    }
}