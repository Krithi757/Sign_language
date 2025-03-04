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

    public AudioSource audioSource;  
    public AudioClip coinSound;  

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

        if (isCoin && audioSource != null && coinSound != null)
        {
            PlayScaledCoinSound(targetValue); // Dynamically adjust sound
        }

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

    //  Adjusts sound length & pitch based on the number of coins collected
    void PlayScaledCoinSound(int coinAmount)
    {
        float baseDuration = coinSound.length; // Original sound length
        float adjustedDuration = Mathf.Clamp(baseDuration * (coinAmount / 10f), 0.2f, 5f); // Scale duration
        float pitchFactor = Mathf.Clamp(1.0f - (coinAmount / 100f), 0.5f, 1.2f); // Adjust pitch based on amount

        audioSource.pitch = pitchFactor;
        audioSource.Play();

        StartCoroutine(StopSoundAfterDuration(adjustedDuration));
    }

    //  Ensures the sound plays for the correct duration
    IEnumerator StopSoundAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
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
