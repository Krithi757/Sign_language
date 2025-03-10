

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;

public class RandomVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public Text videoInfoText; // UI text to display video info

    // Lists to hold video paths and corresponding text
    private List<string> videoPaths;
    private List<string> videoTexts;
    private int currentVideoIndex = 0;

    void Start()
    {
        // Retrieve the dictionary containing video paths as keys and corresponding text as values
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();

        // Convert dictionary keys and values into lists
        videoPaths = videoData.Keys.ToList();
        videoTexts = videoData.Values.ToList();

        if (videoPaths.Count == 0)
        {
            Debug.LogError("No video paths found from VideoPathManager!");
            return;
        }

        // Shuffle both lists in parallel to preserve the mapping between video path and text
        ShuffleVideos();

        // Ensure the RawImage displays the VideoPlayer's RenderTexture
        rawImage.texture = videoPlayer.targetTexture;

        // Subscribe to the event to know when the video ends
        videoPlayer.loopPointReached += OnVideoEnd;

        // Play the first video
        PlayVideo();
    }

    void ShuffleVideos()
    {
        for (int i = 0; i < videoPaths.Count; i++)
        {
            int randomIndex = Random.Range(i, videoPaths.Count);

            // Swap video paths
            string tempPath = videoPaths[i];
            videoPaths[i] = videoPaths[randomIndex];
            videoPaths[randomIndex] = tempPath;

            // Swap the corresponding text
            string tempText = videoTexts[i];
            videoTexts[i] = videoTexts[randomIndex];
            videoTexts[randomIndex] = tempText;
        }
    }

    void PlayVideo()
    {
        if (videoPaths.Count == 0)
            return;

        string selectedVideoPath = videoPaths[currentVideoIndex];
        string selectedText = videoTexts[currentVideoIndex];

        // Load the VideoClip from the Resources folder using the video path
        VideoClip videoClip = Resources.Load<VideoClip>(selectedVideoPath);
        if (videoClip == null)
        {
            Debug.LogError("Video clip not found at path: " + selectedVideoPath);
            return;
        }

        // Set the VideoPlayer's clip and start playback
        videoPlayer.clip = videoClip;
        videoPlayer.Play();

        // Update the UI text with the corresponding text for the video
        if (videoInfoText != null)
        {
            videoInfoText.text = selectedText;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Advance to the next video (with wrap-around)
        currentVideoIndex = (currentVideoIndex + 1) % videoPaths.Count;
        PlayVideo();
    }
}