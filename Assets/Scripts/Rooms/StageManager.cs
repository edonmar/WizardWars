using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    private float gameTime;
    private bool isTimerActive;
    private int totalRooms;
    private int remainingRooms;
    private int castedSpells;
    private Dictionary<string, int> castedMagicks;
    [HideInInspector] public GameObject currentRoom;

    private MagickManager magickManager;
    private GameUIManager gameUIManager;

    private void Start()
    {
        gameTime = 0;
        castedSpells = 0;
        castedMagicks = new Dictionary<string, int>();
        magickManager = GameObject.Find("Manager").GetComponent<MagickManager>();
        gameUIManager = GameObject.Find("GameUI").GetComponent<GameUIManager>();
        magickManager.LockAllMagicks();
    }

    private void Update()
    {
        if (isTimerActive)
            gameTime += Time.deltaTime;
    }

    public void StartTimer()
    {
        isTimerActive = true;
        gameTime = 0;
    }

    private void PauseTimer()
    {
        isTimerActive = false;
    }

    public void SpellCasted()
    {
        castedSpells++;
    }

    public void MagickCasted(string magickName)
    {
        castedMagicks[magickName] += 1;
    }

    public void UnlockedMagick(string magickName)
    {
        castedMagicks.Add(magickName, 0);
    }

    public void CompleteStage()
    {
        PauseTimer();

        string time = ConvertTime();
        string rooms = totalRooms - remainingRooms + "/" + totalRooms;
        int castedMagicksTotal = castedMagicks.Sum(magick => magick.Value);
        gameUIManager.ShowStageEndInfo(time, rooms, castedSpells, castedMagicksTotal);

        print("WIN");
        print(time);
        print(rooms);
        print("Casted spells: " + castedSpells);
        print("Casted magicks:");
        foreach (KeyValuePair<string, int> magick in castedMagicks)
            print(magick.Key + ": " + magick.Value);
    }

    private string ConvertTime()
    {
        int seconds = (int) gameTime;
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

    public void SetTotalRooms(int num)
    {
        totalRooms = num;
        remainingRooms = totalRooms;
    }

    public void RoomCleared()
    {
        remainingRooms--;
        Tuple<string, List<string>> magick = magickManager.UnlockRandomMagick();
        ShowRoomClearedInfo(magick);
    }

    private void ShowRoomClearedInfo(Tuple<string, List<string>> magick)
    {
        string magickName = "";
        List<string> magickElements = new List<string>();

        if (magick != null)
        {
            magickName = magick.Item1;
            magickElements = magick.Item2;
        }

        gameUIManager.ShowRoomClearedInfo(magickName, magickElements, remainingRooms);
    }
}