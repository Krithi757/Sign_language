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

        // Accumulate Total Values

        // int totalCoins = PlayerPrefs.GetInt("AllCoins", 0) + sessionCoins;
        // PlayerPrefs.SetInt("AllCoins", totalCoins);
        //PlayerPrefs.Save();
        //PlayerPrefs.SetInt("AllDiamonds", PlayerPrefs.GetInt("AllDiamonds", 0) + sessionDiamonds);
        //PlayerPrefs.SetInt("AllScores", PlayerPrefs.GetInt("AllScores", 0) + sessionScore);
        PlayerPrefs.Save();

        UpdateTimeDisplay(savedTime);

        string currentDate = System.DateTime.Now.ToString("ddd d MMM");
        dateText.text = currentDate;

        // Start animation with already stored values
        StartCoroutine(AnimateCount(PlayerPrefs.GetInt("AllCoins", 0), coinsText, originalCoinFontSize, true));
        StartCoroutine(AnimateCount(PlayerPrefs.GetInt("AllDiamonds", 0), diamondsText, originalDiamondFontSize, false));
        StartCoroutine(AnimateCount(PlayerPrefs.GetInt("AllScores", 0), scoreText, originalScoreFontSize, false));
    }

    public void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        int totalCoins = PlayerPrefs.GetInt("AllCoins", 0);

        Debug.Log($"Before Adding: Coins = {currentCoins}, AllCoins = {totalCoins}");

        currentCoins += amount;
        totalCoins += amount;

        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.SetInt("AllCoins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log($"After Adding: Coins = {currentCoins}, AllCoins = {totalCoins}");

        StartCoroutine(AnimateCount(totalCoins, coinsText, originalCoinFontSize, true));
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
        int currentValue = int.Parse(textElement.text);
        float elapsedTime = 0f;

        if (isCoin && audioSource != null && coinSound != null)
        {
            PlayScaledCoinSound(targetValue);
        }

        while (elapsedTime < animationDuration)
        {
            int newValue = Mathf.FloorToInt(Mathf.Lerp(currentValue, targetValue, elapsedTime / animationDuration));
            textElement.text = newValue.ToString();

            if (newValue % 5 == 0 && newValue != 0)
            {
                StartCoroutine(ZoomEffect(textElement, originalFontSize));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textElement.text = targetValue.ToString();
    }

    void PlayScaledCoinSound(int coinAmount)
    {
        float baseDuration = coinSound.length;
        float adjustedDuration = Mathf.Clamp(baseDuration * (coinAmount / 10f), 0.2f, 5f);
        float pitchFactor = Mathf.Clamp(1.0f - (coinAmount / 100f), 0.5f, 1.2f);

        audioSource.pitch = pitchFactor;
        audioSource.Play();

        StartCoroutine(StopSoundAfterDuration(adjustedDuration));
    }

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
