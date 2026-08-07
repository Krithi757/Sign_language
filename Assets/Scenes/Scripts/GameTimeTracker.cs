using UnityEngine;
using System;

public class GameTimeTracker : MonoBehaviour
{
    private float elapsedTime = 0f;
    private string lastPlayedDateKey = "LastPlayedDate";
    private string timeSpentKey = "TimeSpent";

    [Tooltip("How often (seconds) to actually flush to disk. PlayerPrefs.Save() every " +
             "single frame was almost certainly a big chunk of your lag — it's a " +
             "synchronous disk write, one of the most expensive things you can do " +
             "per-frame on mobile. The in-memory value still updates every frame " +
             "(cheap); only the disk write is now throttled.")]
    public float saveInterval = 5f;
    private float saveTimer = 0f;

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
        saveTimer += Time.deltaTime;

        // Keep the in-memory PlayerPrefs value current every frame (cheap —
        // this does NOT touch disk, just an in-process cache) but only
        // actually flush to disk every saveInterval seconds instead of every
        // single frame.
        PlayerPrefs.SetFloat(timeSpentKey, elapsedTime);

        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            PlayerPrefs.Save();
        }
    }

    void OnApplicationPause(bool pause)
    {
        // Make sure the latest time is actually flushed to disk if the app
        // gets backgrounded/interrupted between periodic saves.
        if (pause) PlayerPrefs.Save();
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}