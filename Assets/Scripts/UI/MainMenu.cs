using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject registerScreen;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject gameEndInfo;
    [SerializeField] private TMP_Text gameEndResult;
    [SerializeField] private TMP_Text gameEndDetails;

    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text infoLoginText;

    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text infoRegisterText;

    [SerializeField] private TMP_Text userName;

    private GameManager gameManager;
    private FirebaseManager firebaseManager;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        firebaseManager = GameObject.Find("FirebaseManager").GetComponent<FirebaseManager>();
        firebaseManager.mainMenuScript = GetComponent<MainMenu>();
    }

    private void Start()
    {
        if (!gameManager.gameEnded)
            return;
        SetGameEndInfo();
        ShowGameEndInfo();
    }

    public void Play()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ExitGame()
    {
        firebaseManager.auth.SignOut();
        Application.Quit();
    }

    private void SetGameEndInfo()
    {
        gameEndResult.text = gameManager.result ? "Victoria" : "Derrota";

        string text =
            "Tiempo: " + gameManager.time + "\n" +
            "Habitaciones: " + gameManager.rooms + "\n" +
            "Hechizos usados: " + gameManager.castedSpells + "\n" +
            "Magicks usados: " + gameManager.castedMagicksTotal;
        gameEndDetails.text = text;
    }

    public void LoginButton()
    {
        firebaseManager.LoginButton();
    }

    public void RegisterButton()
    {
        firebaseManager.RegisterButton();
    }

    public void SignOutButton()
    {
        firebaseManager.SignOutButton();
    }

    public void ShowLoginScreen()
    {
        ClearRegisterFields();
        registerScreen.SetActive(false);
        titleScreen.SetActive(false);
        loginScreen.SetActive(true);
    }

    public void ShowRegisterScreen()
    {
        ClearLoginFields();
        loginScreen.SetActive(false);
        registerScreen.SetActive(true);
    }

    private void ShowGameEndInfo()
    {
        loginScreen.SetActive(false);
        titleScreen.SetActive(false);
        gameEndInfo.SetActive(true);
    }

    public void ShowTitleScreen()
    {
        ClearLoginFields();
        userName.text = firebaseManager.user.DisplayName;
        loginScreen.SetActive(false);
        gameEndInfo.SetActive(false);
        titleScreen.SetActive(true);
    }

    private void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        infoLoginText.text = "";
    }

    private void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
        infoRegisterText.text = "";
    }
}