using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject registerScreen;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject gameEndInfo;
    [SerializeField] private GameObject pcControlsInfo;
    [SerializeField] private GameObject buttonPcControlsInfo;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject myScore;
    [SerializeField] private Transform scoreTable;
    [SerializeField] private Transform myScoreRow;

    [SerializeField] private TMP_Text gameEndResult;
    [SerializeField] private TMP_Text gameEndDetails;
    [SerializeField] private TMP_Text gameEndDetailsMagicks;

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
        if (!gameManager.isMobile)
            buttonPcControlsInfo.SetActive(true);

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
        //firebaseManager.auth.SignOut();
        Application.Quit();
    }

    private void SetGameEndInfo()
    {
        gameEndResult.text = gameManager.result ? "Victoria" : "Derrota";

        string text =
            "Tiempo: " + ConvertTime(gameManager.time) + "\n" +
            "Habitaciones: " + gameManager.rooms + "\n" +
            "Hechizos usados: " + gameManager.castedSpells + "\n" +
            "Magicks usados: " + gameManager.castedMagicksTotal;
        gameEndDetails.text = text;
        gameEndDetailsMagicks.text = gameManager.magickDetails;
    }

    private string ConvertTime(int seconds)
    {
        int minutes = seconds / 60;
        seconds -= minutes * 60;

        string strSeconds = seconds.ToString();
        string strMinutes = minutes.ToString();

        if (seconds < 10)
            strSeconds = "0" + strSeconds;
        if (minutes < 10)
            strMinutes = "0" + strMinutes;

        return strMinutes + ":" + strSeconds;
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

    public void ScoreboardButton()
    {
        StartCoroutine(firebaseManager.LoadScoreboardData());
    }

    public void ShowLoginScreen()
    {
        ClearRegisterFields();
        registerScreen.SetActive(false);
        titleScreen.SetActive(false);
        gameEndInfo.SetActive(false);
        pcControlsInfo.SetActive(false);
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

    public void ShowPcControlsInfo()
    {
        loginScreen.SetActive(false);
        pcControlsInfo.SetActive(true);
    }

    public void ShowTitleScreen()
    {
        ClearLoginFields();
        userName.text = firebaseManager.user.DisplayName;
        loginScreen.SetActive(false);
        gameEndInfo.SetActive(false);
        scoreboard.SetActive(false);
        myScore.SetActive(false);
        titleScreen.SetActive(true);
    }

    public void ShowScoreboard()
    {
        titleScreen.SetActive(false);
        scoreboard.SetActive(true);
    }

    public void ShowMyScore()
    {
        titleScreen.SetActive(false);
        myScore.SetActive(true);
    }

    public void FinishGame()
    {
        if (gameManager.isLogged)
            ShowTitleScreen();
        else
            ShowLoginScreen();
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

    public void FillScoreRow(int place, string username, int time, string rooms, int spells, int magicks,
        string magickDetails)
    {
        Transform scoreRow = scoreTable.GetChild(place - 1);
        Transform tableRow = scoreRow.Find("TableRow");

        TMP_Text placeText = tableRow.Find("Place").GetComponent<TMP_Text>();
        TMP_Text usernameText = tableRow.Find("Username").GetComponent<TMP_Text>();
        TMP_Text timeText = tableRow.Find("Time").GetComponent<TMP_Text>();
        TMP_Text roomsText = tableRow.Find("Rooms").GetComponent<TMP_Text>();
        TMP_Text spellsText = tableRow.Find("Spells").GetComponent<TMP_Text>();
        TMP_Text magicksText = tableRow.Find("Magicks").GetComponent<TMP_Text>();
        TMP_Text magickDetailsText = scoreRow.Find("MagickDetails").GetComponent<TMP_Text>();

        placeText.text = place.ToString();
        usernameText.text = username;
        timeText.text = time.ToString();
        roomsText.text = rooms;
        spellsText.text = spells.ToString();
        magicksText.text = magicks.ToString();
        magickDetailsText.text = magickDetails;
    }

    public void EmptyScoreRow(int place)
    {
        Transform scoreRow = scoreTable.GetChild(place - 1);
        Transform tableRow = scoreRow.Find("TableRow");

        TMP_Text placeText = tableRow.Find("Place").GetComponent<TMP_Text>();
        TMP_Text usernameText = tableRow.Find("Username").GetComponent<TMP_Text>();
        TMP_Text timeText = tableRow.Find("Time").GetComponent<TMP_Text>();
        TMP_Text roomsText = tableRow.Find("Rooms").GetComponent<TMP_Text>();
        TMP_Text spellsText = tableRow.Find("Spells").GetComponent<TMP_Text>();
        TMP_Text magicksText = tableRow.Find("Magicks").GetComponent<TMP_Text>();
        TMP_Text magickDetailsText = scoreRow.Find("MagickDetails").GetComponent<TMP_Text>();

        placeText.text = "";
        usernameText.text = "";
        timeText.text = "";
        roomsText.text = "";
        spellsText.text = "";
        magicksText.text = "";
        magickDetailsText.text = "";
    }

    public void FillMyScoreRow(string username, int time, string rooms, int spells, int magicks,
        string magickDetails)
    {
        Transform myTableRow = myScoreRow.Find("MyTableRow");

        TMP_Text usernameText = myTableRow.Find("Username").GetComponent<TMP_Text>();
        TMP_Text timeText = myTableRow.Find("Time").GetComponent<TMP_Text>();
        TMP_Text roomsText = myTableRow.Find("Rooms").GetComponent<TMP_Text>();
        TMP_Text spellsText = myTableRow.Find("Spells").GetComponent<TMP_Text>();
        TMP_Text magicksText = myTableRow.Find("Magicks").GetComponent<TMP_Text>();
        TMP_Text magickDetailsText = myScoreRow.Find("MagickDetails").GetComponent<TMP_Text>();

        usernameText.text = username;
        timeText.text = time.ToString();
        roomsText.text = rooms;
        spellsText.text = spells.ToString();
        magicksText.text = magicks.ToString();
        magickDetailsText.text = magickDetails;
    }

    public void EmptyMyScoreRow()
    {
        Transform myTableRow = myScoreRow.Find("MyTableRow");

        TMP_Text usernameText = myTableRow.Find("Username").GetComponent<TMP_Text>();
        TMP_Text timeText = myTableRow.Find("Time").GetComponent<TMP_Text>();
        TMP_Text roomsText = myTableRow.Find("Rooms").GetComponent<TMP_Text>();
        TMP_Text spellsText = myTableRow.Find("Spells").GetComponent<TMP_Text>();
        TMP_Text magicksText = myTableRow.Find("Magicks").GetComponent<TMP_Text>();
        TMP_Text magickDetailsText = myScoreRow.Find("MagickDetails").GetComponent<TMP_Text>();

        usernameText.text = "";
        timeText.text = "";
        roomsText.text = "";
        spellsText.text = "";
        magicksText.text = "";
        magickDetailsText.text = "";
    }
}