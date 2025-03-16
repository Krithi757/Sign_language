using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject helpPanel;  // Reference to your help panel
    public GameObject pauseButton; // Reference to the pause button
    public GameObject helpButton;  // Reference to the help button

    // Singleton instance
    public static GameManager instance;

    // Score and lives variables
    public int score = 0;
    public int lives = 3;

    // Game state variables
    public bool gameStarted = false;
    public GameObject tapText;
    public Text scoreText;
    public Text livesText;

    // UI Panels for Pause and Game Over
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Initially hide the help panel
        helpPanel.SetActive(false);

        // Ensure buttons are visible during gameplay
        pauseButton.SetActive(true);
        helpButton.SetActive(true);

        // Set initial states for UI components
        UpdateUI();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Method to toggle help panel visibility
    public void ToggleHelpPanel()
    {
        helpPanel.SetActive(!helpPanel.activeSelf);
    }

    // Method to hide the help panel manually
    public void HideHelpPanel()
    {
        helpPanel.SetActive(false);
    }

    void Update()
    {
        // Start the game when the player taps the screen
        if (Input.GetMouseButtonDown(0) && !gameStarted)
        {
            gameStarted = true;
            tapText.SetActive(false);
        }
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
