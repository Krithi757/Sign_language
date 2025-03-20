

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;

using TMPro;
public class RandomVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
     // UI text to display video info

    // Lists to hold video paths and corresponding text
    private List<string> videoPaths;
    private List<string> videoTexts;
    private int currentVideoIndex = 0;

    void Start()
    {
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();
        videoPaths = videoData.Keys.ToList();
        videoTexts = videoData.Values.ToList();

        if (videoPaths.Count == 0)
        {
            Debug.LogError("No video paths found from VideoPathManager!");
            return;
        }

        ShuffleVideos();
        rawImage.texture = videoPlayer.targetTexture;

        // Remove auto-play when video ends
        // videoPlayer.loopPointReached += OnVideoEnd;

        PlayVideo();
    }

    // Remove OnVideoEnd() since we don't want auto-change
    // void OnVideoEnd(VideoPlayer vp) { PlayVideo(); } <-- DELETE THIS

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
        videoPlayer.isLooping = true; // ✅ Keep looping the video
        videoPlayer.Play();

        // Update the UI text with the corresponding text for the video
        
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Advance to the next video (with wrap-around)
        currentVideoIndex = (currentVideoIndex + 1) % videoPaths.Count;
        PlayVideo();
    }

    public string GetCurrentAnswer()
    {
        return videoTexts[currentVideoIndex]; // Return the correct answer
    }

    public void CheckAnswer(string selectedAnswer)
    {
        if (selectedAnswer == GetCurrentAnswer())
        {
            Debug.Log("Correct answer! Moving to next video.");

            // Move to the next video
            currentVideoIndex = (currentVideoIndex + 1) % videoPaths.Count;
            PlayVideo();
        }
        else
        {
            Debug.Log("Wrong answer! Video stays the same.");
        }
    }


}