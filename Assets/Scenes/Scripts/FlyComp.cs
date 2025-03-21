using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class FlyComp : MonoBehaviour
{
    public float velocity = 1;
    private Rigidbody2D rb;
    public int score = 0;
    public int coins = 0;

    // UI elements to display score and coins
    public TextMeshProUGUI scoreText;
    public VideoPlayer videoPlayer;
    public TextMeshProUGUI coinsText;

    // Image to display at the end of the game (raw image)
    public GameObject endGameImage;

    // Reference to all pipes and moving text
    public GameObject[] pipes;
    public TextMeshProUGUI movingText; // if text is moving or animated
    public GameObject mainMenuPanel;
    public GameObject resume;
    public GameObject paus;
    public GameObject helpPanel;
    public GameObject closeButton;

    private void Start()
    {
        mainMenuPanel.SetActive(false);
        helpPanel.SetActive(false);
        closeButton.SetActive(false);
        endGameImage.SetActive(false);
        score = 0;
        coins = 0;
        rb = GetComponent<Rigidbody2D>();
        UpdateUI(); // Update the UI at the start of the game
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            rb.velocity = Vector2.up * velocity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        // Check if the object is tagged as "Pipe"
        if (collision.CompareTag("Pipe"))
        {
            // Freeze pipes and stop text

            // Try to get the Pipe component from the colliding object
            Pipes pipe = collision.GetComponent<Pipes>();

            if (pipe == null)
            {
                Debug.LogError("❌ No Pipe component found on: " + collision.gameObject);
                return; // Exit if no Pipe component is found
            }

            // Get the answer from the Pipe component
            string selectedAnswer = pipe.GetAnswer();
            string correctAnswer = FindObjectOfType<RandomVideoPlayer>().GetCurrentAnswer();

            Debug.Log("Selected Answer: " + selectedAnswer);
            Debug.Log("Correct Answer: " + correctAnswer);

            // Check if the answer is correct or not
            if (selectedAnswer == correctAnswer)
            {
                score += 20; // Correct answer: +20 points
                coins += 10; // Correct answer: +10 coins
                Debug.Log("Correct! Score: " + score);
                FindObjectOfType<RandomVideoPlayer>().CheckAnswer(selectedAnswer);
            }
            else
            {
                score = Mathf.Max(0, score - 10); // Incorrect answer: -10 points (score can't go below 0)
                Debug.Log("Wrong! Score: " + score);
            }

            UpdateUI(); // Update the UI after the collision
        }
    }

    // Method to update the score and coin UI
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    // Method to freeze pipes and stop text movement
    private void StopGame()
    {
        endGameImage.SetActive(true);
        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Resume video
        }
        FindObjectOfType<AudioManager>().PlaySound("GameOver");
        EndGame();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Normal collision detected with: " + collision.gameObject.name);

        // Only stop the game if we collide with a pipe (not with answer text)
        if (collision.collider.CompareTag("Pip"))
        {
            StopGame();
        }
    }
    // End the game, display the raw image, and wait for 2 seconds before going to the next scene
    private void EndGame()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("IsCompleted", 1);

        int totalCoins = PlayerPrefs.GetInt("AllCoins", 0) + coins;
        PlayerPrefs.SetInt("AllCoins", totalCoins);
        PlayerPrefs.Save();

        // Wait for 2 seconds before changing the scene
        StartCoroutine(WaitAndLoadNextScene());
    }

    private void enddGame()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("IsCompleted", 1);
        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Resume video
        }

        int totalCoins = PlayerPrefs.GetInt("AllCoins", 0) + coins;
        PlayerPrefs.SetInt("AllCoins", totalCoins);
        PlayerPrefs.Save();
    }

    // Coroutine to handle the waiting time before transitioning to the next scene
    private IEnumerator WaitAndLoadNextScene()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        SceneManager.LoadScene(5);

    }

    public void pause()
    {
        mainMenuPanel.SetActive(true);
        paus.SetActive(false);
        resume.SetActive(true);
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(WaitForTapSound());
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PauseAllSounds(); // Play sound only once
        }

        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Resume video
        }

        Time.timeScale = 0f; // Pause the entire game
    }

    public void giveUp()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound");
        }
        Time.timeScale = 1f; // Ensure normal time scale 
        stoppGame();
        SceneManager.LoadScene(5);
    }

    public void ShowHelp()
    {
        helpPanel.SetActive(true);
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(WaitForTapSound());
        FindObjectOfType<AudioManager>().PauseAllSounds(); // Play sound only once
        closeButton.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Resume video
        }

        Time.timeScale = 0f; // Pause the entire game
    }

    private void stoppGame()
    {
        EndGame();
        WaitForTapSound();
    }


    public void resumeGame()
    {
        paus.SetActive(true);
        mainMenuPanel.SetActive(false);
        resume.SetActive(false);
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().ResumeAllSounds(); // Play sound only once
        }
        if (videoPlayer != null)
        {
            videoPlayer.Play(); // Resume video
        }
        Time.timeScale = 1f; // Resume the game
    }

    public void HideHelp()
    {
        helpPanel.SetActive(false);
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().ResumeAllSounds(); // Play sound only once
        }
        closeButton.SetActive(false);
        if (videoPlayer != null)
        {
            videoPlayer.Play(); // Resume video
        }
        Time.timeScale = 1f; // Resume the game
    }


    private IEnumerator WaitForTapSound()
    {
        // Wait for 0.3 seconds to allow the sound to be heard
        yield return new WaitForSeconds(0.3f);
    }


}
