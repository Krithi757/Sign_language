using System.Collections;
using UnityEngine;
using TMPro;

public class ProgressTracker : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeSpentText;
    public TextMeshProUGUI dateText;

    public float animationDuration = 2f;
    public float zoomDuration = 0.5f;
    public float zoomScale = 1.5f;

    private float originalCoinFontSize;
    private float originalDiamondFontSize;
    private float originalScoreFontSize;

    public AudioSource audioSource;  // Add this
    public AudioClip coinSound;  // Assign this in the Inspector

    void Start()
    {
        Debug.Log("Session Coins: " + PlayerPrefs.GetInt("Coins", 0));
        Debug.Log("Session Diamonds: " + PlayerPrefs.GetInt("Diamond", 0));
        Debug.Log("Session Score: " + PlayerPrefs.GetInt("Score", 0));
        Debug.Log("Time Spent: " + PlayerPrefs.GetFloat("TimeSpent", 0f));

        originalCoinFontSize = coinsText.fontSize;
        originalDiamondFontSize = diamondsText.fontSize;
        originalScoreFontSize = scoreText.fontSize;

        int sessionCoins = PlayerPrefs.GetInt("Coins", 0);
        int sessionDiamonds = PlayerPrefs.GetInt("Diamond", 0);
        int sessionScore = PlayerPrefs.GetInt("Score", 0);
        float savedTime = PlayerPrefs.GetFloat("TimeSpent", 0f);

        int totalCoins = PlayerPrefs.GetInt("AllCoins", 0) + sessionCoins;
        int totalDiamonds = PlayerPrefs.GetInt("AllDiamonds", 0) + sessionDiamonds;
        int totalScores = PlayerPrefs.GetInt("AllScores", 0) + sessionScore;

        PlayerPrefs.SetInt("AllCoins", totalCoins);
        PlayerPrefs.SetInt("AllDiamonds", totalDiamonds);
        PlayerPrefs.SetInt("AllScores", totalScores);
        PlayerPrefs.Save();

        UpdateTimeDisplay(savedTime);

        string currentDate = System.DateTime.Now.ToString("ddd d MMM");
        dateText.text = currentDate;

        StartCoroutine(AnimateCount(totalCoins, coinsText, originalCoinFontSize, true));  // Pass 'true' for coins
        StartCoroutine(AnimateCount(totalDiamonds, diamondsText, originalDiamondFontSize, false));
        StartCoroutine(AnimateCount(totalScores, scoreText, originalScoreFontSize, false));
    }

    void UpdateTimeDisplay(float elapsedTime)
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        timeSpentText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    IEnumerator AnimateCount(int targetValue, TextMeshProUGUI textElement, float originalFontSize, bool isCoin)
    {
        int currentValue = 0;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            int newValue = Mathf.FloorToInt(Mathf.Lerp(0, targetValue, elapsedTime / animationDuration));

            if (newValue > currentValue)  // Play sound only when the value increases
            {
                if (isCoin && audioSource != null && coinSound != null)
                {
                    audioSource.PlayOneShot(coinSound);
                }
            }

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
