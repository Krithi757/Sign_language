using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Drag your VideoPlayer component here in the Inspector

    // Call this when you click the Pause button
    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("Video Paused");
        }
    }

    // Call this when you click the Resume button
    public void ResumeVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("Video Resumed");
        }
    }
}
