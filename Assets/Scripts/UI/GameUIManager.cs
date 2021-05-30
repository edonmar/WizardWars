using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private Transform currentElements;

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

    private void Awake()
    {
        LoadSprites();
        GetCurrentElementsImages();
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
            imgCurrEle[i] = currentElements.GetChild(i).GetComponent<Image>();
    }

    // Muestra los elementos seleccionados actualmente
    public void ShowLoadedElements(List<string> loadedElements)
    {
        int len = loadedElements.Count;
        int casillasVacias = 5 - len;

        // Hace invisibles las casillas vacías
        for (int i = 5; i > 5 - casillasVacias; i--)
        {
            imgCurrEle[i - 1].sprite = null;
            imgCurrEle[i - 1].color = new Color(255, 255, 255, 0f);
        }

        // Muestra las casillas ocupadas
        for (int i = 0; i < len; i++)
        {
            imgCurrEle[i].color = new Color(255, 255, 255, 1f);

            imgCurrEle[i].sprite = loadedElements[i] switch
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
                _ => imgCurrEle[i].sprite
            };
        }
    }
}