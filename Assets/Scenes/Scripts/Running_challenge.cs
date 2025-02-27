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

    private int[] diamondRewardScores = { 5, 10, 20 };
    private bool diamondGranted = false;
    public TextMeshProUGUI diamondPanelText;
    public GameObject diamondPanel;

    void Start()
    {
        resume.SetActive(false);
        helpPanel.SetActive(false);
        closeButton.SetActive(false);
        diamondPanel.SetActive(false);
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // Get Animator
        numberOfCoins = 0;
        numberOfDiamonds = 0;
        ChallengeTracker.currentChallenge = 3;

        Vector3 startPosition = transform.position;
        startPosition.x = startX;
        startPosition.y = 11.044f;
        transform.position = startPosition;

        targetX = startX;
        tileManager = FindObjectOfType<TileManager>();

        if (tileManager != null && tileManager.videoPlayer != null)
        {
            currentVideoName = tileManager.videoPlayer.clip.name;
        }

        // Initialize videoPlayer directly
        if (tileManager != null && tileManager.videoPlayer != null)
        {
            videoPlayer = tileManager.videoPlayer; // Assign videoPlayer from TileManager
            videoPlayer.Play();
            videoPlayer.isLooping = true;
        }

        StartCoroutine(StartRunningAfterCountdown()); // Start countdown
    }

    IEnumerator StartRunningAfterCountdown()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("ReadySound"); // Play sound only once
        }
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

    private void CheckForDiamondReward()
    {
        if (!diamondGranted && System.Array.Exists(diamondRewardScores, s => s == scoreNumber))
        {
            if (Random.value < 0.3f) // 30% chance to get diamonds
            {
                numberOfDiamonds += 3;
                diamondPanelText.text = "You got +3 Diamonds!";
                diamondPanel.SetActive(true);
                diamondGranted = true;
                StartCoroutine(HideDiamondPanel());

                PlayerPrefs.SetInt("Diamond", numberOfDiamonds);
                PlayerPrefs.Save();
            }
        }
    }

    IEnumerator HideDiamondPanel()
    {
        yield return new WaitForSeconds(2f);
        diamondPanel.SetActive(false);
    }

    public void ShowHelp()
    {
        helpPanel.SetActive(true);
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
        resume.SetActive(true);
        isRunning = false; // Pause player movement
        animator.SetBool("isRunning", false); // Pause animation

        if (videoPlayer != null)
        {
            videoPlayer.Pause(); // Pause video
        }

        Time.timeScale = 0f; // Pause the entire game
    }

    public void resumeGame()
    {
        resume.SetActive(false);

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
                Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z - 10f);
                mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetCameraPosition, ref cameraVelocity, lerpSpeed);
                mainCamera.transform.LookAt(transform);
            }
        }

        controller.center = new Vector3(0, controller.height / 2, 0.1f);
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

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Collided with: " + hit.collider.gameObject.name);
            forwardSpeed = 0;
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("GameOver");
            }
            isCompleted = true;
            isRunning = false; // Stop running
            animator.SetBool("isRunning", false); // Stop running animation

            PlayerPrefs.SetInt("Coins", numberOfCoins);
            PlayerPrefs.SetInt("Score", scoreNumber);
            int levelCompleted = PlayerPrefs.GetInt("SelectedLevelId");
            PlayerPrefs.SetInt("ChallengeIsCompleted", isCompleted ? 1 : 0); // Save as int 
            PlayerPrefs.Save();

            StartCoroutine(LoadNextSceneWithDelay());
        }
    }

    IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        SceneManager.LoadScene(6);
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
            coins.text = "Coins: " + numberOfCoins.ToString();
            Debug.Log("Collected coin: " + numberOfCoins);

            // Optionally deactivate the coin after collection
            other.gameObject.SetActive(false);
        }
    }
}
