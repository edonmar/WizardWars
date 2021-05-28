using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Transform cameraPosition;
    public GameObject enemies;
    [SerializeField] private GameObject doorSpaceTL;
    [SerializeField] private GameObject doorSpaceTR;
    [SerializeField] private GameObject doorSpaceBR;
    [SerializeField] private GameObject doorSpaceBL;

    private Dictionary<string, GameObject> corridors;

    private void Awake()
    {
        corridors = new Dictionary<string, GameObject>();
    }

    // Añade el pasillo al diccionario, y elimina el hueco de la puerta en la pared en la que esté el pasillo
    public void SetCorridorAndDestroyDoorSpace(string pos, GameObject c)
    {
        corridors.Add(pos, c);

        switch (pos)
        {
            case "TL":
                Destroy(doorSpaceTL);
                break;
            case "TR":
                Destroy(doorSpaceTR);
                break;
            case "BR":
                Destroy(doorSpaceBR);
                break;
            case "BL":
                Destroy(doorSpaceBL);
                break;
        }
    }

    public void EnableEnemies()
    {
        enemies.SetActive(true);
    }

    public void OpenDoors()
    {
        foreach (KeyValuePair<string, GameObject> c in corridors)
            c.Value.GetComponent<Corridor>().OpenDoors();
    }

    public void CloseDoors()
    {
        foreach (KeyValuePair<string, GameObject> c in corridors)
            c.Value.GetComponent<Corridor>().CloseDoors();
    }
}