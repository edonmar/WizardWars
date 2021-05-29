using UnityEngine;

public class StageManager : MonoBehaviour
{
    private float gameTime;
    private bool isTimerActive;
    private int totalRooms;
    private int remainingRooms;
    [HideInInspector] public GameObject currentRoom;

    private void Start()
    {
        gameTime = 0;
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

    public void CompleteStage()
    {
        PauseTimer();
        print("WIN");
        print(ConvertTime());
        print("Rooms: " + (totalRooms - remainingRooms) + "/" + totalRooms);
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
    }
}