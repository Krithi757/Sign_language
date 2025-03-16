using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Text scoreText, coinsText, livesText, wordText;
    public GameObject helpPanel;
    public Button helpButton, hideHelpButton, pauseButton;
    public Sprite pauseIcon, resumeIcon; // Assign icons in Inspector

    private int score = 0;
    private int coins = 0;
    private int lives = 3;
    private bool isPaused = false;

    void Start()
    {
        UpdateUI();
        helpPanel.SetActive(false);
        pauseButton.image.sprite = pauseIcon;

        helpButton.onClick.AddListener(ShowHelp);
        hideHelpButton.onClick.AddListener(HideHelp);
        pauseButton.onClick.AddListener(TogglePause);
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        coinsText.text = "Coins: " + coins;
        livesText.text = "Lives: " + lives;
    }

    public void CheckMatch(bool isCorrect)
    {
        if (isCorrect)
        {
            score += 10;
            coins += 1;
        }
        else
        {
            score -= 5;
            lives--;

            if (lives <= 0)
            {
                GameOver();
            }
        }
        UpdateUI();
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart game
    }

    void ShowHelp()
    {
        helpPanel.SetActive(true);
    }

    void HideHelp()
    {
        helpPanel.SetActive(false);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        pauseButton.image.sprite = isPaused ? resumeIcon : pauseIcon;
    }
}
