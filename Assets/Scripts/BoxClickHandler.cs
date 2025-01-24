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

    public static Dictionary<string, string> availableVideoPaths = new Dictionary<string, string>
    {
        { "Sample/Beautiful_002", "Beautiful" },
        { "Sample/Bad_004", "Bad" },
        { "Sample/Careful_001", "Careful" },
        { "Sample/Cold_001", "Cold" }
    };

    public static Dictionary<GameObject, string> boxVideoAssignments;

    private static readonly string VideoPathsKey = "AvailableVideoPaths";

    void Awake()
    {
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

        StartCoroutine(RotateBox(() =>
        {
            PlayVideo(assignedVideoPath);
            videoDisplay.enabled = true;
        }));
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
        var shuffledPaths = new List<KeyValuePair<string, string>>(availableVideoPaths);
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
        foreach (var pair in availableVideoPaths)
        {
            serializedPaths.Add($"{pair.Key}|{pair.Value}");
        }

        PlayerPrefs.SetString(VideoPathsKey, string.Join(",", serializedPaths));
        PlayerPrefs.Save();

        Debug.Log("Video paths saved.");
    }

    void LoadVideoPaths()
    {
        if (availableVideoPaths != null && availableVideoPaths.Count > 0) return;

        string savedPaths = PlayerPrefs.GetString(VideoPathsKey, null);

        if (!string.IsNullOrEmpty(savedPaths))
        {
            availableVideoPaths = new Dictionary<string, string>();
            foreach (string entry in savedPaths.Split(','))
            {
                string[] pair = entry.Split('|');
                if (pair.Length == 2)
                {
                    availableVideoPaths[pair[0]] = pair[1];
                }
            }

            Debug.Log("Loaded video paths from PlayerPrefs.");
        }
    }
}
