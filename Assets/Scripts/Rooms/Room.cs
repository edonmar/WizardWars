using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
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
}