using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int score = 0;
    public int lives = 3;

    public Text scoreText;
    public Text livesText;

    public GameObject pausePanel;
    public GameObject gameOverPanel; // Optional

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void CorrectAnswer()
    {
        score += 10;
        UpdateUI();
        Debug.Log("Correct Answer! Score: " + score);
    }

    public void WrongAnswer()
    {
        lives--;
        UpdateUI();
        Debug.Log("Wrong Answer! Lives left: " + lives);

        if (lives <= 0)
            GameOver();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
}
