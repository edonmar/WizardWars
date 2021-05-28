using UnityEngine;

public class Corridor : MonoBehaviour
{
    [SerializeField] private GameObject doors;
    public GameObject roomA;
    public GameObject roomB;

    public void OpenDoors()
    {
        doors.SetActive(false);
    }

    public void CloseDoors()
    {
        doors.SetActive(true);
    }
}