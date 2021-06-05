using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    private MagickManager magickManager;

    // Elementos actuales del jugador
    [SerializeField] private Transform currentElementsImgs;

    // Informacion al completar una habitación
    [SerializeField] private CanvasGroup roomClearedInfoCanvasGroup;
    [SerializeField] private TMP_Text magickNameText;
    [SerializeField] private Transform magickElementsImgs;
    [SerializeField] private TMP_Text remainingRoomsText;

    private Sprite spriteEleWater;
    private Sprite spriteEleLife;
    private Sprite spriteEleShield;
    private Sprite spriteEleCold;
    private Sprite spriteEleLightning;
    private Sprite spriteEleArcane;
    private Sprite spriteEleEarth;
    private Sprite spriteEleFire;
    private Sprite spriteEleIce;
    private Sprite spriteEleSteam;

    private Image[] imgCurrEle;
    private Image[] imgNewMagickEle;

    private void Awake()
    {
        magickManager = GameObject.Find("Manager").GetComponent<MagickManager>();
        LoadSprites();
        GetCurrentElementsImages();
        GetNewMagicksElementsImages();
    }

    private void LoadSprites()
    {
        spriteEleWater = Resources.Load<Sprite>("Images/ElementIcons/EleWater");
        spriteEleLife = Resources.Load<Sprite>("Images/ElementIcons/EleLife");
        spriteEleShield = Resources.Load<Sprite>("Images/ElementIcons/EleShield");
        spriteEleCold = Resources.Load<Sprite>("Images/ElementIcons/EleCold");
        spriteEleLightning = Resources.Load<Sprite>("Images/ElementIcons/EleLightning");
        spriteEleArcane = Resources.Load<Sprite>("Images/ElementIcons/EleArcane");
        spriteEleEarth = Resources.Load<Sprite>("Images/ElementIcons/EleEarth");
        spriteEleFire = Resources.Load<Sprite>("Images/ElementIcons/EleFire");
        spriteEleIce = Resources.Load<Sprite>("Images/ElementIcons/EleIce");
        spriteEleSteam = Resources.Load<Sprite>("Images/ElementIcons/EleSteam");
    }

    private void GetCurrentElementsImages()
    {
        imgCurrEle = new Image[5];
        for (int i = 0; i < 5; i++)
            imgCurrEle[i] = currentElementsImgs.GetChild(i).GetComponent<Image>();
    }

    private void GetNewMagicksElementsImages()
    {
        imgNewMagickEle = new Image[5];
        for (int i = 0; i < 5; i++)
            imgNewMagickEle[i] = magickElementsImgs.GetChild(i).GetComponent<Image>();
    }

    public void ShowLoadedElements(List<string> loadedElements)
    {
        ShowElements(loadedElements, imgCurrEle);
    }

    public void ShowRoomClearedInfo(string magickName, List<string> magickElementss, int remainingRooms)
    {
        ShowRemainingRooms(remainingRooms);
        if (magickName == "")
            return;
        ShowMagickTitle(magickName);
        ShowElements(magickElementss, imgNewMagickEle);
        StartCoroutine(ShowCanvasGroupDuring(roomClearedInfoCanvasGroup, 4));
    }

    // Muestra los elementos seleccionados actualmente
    private void ShowElements(List<string> loadedElements, Image[] imgArray)
    {
        int len = loadedElements.Count;
        int casillasVacias = 5 - len;

        // Hace invisibles las casillas vacías
        for (int i = 5; i > 5 - casillasVacias; i--)
        {
            imgArray[i - 1].sprite = null;
            imgArray[i - 1].color = new Color(255, 255, 255, 0f);
        }

        // Muestra las casillas ocupadas
        for (int i = 0; i < len; i++)
        {
            imgArray[i].color = new Color(255, 255, 255, 1f);

            imgArray[i].sprite = loadedElements[i] switch
            {
                "WAT" => spriteEleWater,
                "LIF" => spriteEleLife,
                "SHI" => spriteEleShield,
                "COL" => spriteEleCold,
                "LIG" => spriteEleLightning,
                "ARC" => spriteEleArcane,
                "EAR" => spriteEleEarth,
                "FIR" => spriteEleFire,
                "ICE" => spriteEleIce,
                "STE" => spriteEleSteam,
                _ => imgArray[i].sprite
            };
        }
    }

    private void ShowMagickTitle(string magickName)
    {
        magickNameText.text = magickManager.MagickNameInSpanish(magickName);
    }

    private void ShowRemainingRooms(int remainingRooms)
    {
        remainingRoomsText.text = remainingRooms + " habitaciones restantes";
    }

    // El valor pasado como duration será el tiempo total que se muestre, contando el tiempo de fadeIn y fadeOut
    private IEnumerator ShowCanvasGroupDuring(CanvasGroup canvasGroup, float duration)
    {
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0, 1, 1));
        yield return new WaitForSeconds(duration);
        StartCoroutine(FadeCanvasGroup(canvasGroup, 1, 0, 1));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / duration);
            yield return null;
        }
    }
}