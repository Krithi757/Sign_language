using UnityEngine;
using System;

public class GameTimeTracker : MonoBehaviour
{
    private float elapsedTime = 0f;
    private string lastPlayedDateKey = "LastPlayedDate";
    private string timeSpentKey = "TimeSpent";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Get the last played date from PlayerPrefs
        string lastPlayedDate = PlayerPrefs.GetString(lastPlayedDateKey, "");

        // Get today's date
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastPlayedDate != todayDate)
        {
            // If the stored date is different from today, reset the time
            elapsedTime = 0f;
            PlayerPrefs.SetFloat(timeSpentKey, elapsedTime);
        }
        else
        {
            // If it's the same day, continue tracking time
            elapsedTime = PlayerPrefs.GetFloat(timeSpentKey, 0f);
        }

        // Save today's date
        PlayerPrefs.SetString(lastPlayedDateKey, todayDate);
        PlayerPrefs.Save();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Save updated time
        PlayerPrefs.SetFloat(timeSpentKey, elapsedTime);
        PlayerPrefs.Save();
    }
}


