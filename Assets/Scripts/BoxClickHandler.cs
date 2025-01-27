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
    public static string selectedWord = "";
    private static string clicked = "";

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

    public void HandleClick()
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

        StartCoroutine(RotateBoxBack());
    }

    public void HandleMatch(GameObject matchedBox1, GameObject matchedBox2)
    {
        StartCoroutine(FlyAwayAndDisable(matchedBox1));
        StartCoroutine(FlyAwayAndDisable(matchedBox2));
    }

    IEnumerator FlyAwayAndDisable(GameObject box)
    {
        // Fly-away animation parameters
        Vector3 startPosition = box.transform.position;
        Vector3 endPosition = startPosition + new Vector3(Random.Range(-5f, 5f), Random.Range(5f, 10f), 0); // Move off-screen
        float duration = 1.0f;
        float elapsedTime = 0f;

        // Smoothly move the box to the end position
        while (elapsedTime < duration)
        {
            box.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        box.transform.position = endPosition;

        // Disable the box after it flies away
        box.SetActive(false);
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

                // Find the matched box for `clicked`
                GameObject clickedBox = null;
                foreach (var pair in boxVideoAssignments)
                {
                    if (pair.Value == clicked)
                    {
                        clickedBox = pair.Key;
                        break;
                    }
                }

                // Find the matched box for `selectedWord`
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
                    HandleMatch(clickedBox, selectedWordBox); // Pass both boxes
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
            }
        }
        else
        {
            Debug.LogWarning($"The video key '{clicked}' does not exist in the dictionary.");
        }
    }


}
