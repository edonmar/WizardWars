using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject gameEndInfo;
    [SerializeField] private TMP_Text gameEndResult;
    [SerializeField] private TMP_Text gameEndDetails;

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