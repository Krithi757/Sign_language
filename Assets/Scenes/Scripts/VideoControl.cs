using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Drag your VideoPlayer component here in the Inspector
    public AudioSource audioSource;

    public void PauseVideo()
    {
        Debug.Log("PauseVideo() method called"); // Check if this logs when you click the button

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned!");
            return;
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            audioSource.Pause();
            Debug.Log("Video Paused!");
        }
        else
        {
            Debug.Log("Video is already paused!");
        }
    }

    public void ResumeVideo()
    {
        Debug.Log("ResumeVideo() method called"); // Check if this logs when you click the button

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not assigned!");
            return;
        }

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            audioSource.Play();
            Debug.Log("Video Resumed!");
        }
        else
        {
            Debug.Log("Video is already playing!");
        }
    }
}
