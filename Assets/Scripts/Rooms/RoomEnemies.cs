using UnityEngine;

public class RoomEnemies : MonoBehaviour
{
    [SerializeField] private Room roomScript;
    [HideInInspector] public int enemyCount;

    private void Awake()
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
            roomScript.ClearRoom();
    }
}