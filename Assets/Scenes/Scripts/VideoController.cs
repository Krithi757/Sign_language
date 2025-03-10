using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;     // Assign your VideoPlayer component in the Inspector
    public RawImage rawImage;           // Assign the RawImage UI element for video display
    public Button nextButton;           // Assign the Next button in the Inspector
    public Button previousButton;       // Assign the Previous button in the Inspector
    public TextMeshProUGUI captionText; // Assign the TextMeshPro UI element for captions

    private List<string> videoPaths = new List<string>(); // List of video names (without extensions)
    private Dictionary<string, string> videoCaptions = new Dictionary<string, string>(); // Store captions
    private int currentVideoIndex = 0;

    void Start()
    {
        int level = PlayerPrefs.GetInt("SelectedLevelId");
        Debug.Log("Current level is " + level);

        // Get video paths and captions from VideoPathManager
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();

        // Convert dictionary keys to a list and SORT them to maintain a consistent order
        videoPaths = new List<string>(videoData.Keys);
        videoPaths.Sort(); // Ensures the videos play in the same order every time

        // Store captions for each video (assuming values in the dictionary are captions)
        foreach (var kvp in videoData)
        {
            videoCaptions[kvp.Key] = kvp.Value; // Key: Video name, Value: Caption
        }

        if (videoPaths.Count == 0)
        {
            Debug.LogWarning("No videos found in VideoPathManager.");
            return;
        }

        // Add listeners for button clicks
        nextButton.onClick.AddListener(PlayNextVideo);
        previousButton.onClick.AddListener(PlayPreviousVideo);

        // Play the first video
        PlayVideo(currentVideoIndex);
    }

    void PlayVideo(int index)
    {
        if (videoPaths.Count == 0) return;

        // Ensure index stays within range
        currentVideoIndex = Mathf.Clamp(index, 0, videoPaths.Count - 1);

        string videoName = videoPaths[currentVideoIndex]; // Name without extension
        Debug.Log($"🎥 Loading video: {videoName}");

        // Load the video from Resources (e.g., Resources/Videos)
        VideoClip videoClip = Resources.Load<VideoClip>(videoName);

        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.isLooping = true; // Keep looping the current video until Next/Previous is clicked
            videoPlayer.Play();

            Debug.Log($"Now playing: {videoName}");
        }
        else
        {
            Debug.LogWarning($"Video not found in Resources/Videos: {videoName}");
        }

        // Update the caption text
        if (captionText != null && videoCaptions.ContainsKey(videoName))
        {
            captionText.text = videoCaptions[videoName]; // Set caption based on video name
        }
        else
        {
            captionText.text = "No caption available"; // Default text if no caption is found
        }
    }

    public void PlayNextVideo()
    {
        if (currentVideoIndex < videoPaths.Count - 1) // Move forward until the last video
        {
            currentVideoIndex++;
            PlayVideo(currentVideoIndex);
        }
    }

    public void PlayPreviousVideo()
    {
        if (currentVideoIndex > 0) // Move backward until the first video
        {
            currentVideoIndex--;
            PlayVideo(currentVideoIndex);
        }
    }

    public void gotoChallenge()
    {
        SceneManager.LoadScene(4);
    } 

    public void gotoHome()
    {
        SceneManager.LoadScene(1);
    }
}
