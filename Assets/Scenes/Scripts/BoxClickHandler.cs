using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BoxClickHandler : MonoBehaviour

{
    public RawImage videoDisplay;
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;
    public GameObject helpPanel; // Panel for help instructions
    public GameObject timeUpPanel;

    public GameObject closeButton;
    private static bool isBoxClicked = false;

    // Removed the availableVideoPaths dictionary
    public static Dictionary<GameObject, string> boxVideoAssignments;

    private static readonly string VideoPathsKey = "AvailableVideoPaths";
    public static string selectedWord = "";
    private static string clicked = "";
    private static int score;
    public TextMeshProUGUI scoreText;

    private bool isClickedOnce = false;

    private bool isFlyingAway = false;
    private GameObject currentBoxToFlyAway;
    public GameObject resume;

    private GameObject matchedBox2ToFlyAway;
    private float flyAwayDuration = 1.0f;
    private float flyAwayTimeElapsed = 0f;
    private static bool isPause = false;


    private Vector3 flyAwayStartPos;
    private Vector3 flyAwayStartPos2;

    private Vector3 flyAwayEndPos2;
    private Vector3 flyAwayEndPos;

    public TextMeshProUGUI coinText; // Reference to the coin UI text


    private static int coins = 0;
    public TextMeshProUGUI timeUpText;
    public TextMeshProUGUI timerText;


    private float gameDuration = 30f;
    private float timeRemaining;
    private bool isGameOver = false;


    void Awake()
    {

        // Load video paths from the manager instead of local storage
        LoadVideoPaths();

        if (boxVideoAssignments == null)
        {
            RandomizeAndAssignVideos();
        }
    }

    void Start()
    {
        timeUpPanel.SetActive(false);
        helpPanel.SetActive(false);
        closeButton.SetActive(false);
        resume.SetActive(false);
        if (boxVideoAssignments == null)
        {
            boxVideoAssignments = new Dictionary<GameObject, string>();
            Debug.Log(boxVideoAssignments);
        }

        timeRemaining = gameDuration;
        ChallengeTracker.currentChallenge = 2;
        timeUpText.gameObject.SetActive(false); // Hide "Time is Up" label at start
        StartCoroutine(GameTimer());
        if (videoPlayer == null || videoDisplay == null || renderTexture == null)
        {
            Debug.LogError("VideoPlayer, VideoDisplay, or RenderTexture not assigned!");
            return;
        }


        ClearCacheAndReset();
        ClearRenderTexture();

        videoPlayer.targetTexture = renderTexture;
        videoDisplay.texture = renderTexture;

        videoDisplay.enabled = false;
        videoPlayer.loopPointReached += OnVideoEnd;

        coinText.text = "Coins: 0";
        scoreText.text = "Score: 0";

        Debug.Log($"Script initialized for box: {gameObject.name}");
    }

    void ResetStaticVariables()
    {
        isBoxClicked = false;
        selectedWord = "";
        clicked = "";
        score = 0;
        coins = 0;

        // Ensure it's always a valid dictionary
        if (boxVideoAssignments == null)
        {
            boxVideoAssignments = new Dictionary<GameObject, string>();
        }
        else
        {
            boxVideoAssignments.Clear();
        }
    }


    IEnumerator GameTimer()
    {
        while (timeRemaining > 0)
        {
            // If the help panel is active or the game is paused, wait without decrementing time
            if ((helpPanel.activeSelf) || (isPause))
            {
                Debug.Log("Game is paused or help panel is active.");
                yield return null; // Wait until the next frame, effectively pausing the timer
                continue; // Skip to the next frame and check the conditions again
            }

            // Decrement time and update the UI if game is running
            timeRemaining -= Time.deltaTime;
            int hours = Mathf.FloorToInt(timeRemaining / 3600);
            int minutes = Mathf.FloorToInt((timeRemaining % 3600) / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);

            yield return null; // Wait until the next frame
        }

        // Once the timer ends
        EndGame();
    }

    // Pause button method to toggle the pause state
    public void isPaused()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(WaitForTapSound());
        FindObjectOfType<AudioManager>().PauseAllSounds();
        isPause = true;
        Debug.Log("isPause toggled. Current state: " + isPause);
        resume.SetActive(true);
    }
    public void isResumed()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        FindObjectOfType<AudioManager>().ResumeAllSounds();
        resume.SetActive(false);
        isPause = false;
        Debug.Log("isPause toggled. Current state: " + isPause);
    }




    void ClearRenderTexture()
    {
        if (renderTexture != null)
        {
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
            Debug.Log("RenderTexture cleared.");
        }
    }

    void Update()
    {
        if (helpPanel.activeSelf) return; // Pause game updates if help panel is open

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;

            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPos, null, out localPoint);

            if (rectTransform.rect.Contains(localPoint))
            {
                HandleClick();
            }
        }

        if (isFlyingAway)
        {
            FlyAwayAndDisableUpdate();
        }
    }


    public void HandleClick()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }

        if (isBoxClicked) return; // Prevent interaction if a box is already clicked

        // Set the flag to true as soon as a box is clicked
        isBoxClicked = true;

        Debug.Log($"Box touched: {gameObject.name}");

        ClearCacheAndReset();
        MoveVideoRendererToBox();

        string assignedVideoPath = boxVideoAssignments[gameObject];
        videoPlayer.clip = Resources.Load<VideoClip>(assignedVideoPath);
        videoPlayer.Prepare();

        // Trigger the rotation and video play
        StartCoroutine(RotateBox(() =>
        {
            Debug.Log("Playing " + assignedVideoPath);
            PlayVideo(assignedVideoPath);
            videoDisplay.enabled = true;


            clicked = assignedVideoPath;
        }));

        // Toggle click state if needed
        isClickedOnce = !isClickedOnce;
    }

    void ClearCacheAndReset()
    {
        if (videoPlayer.isPlaying || videoPlayer.isPrepared)
        {
            videoPlayer.Stop();
        }

        videoPlayer.clip = null;
        videoPlayer.Prepare();

        videoDisplay.enabled = false;
        videoDisplay.rectTransform.SetParent(null);
        videoDisplay.rectTransform.localPosition = Vector3.zero;
        videoDisplay.rectTransform.localRotation = Quaternion.identity;

        Debug.Log("Cache cleared and video player reset.");
    }

    public void MoveVideoRendererToBox()
    {
        videoDisplay.rectTransform.SetParent(transform);
        videoDisplay.rectTransform.localPosition = new Vector3(0, 0, -0.5f);
        videoDisplay.rectTransform.localRotation = Quaternion.Euler(0, 180, 0);
        videoDisplay.enabled = false;

        Debug.Log($"Video renderer moved to backside of box at local position: {videoDisplay.rectTransform.localPosition}");
    }

    void PlayVideo(string videoPath)
    {
        videoPlayer.clip = Resources.Load<VideoClip>(videoPath);

        if (videoPlayer.clip == null)
        {
            Debug.LogError($"Failed to load video: {videoPath}");
            return;
        }

        videoDisplay.enabled = true;
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnd;

        Debug.Log($"Playing video: {videoPlayer.clip.name}");
    }

    IEnumerator RotateBox(System.Action onRotationComplete)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 180, 0);

        float duration = 0.5f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;

        onRotationComplete?.Invoke();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (vp == null) return;

        vp.Stop();
        ClearCacheAndReset();
        videoDisplay.enabled = false;
        vp.loopPointReached -= OnVideoEnd;

        isBoxClicked = false;

        StartCoroutine(RotateBoxBack());
    }

    public void HandleMatch(GameObject matchedBox1, GameObject matchedBox2)
    {
        if (matchedBox1 != null && matchedBox2 != null)
        {
            // Set isFlyingAway to true for both boxes to start the flying-away behavior
            isFlyingAway = true;

            currentBoxToFlyAway = matchedBox1;
            flyAwayStartPos = matchedBox1.transform.position;

            // Fly away to a point far outside the screen (adjust range as needed)
            flyAwayEndPos = flyAwayStartPos + new Vector3(
            Random.Range(1106.077f, 1200f),  // Farther away on the X-axis (screen width + extra)
            Random.Range(2356.097f, 2400f),  // Farther away on the Y-axis (screen height + extra)
            0); // Keep the Z-axis unchanged

            matchedBox2ToFlyAway = matchedBox2; // Set matchedBox2 to fly away as well
            flyAwayStartPos2 = matchedBox2.transform.position;

            // Fly away to a point far outside the screen for matchedBox2
            flyAwayEndPos2 = flyAwayStartPos2 + new Vector3(
                Random.Range(1106.077f, 1200f),  // Farther away on the X-axis (screen width + extra)
                Random.Range(2356.097f, 2400f),  // Farther away on the Y-axis (screen height + extra)
                0); // Keep the Z-axis unchanged

            coins += 2;
            score += 1;
            coinText.text = "Coins: " + coins;
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogWarning("One of the boxes is null. Cannot handle match.");
        }
    }

    void FlyAwayAndDisableUpdate()
    {
        // Handle flying away for matchedBox1
        if (flyAwayTimeElapsed < flyAwayDuration)
        {
            currentBoxToFlyAway.transform.position = Vector3.Lerp(flyAwayStartPos, flyAwayEndPos, flyAwayTimeElapsed / flyAwayDuration);
            flyAwayTimeElapsed += Time.deltaTime;
        }
        else
        {
            currentBoxToFlyAway.transform.position = flyAwayEndPos;
            // MatchedBox1 has finished flying away, keep it active but off-screen
            // No need to deactivate it, just leave it far off-screen.
            Debug.Log(currentBoxToFlyAway + " flew away");
        }

        // Handle flying away for matchedBox2
        if (flyAwayTimeElapsed < flyAwayDuration)
        {
            matchedBox2ToFlyAway.transform.position = Vector3.Lerp(flyAwayStartPos2, flyAwayEndPos2, flyAwayTimeElapsed / flyAwayDuration);
        }
        else
        {
            matchedBox2ToFlyAway.transform.position = flyAwayEndPos2;
            // MatchedBox2 has finished flying away, keep it active but off-screen
            Debug.Log(matchedBox2ToFlyAway + " flew away");
        }

        // If both boxes have finished flying away, reset the flags
        if (flyAwayTimeElapsed >= flyAwayDuration)
        {
            isFlyingAway = false;
            flyAwayTimeElapsed = 0f;
        }
    }
    IEnumerator RotateBoxBack()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, 0);

        float duration = 1.0f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
    }

    void RandomizeAndAssignVideos()
    {
        // Get video paths from VideoPathManager instead of local availableVideoPaths dictionary
        var shuffledPaths = new List<KeyValuePair<string, string>>(VideoPathManager.GetVideoPaths());

        for (int i = 0; i < shuffledPaths.Count; i++)
        {
            var temp = shuffledPaths[i];
            int randomIndex = Random.Range(i, shuffledPaths.Count);
            shuffledPaths[i] = shuffledPaths[randomIndex];
            shuffledPaths[randomIndex] = temp;
        }

        boxVideoAssignments = new Dictionary<GameObject, string>();
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("VideoBox");

        for (int i = 0; i < boxes.Length && i < shuffledPaths.Count; i++)
        {
            boxVideoAssignments[boxes[i]] = shuffledPaths[i].Key;
            Debug.Log($"Assigned {shuffledPaths[i].Value} ({shuffledPaths[i].Key}) to box: {boxes[i].name}");
        }

        SaveVideoPaths();
    }

    void SaveVideoPaths()
    {
        List<string> serializedPaths = new List<string>();
        foreach (var pair in VideoPathManager.GetVideoPaths())
        {
            serializedPaths.Add($"{pair.Key}|{pair.Value}");
        }

        PlayerPrefs.SetString(VideoPathsKey, string.Join(",", serializedPaths));
        PlayerPrefs.Save();

        Debug.Log("Video paths saved.");
    }

    void LoadVideoPaths()
    {
        if (VideoPathManager.GetVideoPaths() != null && VideoPathManager.GetVideoPaths().Count > 0) return;

        string savedPaths = PlayerPrefs.GetString(VideoPathsKey, null);

        if (!string.IsNullOrEmpty(savedPaths))
        {
            var videoPaths = new Dictionary<string, string>();
            foreach (string entry in savedPaths.Split(','))
            {
                string[] pair = entry.Split('|');
                if (pair.Length == 2)
                {
                    videoPaths[pair[0]] = pair[1];
                }
            }

            Debug.Log("Loaded video paths from PlayerPrefs.");
        }
    }
    public void CheckMatchWithWord(string selectedWord)
    {
        if (VideoPathManager.GetVideoPaths().TryGetValue(clicked, out string correctWord))
        {
            if (selectedWord == correctWord)
            {
                Debug.Log($"Match! Word '{selectedWord}' matches the assigned word '{correctWord}'.");

                // Find the matched box for clicked
                GameObject clickedBox = null;
                foreach (var pair in boxVideoAssignments)
                {
                    if (pair.Value == clicked)
                    {
                        clickedBox = pair.Key;
                        break;
                    }
                }

                // Find the matched box for selectedWord
                GameObject selectedWordBox = null;
                foreach (var pair in WordDisplayHandler.boxWords)
                {
                    if (pair.Value == selectedWord)
                    {
                        selectedWordBox = pair.Key;
                        break;
                    }
                }

                // Ensure both boxes are found

                if (clickedBox != null && selectedWordBox != null)
                {
                    Debug.Log("Video box is " + clickedBox);
                    Debug.Log("Word box is " + selectedWordBox);
                    if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
                    {
                        FindObjectOfType<AudioManager>().PlaySound("Correct"); // Play sound only once
                    }
                    HandleMatch(selectedWordBox, clickedBox); // Pass both boxes
                }
                else
                {
                    if (clickedBox == null) Debug.LogError("Clicked box could not be found!");
                    if (selectedWordBox == null) Debug.LogError("Selected word box could not be found!");
                }

            }
            else
            {
                Debug.Log($"No match. Word '{selectedWord}' does not match the assigned word '{correctWord}'.");
                if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
                {
                    FindObjectOfType<AudioManager>().PlaySound("Error"); // Play sound only once
                }
                if (coins >= 2)
                {
                    coins -= 2;
                }
                coinText.text = "Coins: " + coins;
            }
        }
        else
        {
            Debug.LogWarning($"The video key '{clicked}' does not exist in the dictionary.");
        }
    }

    void EndGame()
    {
        isGameOver = true;
        FindObjectOfType<AudioManager>().PlaySound("GameOver");
        int isCompleted = (score == 8) ? 1 : 0;
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("IsCompleted", isCompleted);

        // Show "Time is Up" label
        timerText.text = "Time is Up!";
        timeUpPanel.gameObject.SetActive(true);

        // Wait for 1 second before transitioning to the next scene
        StartCoroutine(ShowTimeUpAndNextScene());

        Debug.Log("Game Over! IsCompleted: " + isCompleted);
    }

    IEnumerator ShowTimeUpAndNextScene()
    {
        // Wait for 1 second before moving to the next scene
        yield return new WaitForSeconds(3f);

        // Hide the "Time is up!" text
        timeUpText.gameObject.SetActive(false);
        StopAllCoroutines();

        // Load the next scene (replace with your actual scene name)
        SceneManager.LoadScene(6); // Replace "NextScene" with the actual scene name
    }
    public void ShowHelp()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(WaitForTapSound());
        FindObjectOfType<AudioManager>().PauseAllSounds();
        helpPanel.SetActive(true);
        closeButton.SetActive(true); // Show close button when panel is visible
    }

    public void HideHelp()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        FindObjectOfType<AudioManager>().ResumeAllSounds();
        helpPanel.SetActive(false);
        closeButton.SetActive(false); // Hide close button when panel is hidden
    }

    private IEnumerator WaitForTapSound()
    {
        // Wait for 0.3 seconds to allow the sound to be heard
        yield return new WaitForSeconds(1);
    }


}