using UnityEngine;
using System;

public class GameTimeTracker : MonoBehaviour
{
    private float elapsedTime = 0f;
    private string lastPlayedDateKey = "LastPlayedDate";
    private string timeSpentKey = "TimeSpent";
    private string weeklyTimeKey = "WeeklyTimeSpent"; // Store total weekly time

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Get the last played date from PlayerPrefs
        string lastPlayedDate = PlayerPrefs.GetString(lastPlayedDateKey, "");

        // Get today's date
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Get the start of the current week (Monday)
        DateTime startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);

        if (lastPlayedDate != todayDate)
        {
            // If the stored date is different from today, reset the time
            elapsedTime = 0f;
            PlayerPrefs.SetFloat(timeSpentKey, elapsedTime);

            // If the week has changed, reset weekly time
            if (DateTime.Now >= startOfWeek)
            {
                PlayerPrefs.SetFloat(weeklyTimeKey, 0f);  // Reset weekly time when a new week starts
            }
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

        // Save updated time spent today
        PlayerPrefs.SetFloat(timeSpentKey, elapsedTime);
        
        // Update total weekly time
        float weeklyTime = PlayerPrefs.GetFloat(weeklyTimeKey, 0f) + Time.deltaTime;
        PlayerPrefs.SetFloat(weeklyTimeKey, weeklyTime);

        PlayerPrefs.Save();
    }
}
