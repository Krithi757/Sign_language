using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FlyComp : MonoBehaviour
{
    public float velocity = 1;
    private Rigidbody2D rb;
    public int score = 0;
    public int coins = 0;

    // UI elements to display score and coins
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinsText;

    // Image to display at the end of the game (raw image)
    public GameObject endGameImage;

    // Reference to all pipes and moving text
    public GameObject[] pipes;
    public TextMeshProUGUI movingText; // if text is moving or animated

    private void Start()
    {
        endGameImage.SetActive(false);
        score = 0;
        coins = 0;
        rb = GetComponent<Rigidbody2D>();
        UpdateUI(); // Update the UI at the start of the game
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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

            Debug.Log("🔤 Selected Answer: " + selectedAnswer);
            Debug.Log("🎯 Correct Answer: " + correctAnswer);

            // Check if the answer is correct or not
            if (selectedAnswer == correctAnswer)
            {
                score += 20; // Correct answer: +20 points
                coins += 10; // Correct answer: +10 coins
                Debug.Log("✅ Correct! Score: " + score);
                FindObjectOfType<RandomVideoPlayer>().CheckAnswer(selectedAnswer);
            }
            else
            {
                score = Mathf.Max(0, score - 10); // Incorrect answer: -10 points (score can't go below 0)
                Debug.Log("❌ Wrong! Score: " + score);
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
            coinsText.text = "Coins: " + coins;
    }

    // Method to freeze pipes and stop text movement
    private void StopGame()
    {
        endGameImage.SetActive(true);
        // Freeze the velocity of the player's Rigidbody2D (stop the bird's movement)
        rb.velocity = Vector2.zero;

        // Freeze pipes' movement by stopping their movement scripts or freezing their Rigidbody2D
        foreach (var pipe in pipes)
        {
            Rigidbody2D pipeRb = pipe.GetComponent<Rigidbody2D>();
            if (pipeRb != null)
            {
                pipeRb.velocity = Vector2.zero; // Stop the pipe's movement
            }
        }

        // Stop moving text (if any)
        if (movingText != null)
        {
            // You can either disable the script controlling text movement or stop its position change logic
            // Example: Disable the script (assuming you have a script controlling the movement)
            movingText.gameObject.SetActive(false); // If you just want to hide it, you can use this line
        }

        // Optionally, pause the entire game
        Time.timeScale = 0; // Pause the game entirely (all physics and scripts will stop)
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
        //StartCoroutine(WaitAndLoadNextScene());
        SceneManager.LoadScene(5);
    }

    // Coroutine to handle the waiting time before transitioning to the next scene
    private IEnumerator WaitAndLoadNextScene()
    {
        yield return new WaitForSeconds(0.3f); // Wait for 2 seconds
        SceneManager.LoadScene(5);

    }

    
}
