using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    private FirebaseManager firebaseManager;

    [HideInInspector] public bool isMobile; // Si el juego se está ejecutando en un móvil

    [HideInInspector] public bool gameEnded;
    [HideInInspector] public bool result;
    [HideInInspector] public int time;
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


        firebaseManager = GameObject.Find("FirebaseManager").GetComponent<FirebaseManager>();
        isMobile = IsTouchInterface;
    }

    public void GameEnd(bool result, int time, string rooms, int castedSpells, int castedMagicksTotal,
        string magickDetails)
    {
        gameEnded = true;
        SetGameEndInfo(result, time, rooms, castedSpells, castedMagicksTotal, magickDetails);
        if (!result)
            return;
        if (firebaseManager.IsBestScore(time, rooms))
            firebaseManager.SaveData(time, rooms, castedSpells, castedMagicksTotal, magickDetails);
    }

    private void SetGameEndInfo(bool result, int time, string rooms, int castedSpells, int castedMagicksTotal,
        string magickDetails)
    {
        this.result = result;
        this.time = time;
        this.rooms = rooms;
        this.castedSpells = castedSpells;
        this.castedMagicksTotal = castedMagicksTotal;
        this.magickDetails = magickDetails;
    }
    
    private RuntimePlatform platform
    {
        get
        {
            #if UNITY_ANDROID
    			return RuntimePlatform.Android;
            #elif UNITY_IOS
    			return RuntimePlatform.IPhonePlayer;
            #elif UNITY_STANDALONE_OSX
			    return RuntimePlatform.OSXPlayer;
            #elif UNITY_STANDALONE_WIN
                return RuntimePlatform.WindowsPlayer;
            #endif
        }
    }

    private bool IsTouchInterface
    {
        get
        {
            #if UNITY_EDITOR
                // Si el juego se está ejecutando en el editor y la BuildTarget es Android o iOS
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                    return true;
            #endif

            // Si el juego se está ejecutando en un dispositivo Android o iOS
            return platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer;
        }
    }
}