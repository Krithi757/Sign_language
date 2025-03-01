using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WeeklyTimeTracker : MonoBehaviour
{
    public Slider progressBar; // UI Slider for the progress bar
    public TextMeshProUGUI timeSpentText; // UI Text to display time spent today
    public TextMeshProUGUI timeSpentTodayText; // UI Text to display time spent today

    private float timeSpentToday = 0f;
    private string timeSpentKey = "TimeSpentToday"; // Key for saving today's time spent
    private string savedDateKey = "SavedDate"; // Key for saving the last played date

    private float[] dailyTimeSpent = new float[7]; // Array to store time spent for each day of the week
    private string[] timeKeys = { "SundayTime", "MondayTime", "TuesdayTime", "WednesdayTime", "ThursdayTime", "FridayTime", "SaturdayTime" };

    void Start()
    {
        LoadWeeklyTimeSpent();
        SaveWeeklyTimeSpent();
        UpdateUI();
    }

    void Update()
    {
        // Increment the time spent for today
        int todayIndex = (int)DateTime.Now.DayOfWeek; // Get index for today (Sunday = 0, Monday = 1, ..., Saturday = 6)
        dailyTimeSpent[todayIndex] += Time.deltaTime; // Increment time spent today

        SaveWeeklyTimeSpent();
        UpdateUI();
    }

    // Load the saved time spent for each day of the week
    void LoadWeeklyTimeSpent()
    {
        for (int i = 0; i < 7; i++)
        {
            dailyTimeSpent[i] = PlayerPrefs.GetFloat(timeKeys[i], 0f);
        }

        string savedDate = PlayerPrefs.GetString(savedDateKey, "");
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (savedDate != todayDate)
        {
            // Reset time if it's a new day
            timeSpentToday = 0f;
            PlayerPrefs.SetString(savedDateKey, todayDate);
        }
        else
        {
            // Load today's time spent
            timeSpentToday = PlayerPrefs.GetFloat(timeSpentKey, 0f);
        }
    }

    // Save the time spent for each day of the week and today's time
    void SaveWeeklyTimeSpent()
    {
        for (int i = 0; i < 7; i++)
        {
            PlayerPrefs.SetFloat(timeKeys[i], dailyTimeSpent[i]);
        }

        PlayerPrefs.SetFloat(timeSpentKey, timeSpentToday);
        PlayerPrefs.Save();
    }

    // Update the UI (progress bar and text)
    void UpdateUI()
    {
        float maxTime = 1800f; // Example: 1 hour max for full progress bar

        int todayIndex = (int)DateTime.Now.DayOfWeek;
        
        if (progressBar != null)
        {
            progressBar.value = Mathf.Clamp01(dailyTimeSpent[todayIndex] / maxTime) * progressBar.maxValue;
        }

        if (timeSpentText != null)
        {
            int hours = Mathf.FloorToInt(dailyTimeSpent[todayIndex] / 3600);
            int minutes = Mathf.FloorToInt((dailyTimeSpent[todayIndex] % 3600) / 60);
            timeSpentText.text = $"{hours}h {minutes}m";
        }

        if (timeSpentTodayText != null)
        {
            int hours = Mathf.FloorToInt(timeSpentToday / 3600);
            int minutes = Mathf.FloorToInt((timeSpentToday % 3600) / 60);
            timeSpentTodayText.text = $"{hours}h {minutes}m";
        }
    }
}
