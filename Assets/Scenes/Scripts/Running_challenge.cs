using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;

public class Running_challenge : MonoBehaviour
{
    private CharacterController controller;
    public TextMeshProUGUI countdownText;
    private Vector3 direction;
    public TextMeshProUGUI coins;
    public TextMeshProUGUI score;

    private int scoreNumber;
    public float forwardSpeed;
    private int desiredLane = 1;
    public float laneDistance = 7;
    private float targetX;
    public float laneChangeSpeed = 10f;
    private float startX = 125.87f;
    public float jumpForce;
    public float gravity = -9f;
    public static int numberOfCoins;

    private VideoPlayer videoPlayer; // Reference to the VideoPlayer

    private float videoPosX = 126.1f;
    private float videoPosY = 14.5f;
    private float videoPosZ;

    private bool hasJumped = false;

    private float lerpSpeed = 0.1f;

    public float jumpCooldown = 0.1f;
    private float lastJumpTime = -1f;
    private bool jumpRequested = false; // For jump buffering

    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0, 5, -10);
    public float cameraFollowDelay = 0.5f;

    private Vector3 cameraVelocity = Vector3.zero;
    private float followTimer = 0f;
    private TileManager tileManager;
    private string currentVideoName;
    private bool isCompleted;
    public static int numberOfDiamonds;
    public GameObject resume;
    private static bool isPause = false;

    public GameObject closeButton; // Reference to the close (X) button

    private Animator animator;
    private bool isRunning = false; // Player should start only after countdown
    public GameObject helpPanel; // Panel for help instructions

    private int[] diamondRewardScores = { 1, 5, 10, 20 };
    private bool diamondGranted = false;
    public TextMeshProUGUI diamondPanelText;
    public GameObject diamondPanel;
    public GameObject mainMenuPanel;
    private int currentDiamondsCollected = 0; // Diamonds collected in this session 
    public TextMeshProUGUI diamondText;
    private int totalDiamonds = 0; // All diamonds stored

    void Start()
    {
        currentDiamondsCollected = 0;
        numberOfDiamonds = 0;
        mainMenuPanel.SetActive(false);
        resume.SetActive(false);
        helpPanel.SetActive(false);
        closeButton.SetActive(false);
        diamondPanel.SetActive(false);
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        numberOfCoins = 0;
        totalDiamonds = PlayerPrefs.GetInt("Diamond", 0); // Load total diamonds
        currentDiamondsCollected = 0; // Reset session diamonds
        UpdateDiamondText(); // Update UI

        ChallengeTracker.currentChallenge = 3;

        Vector3 startPosition = transform.position;
        startPosition.x = startX;
        startPosition.y = 11.044f;
        transform.position = startPosition;

        targetX = startX;
        tileManager = FindObjectOfType<TileManager>();

        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(126.06f, 13.85f, 0.04f);
        }

        if (tileManager != null && tileManager.videoPlayer != null && tileManager.videoPlayer.clip != null)
        {
            currentVideoName = tileManager.videoPlayer.clip.name;
        }

        if (tileManager != null && tileManager.videoPlayer != null)
        {
            videoPlayer = tileManager.videoPlayer;
            videoPlayer.Play();
            videoPlayer.isLooping = true;
        }

        StartCoroutine(StartRunningAfterCountdown());
    }

    IEnumerator StartRunningAfterCountdown()
    {
        countdownText.gameObject.SetActive(true); // Show the countdown text

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        countdownText.text = "Go!";
        yield return new WaitForSeconds(1);
        countdownText.gameObject.SetActive(false); // Hide after countdown

        isRunning = true;
        animator.SetBool("isRunning", true); // Start running animation 
    }

    private int diamondRewardCount = 0; // Track how many times diamonds have been rewarded
    private int scoreThresholdIncrease = 0; // Increase the range limit after every 2 rewards

    private void UpdateDiamondText()
    {
        if (diamondPanelText != null)
        {
            diamondPanelText.text = $"Diamonds: {currentDiamondsCollected}";
        }
    }

    IEnumerator HideDiamondPanel()
    {
        yield return new WaitForSeconds(4f);
        diamondPanel.SetActive(false);
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

        isRunning = false; // Pause player movement
        animator.SetBool("isRunning", false); // Pause animation

        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Pause video
        }

        Time.timeScale = 0f; // Pause the entire game
    }


    public void pause()
    {
        mainMenuPanel.SetActive(true);
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
        isRunning = false; // Pause player movement
        animator.SetBool("isRunning", false); // Pause animation

        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Pause video
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
        OnEndGame();
        SceneManager.LoadScene(5);
    }


    public void resumeGame()
    {
        mainMenuPanel.SetActive(false);
        resume.SetActive(false);
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().ResumeAllSounds(); // Play sound only once
        }
        isRunning = true; // Resume player movement
        animator.SetBool("isRunning", true); // Resume animation

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

        isRunning = true; // Resume player movement
        animator.SetBool("isRunning", true); // Resume animation

        if (videoPlayer != null)
        {
            videoPlayer.Play(); // Resume video
        }

        Time.timeScale = 1f; // Resume the game
    }




    void Update()
    {
        if (!isRunning) return; // Prevent movement before countdown

        direction.z = forwardSpeed;

        // Handle jumping only when grounded and swipe up is detected
        if (SwipeManager.swipeUp && !hasJumped && controller.isGrounded)
        {
            jumpRequested = true; // Buffer the jump request
            hasJumped = true; // Mark that the player has jumped
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("JumpUp"); // Play sound only once
            }
        }

        if (controller.isGrounded)
        {
            direction.y = 0; // Reset the vertical velocity when grounded

            if (jumpRequested && Time.time - lastJumpTime >= jumpCooldown)
            {
                Jump(); // Apply jump force
                lastJumpTime = Time.time;
                jumpRequested = false; // Reset the jump buffer
                hasJumped = false; // Reset the jump state
            }
        }
        else
        {
            direction.y += gravity * Time.deltaTime; // Apply gravity when in the air
        }

        // Handle lane switching with swipes
        if (SwipeManager.swipeRight)
        {
            desiredLane = Mathf.Min(desiredLane + 1, 2);
        }
        if (SwipeManager.swipeLeft)
        {
            desiredLane = Mathf.Max(desiredLane - 1, 0);
        }

        targetX = startX + (desiredLane - 1) * laneDistance;
        Vector3 moveDirection = new Vector3(targetX - transform.position.x, 0, 0);
        controller.Move(moveDirection * laneChangeSpeed * Time.deltaTime);

        // Handle video position syncing
        if (videoPlayer != null)
        {
            videoPosZ += forwardSpeed * Time.deltaTime;
            videoPosX = transform.position.x;
            videoPlayer.transform.position = new Vector3(videoPosX, videoPosY, videoPosZ);
        }

        followTimer += Time.deltaTime;

        if (followTimer >= cameraFollowDelay)
        {
            if (mainCamera != null)
            {
                Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z - 1f);
                mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetCameraPosition, ref cameraVelocity, lerpSpeed);
                mainCamera.transform.LookAt(transform);
            }
        }

        controller.center = new Vector3(0, controller.height / 2, 0.1f);
        CheckForDiamondReward();
    }


    void FixedUpdate()
    {
        if (isRunning)
        {
            controller.Move(direction * Time.fixedDeltaTime);
        }
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }

    private void CheckForDiamondReward()
    {
        if (!diamondGranted && System.Array.Exists(diamondRewardScores, s => s == scoreNumber))
        {
            int diamondsToAdd = 0;

            if (scoreNumber == 1)
            {
                diamondsToAdd = 2;
            }
            else if (scoreNumber >= 3 + scoreThresholdIncrease && scoreNumber <= 7 + scoreThresholdIncrease)
            {
                if (Random.Range(0, 2) == 0) // 50% chance to grant diamonds
                {
                    diamondsToAdd = 2;
                    diamondRewardCount++;
                }
            }

            if (diamondsToAdd > 0)
            {
                if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
                {
                    FindObjectOfType<AudioManager>().PlaySound("DiamondSound");
                }

                Debug.Log($"Before Update: Session Diamonds = {currentDiamondsCollected}");

                currentDiamondsCollected += diamondsToAdd;  // Only update session diamonds

                diamondPanelText.text = $"You got +{diamondsToAdd} Diamonds!";
                diamondPanel.SetActive(true);
                diamondText.text = currentDiamondsCollected.ToString();
                diamondGranted = true;
                StartCoroutine(HideDiamondPanel());

                PlayerPrefs.SetInt("Diamonds", currentDiamondsCollected); // Save only session diamonds
                PlayerPrefs.Save();

                Debug.Log($"Diamonds Granted: {diamondsToAdd}, Session Diamonds: {currentDiamondsCollected}");

                UpdateDiamondText();

                if (diamondRewardCount > 0 && diamondRewardCount % 2 == 0)
                {
                    scoreThresholdIncrease += 3;
                    Debug.Log($"Score threshold increased to: {scoreThresholdIncrease}");
                }
            }
        }
    }


    void OnEndGame()
    {
        if (!isCompleted)  // Prevent duplicate execution
        {
            isCompleted = true;

            PlayerPrefs.SetInt("Coins", numberOfCoins);
            PlayerPrefs.SetInt("Score", scoreNumber);
            PlayerPrefs.SetInt("IsCompleted", 1);

            int storedTotalDiamonds = PlayerPrefs.GetInt("AllDiamonds", 0);
            totalDiamonds = storedTotalDiamonds + currentDiamondsCollected; // Update only once

            PlayerPrefs.SetInt("AllDiamonds", totalDiamonds);

            // Ensure "Diamonds" is updated correctly even when zero
            PlayerPrefs.SetInt("Diamonds", currentDiamondsCollected);
            PlayerPrefs.Save();

            Debug.Log($"End Game: AllDiamonds={totalDiamonds}, Session Diamonds={currentDiamondsCollected}");
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle") && !isCompleted) // Prevent multiple triggers
        {
            Debug.Log("Collided with: " + hit.collider.gameObject.name);
            forwardSpeed = 0;
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("GameOver");
            }
            isCompleted = true;
            isRunning = false;
            animator.SetBool("isRunning", false);

            PlayerPrefs.SetInt("Coins", numberOfCoins + 1);
            PlayerPrefs.SetInt("Score", scoreNumber);
            PlayerPrefs.SetInt("IsCompleted", 1);

            int storedTotalDiamonds = PlayerPrefs.GetInt("AllDiamonds", 0);
            totalDiamonds = storedTotalDiamonds + currentDiamondsCollected; // Update once

            PlayerPrefs.SetInt("AllDiamonds", totalDiamonds);

            // Ensure "Diamonds" is updated correctly even when zero
            PlayerPrefs.SetInt("Diamonds", currentDiamondsCollected);
            PlayerPrefs.Save();

            Debug.Log($"Game Over: AllDiamonds={totalDiamonds}, Session Diamonds={currentDiamondsCollected}");

            StartCoroutine(LoadNextSceneWithDelay());
        }
    }



    IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        SceneManager.LoadScene(5);
    }

    private void OnTriggerEnter(Collider other)
    {
        TextMeshPro textMeshPro = other.GetComponent<TextMeshPro>();

        if (textMeshPro != null && tileManager != null)
        {
            string currentVideoValue = tileManager.GetCurrentVideoValue();

            if (currentVideoValue != null && textMeshPro.text == currentVideoValue)
            {
                scoreNumber += 1;
                if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
                {
                    FindObjectOfType<AudioManager>().PlaySound("ScorePoint"); // Play sound only once
                }
                score.text = "Score: " + scoreNumber.ToString();
                tileManager.PlayNextVideo();
            }
            else
            {
                if (scoreNumber > 0)
                {
                    scoreNumber -= 1;
                    score.text = "Score: " + scoreNumber.ToString();
                }
            }
        }

        // Handling coin collision
        if (other.CompareTag("Coin"))
        {
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("PickupCoins ");
            }
            numberOfCoins += 1;
            coins.text = numberOfCoins.ToString();
            Debug.Log("Collected coin: " + numberOfCoins);

            // Optionally deactivate the coin after collection
            other.gameObject.SetActive(false);
        }
    }
    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }

    private IEnumerator WaitForTapSound()
    {
        // Wait for 0.3 seconds to allow the sound to be heard
        yield return new WaitForSeconds(0.3f);
    }

}
