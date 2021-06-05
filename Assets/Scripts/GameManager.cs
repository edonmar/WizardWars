using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [HideInInspector] public bool gameEnded;
    [HideInInspector] public bool result;
    [HideInInspector] public string time;
    [HideInInspector] public string rooms;
    [HideInInspector] public int castedSpells;
    [HideInInspector] public int castedMagicksTotal;
    [HideInInspector] public string magickDetails;

    public static GameManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        // Si no hay ningún GameManager instanciado
        if (instance == null)
            instance = this;

        // Si hay alguno instanciado pero no soy yo me destruyo porque no soy necesario
        else if (instance != this)
            Destroy(gameObject);

        // Indico que no debo destruirme cuando recargue los niveles
        DontDestroyOnLoad(gameObject);
    }

    public void SetGameEndInfo(bool result, string time, string rooms, int castedSpells, int castedMagicksTotal,
        string magickDetails)
    {
        this.result = result;
        this.time = time;
        this.rooms = rooms;
        this.castedSpells = castedSpells;
        this.castedMagicksTotal = castedMagicksTotal;
        this.magickDetails = magickDetails;
    }
}