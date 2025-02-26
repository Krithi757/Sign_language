using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using TMPro;

public class VideoPlayerController : MonoBehaviour
{
    public VideoPlayer videoPlayer;  // Assign your VideoPlayer component here
    public RawImage rawImage;  // Assign the RawImage UI element for video display
    public Button nextButton;  // Assign the Next button in the Inspector
    public Button previousButton;  // Assign the Previous button in the Inspector

    private List<string> videoPaths = new List<string>();
    private int currentVideoIndex = 0;

    void Start()
    {
        // Get video paths from VideoPathManager
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();
        videoPaths = new List<string>(videoData.Keys); // Store video file paths

        // Add listeners for button clicks
        nextButton.onClick.AddListener(PlayNextVideo);
        previousButton.onClick.AddListener(PlayPreviousVideo);

        // Play the first video
        PlayVideo(currentVideoIndex);
    }

    void PlayVideo(int index)
    {
        if (videoPaths.Count == 0) return;

        // Ensure the index stays within range
        currentVideoIndex = Mathf.Clamp(index, 0, videoPaths.Count - 1);

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoPaths[currentVideoIndex] + ".mp4");

        Debug.Log("Playing video: " + videoPath);

        videoPlayer.url = videoPath;
        videoPlayer.Play();
    }

    public void PlayNextVideo()
    {
        if (currentVideoIndex < videoPaths.Count - 1)
        {
            PlayVideo(++currentVideoIndex);
        }
        else
        {
            Debug.Log("No more videos. Looping back to first video.");
            PlayVideo(0);
        }
    }

    public void PlayPreviousVideo()
    {
        if (currentVideoIndex > 0)
        {
            PlayVideo(--currentVideoIndex);
        }
        else
        {
            Debug.Log("Already at the first video.");
        }
    }
}
