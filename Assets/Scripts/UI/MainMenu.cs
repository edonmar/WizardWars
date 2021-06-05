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

    [SerializeField] private TMP_InputField emailLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;
    [SerializeField] private TMP_Text infoLoginText;

    [SerializeField] private TMP_InputField usernameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField;
    [SerializeField] private TMP_Text infoRegisterText;

    private GameManager gameManager;
    [SerializeField] private FirebaseManager firebaseManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

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