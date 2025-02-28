using System.Collections;
using UnityEngine;
using TMPro;

public class ProgressTracker : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeSpentText;
    public TextMeshProUGUI timeSpentTodayText;
    public TextMeshProUGUI timeSpentThisWeekText;
    public TextMeshProUGUI dateText;

    public float animationDuration = 2f;
    public float zoomDuration = 0.5f;
    public float zoomScale = 1.5f;

    private float originalCoinFontSize;
    private float originalDiamondFontSize;
    private float originalScoreFontSize;

    void Start()
    {
        originalCoinFontSize = coinsText.fontSize;
        originalDiamondFontSize = diamondsText.fontSize;
        originalScoreFontSize = scoreText.fontSize;

        int sessionCoins = PlayerPrefs.GetInt("Coins", 0);
        int sessionDiamonds = PlayerPrefs.GetInt("Diamond", 0);
        int sessionScore = PlayerPrefs.GetInt("Score", 0);
        float savedTime = PlayerPrefs.GetFloat("TimeSpent", 0f);
        float savedWeeklyTime = PlayerPrefs.GetFloat("WeeklyTimeSpent", 0f);

        // Update total values
        int totalCoins = PlayerPrefs.GetInt("AllCoins", 0) + sessionCoins;
        int totalDiamonds = PlayerPrefs.GetInt("AllDiamonds", 0) + sessionDiamonds;
        int totalScores = PlayerPrefs.GetInt("AllScores", 0) + sessionScore;

        // Save updated values
        PlayerPrefs.SetInt("AllCoins", totalCoins);
        PlayerPrefs.SetInt("AllDiamonds", totalDiamonds);
        PlayerPrefs.SetInt("AllScores", totalScores);
        PlayerPrefs.Save();

        UpdateTimeDisplay(savedTime);
        UpdateWeeklyTimeDisplay(savedWeeklyTime);

        string currentDate = System.DateTime.Now.ToString("ddd d MMM");
        dateText.text = currentDate;

        // Start animations for coins, diamonds, and scores
        StartCoroutine(AnimateCount(totalCoins, coinsText, originalCoinFontSize));
        StartCoroutine(AnimateCount(totalDiamonds, diamondsText, originalDiamondFontSize));
        StartCoroutine(AnimateCount(totalScores, scoreText, originalScoreFontSize));
    }

    void UpdateTimeDisplay(float elapsedTime)
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        timeSpentText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    void UpdateWeeklyTimeDisplay(float weeklyTime)
    {
        int hours = Mathf.FloorToInt(weeklyTime / 3600);
        int minutes = Mathf.FloorToInt((weeklyTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(weeklyTime % 60);

        timeSpentThisWeekText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    IEnumerator AnimateCount(int targetValue, TextMeshProUGUI textElement, float originalFontSize)
    {
        int currentValue = 0;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            int newValue = Mathf.FloorToInt(Mathf.Lerp(0, targetValue, elapsedTime / animationDuration));
            currentValue = newValue;
            textElement.text = currentValue.ToString();

            if (currentValue % 5 == 0 && currentValue != 0)
            {
                StartCoroutine(ZoomEffect(textElement, originalFontSize));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textElement.text = targetValue.ToString();
    }

    IEnumerator ZoomEffect(TextMeshProUGUI textElement, float originalFontSize)
    {
        float zoomTime = 0f;
        while (zoomTime < zoomDuration)
        {
            textElement.fontSize = Mathf.Lerp(originalFontSize, originalFontSize * zoomScale, zoomTime / zoomDuration);
            zoomTime += Time.deltaTime;
            yield return null;
        }

        zoomTime = 0f;
        while (zoomTime < zoomDuration)
        {
            textElement.fontSize = Mathf.Lerp(originalFontSize * zoomScale, originalFontSize, zoomTime / zoomDuration);
            zoomTime += Time.deltaTime;
            yield return null;
        }

        textElement.fontSize = originalFontSize;
    }
}
