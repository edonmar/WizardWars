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

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (gameManager.gameEnded)
        {
            SetGameEndInfo();
            ShowGameEndInfo();
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ExitGame()
    {
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
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
        infoRegisterText.text = "";

        loginScreen.SetActive(true);
        registerScreen.SetActive(false);
    }

    public void ShowRegisterScreen()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        infoLoginText.text = "";

        registerScreen.SetActive(true);
        loginScreen.SetActive(false);
    }

    private void ShowGameEndInfo()
    {
        titleScreen.SetActive(false);
        gameEndInfo.SetActive(true);
    }

    public void ShowTitleScreen()
    {
        gameEndInfo.SetActive(false);
        titleScreen.SetActive(true);
    }
}