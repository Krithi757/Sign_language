using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoxClickHandler : MonoBehaviour

{
    public RawImage videoDisplay;
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;

    // Removed the availableVideoPaths dictionary
    public static Dictionary<GameObject, string> boxVideoAssignments;

    private static readonly string VideoPathsKey = "AvailableVideoPaths";
    private static string selectedWord = "";

    private bool isClickedOnce = false;

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

        Debug.Log($"Script initialized for box: {gameObject.name}");
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
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;

            // Convert the touch position to the local space of the UI element
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPos, null, out localPoint);

            // Check if the touch is within the bounds of the UI element
            if (rectTransform.rect.Contains(localPoint))
            {
                HandleClick();
            }
        }
    }

    private void HandleClick()
    {
        Debug.Log($"Box touched: {gameObject.name}");

        ClearCacheAndReset();
        MoveVideoRendererToBox();

        string assignedVideoPath = boxVideoAssignments[gameObject];
        videoPlayer.clip = Resources.Load<VideoClip>(assignedVideoPath);
        videoPlayer.Prepare();

        // Trigger the rotation and video play
        StartCoroutine(RotateBox(() =>
        {
            PlayVideo(assignedVideoPath);
            videoDisplay.enabled = true;
            // After playing the video, check for match every time the box is clicked
            CheckMatchWithWord(selectedWord);
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

        StartCoroutine(RotateBoxBack());
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
        if (boxVideoAssignments.ContainsKey(gameObject))
        {
            string videoKey = boxVideoAssignments[gameObject]; // Get the video key (e.g., "Sample/Beautiful_002")

            // Check if the key exists in the dictionary and retrieve its value
            if (VideoPathManager.GetVideoPaths().TryGetValue(videoKey, out string correctWord)) // Value (e.g., "Beautiful")
            {
                if (selectedWord == correctWord) // Compare the selected word with the value
                {
                    Debug.Log($"Match! Word '{selectedWord}' matches the assigned word '{correctWord}'.");
                    // Perform any additional actions for a correct match
                }
                else
                {
                    Debug.Log($"No match. Word '{selectedWord}' does not match the assigned word '{correctWord}'.");
                }
            }
            else
            {
                Debug.LogWarning($"The video key '{videoKey}' does not exist in the dictionary.");
            }
        }
        else
        {
            Debug.LogWarning("No video assigned to this box.");
        }
    }

}
