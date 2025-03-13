using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class ProgressTracker : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeSpentText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI streakText;
    public TextMeshProUGUI name;

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
        if (PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName")))
        {
            // If PlayerName already exists, go to scene 9 


            string playerName = PlayerPrefs.GetString("PlayerName", "DefaultName"); // DefaultName is optional and used if PlayerName is not found

            // Log the PlayerName value
            Debug.Log("Player Name: " + playerName);
            name.text = "Welcome back, " + playerName;
        }
        else
        {
            name.gameObject.SetActive(false);
        }
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

        PlayerPrefs.Save();

        UpdateTimeDisplay(savedTime);

        string currentDate = System.DateTime.Now.ToString("ddd d MMM");
        dateText.text = currentDate;

        UpdateStreak();

        StartCoroutine(AnimateCount(PlayerPrefs.GetInt("AllCoins", 0), coinsText, originalCoinFontSize, true));
        StartCoroutine(AnimateCount(PlayerPrefs.GetInt("AllDiamonds", 0), diamondsText, originalDiamondFontSize, false));
        StartCoroutine(AnimateCount(PlayerPrefs.GetInt("Score", 0), scoreText, originalScoreFontSize, false));
    }

    void UpdateTimeDisplay(float elapsedTime)
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        timeSpentText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    void UpdateStreak()
    {
        string lastLogin = PlayerPrefs.GetString("LastLoginDate", "");
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        int streak = PlayerPrefs.GetInt("Streak", 0);

        if (lastLogin == today)
        {
            Debug.Log("Already logged in today. Streak remains the same.");
        }
        else
        {
            System.DateTime lastDate;
            if (System.DateTime.TryParse(lastLogin, out lastDate))
            {
                System.DateTime currentDate = System.DateTime.Now;
                if ((currentDate - lastDate).Days == 1)
                {
                    streak++;
                }
                else
                {
                    streak = 1;
                }
            }
            else
            {
                streak = 1;
            }

            PlayerPrefs.SetInt("Streak", streak);
            PlayerPrefs.SetString("LastLoginDate", today);
            PlayerPrefs.Save();
        }

        streakText.text = GetStreakMessage(streak);
    }

    string GetStreakMessage(int streak)
    {
        if (streak >= 30)
            return "30-Day Streak! You're doing great!";
        else if (streak >= 14)
            return "2 Weeks Streak! Keep it up!";
        else if (streak >= 7)
            return "7-Day Streak! You're on fire!";
        else if (streak >= 3)
            return "Keep going! " + streak + "-day streak!";
        else
            return "Great start! " + streak + "-day streak!";
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

    IEnumerator AnimateCount(int targetValue, TextMeshProUGUI textElement, float originalFontSize, bool isCoin)
    {
        int currentValue = int.Parse(textElement.text);
        float elapsedTime = 0f;

        if (isCoin && audioSource != null && coinSound != null)
        {
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                PlayScaledCoinSound(targetValue);
            }
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
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            audioSource.Play();
        }

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

    public void goToHome()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        Debug.Log("Clicked main menu");

        // Start the coroutine to wait for the sound to finish before loading the scene
        StartCoroutine(LoadSceneAfterSound(1));
    }

    // Coroutine to wait for the sound to finish
    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }
}
