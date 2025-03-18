using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class VideoTogglePlayPause : MonoBehaviour
{
    public VideoPlayer videoPlayer;   // Reference to the VideoPlayer component
    public Button playButton;         // Reference to the Play button
    public RawImage thumbnail;
    private bool isPaused = false;    // State to track if the video is paused
    private bool hasPlayed = false;   // State to track if the video has played at least once

    void Start()
    {
        // Ensure the Play button is properly set at the start
        playButton.gameObject.SetActive(true); // Show Play button initially

        // Add a listener for the Play button
        playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    void Update()
    {
        // Check if the screen is being touched
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Check if the touch phase is a tap (began touch)
            if (touch.phase == TouchPhase.Began)
            {
                // Convert touch position to world point
                Vector2 touchPos = touch.position;
                RectTransform rt = GetComponent<RectTransform>();
                Vector2 localPoint;

                // Convert screen position to local position relative to the RawImage
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, touchPos, null, out localPoint);

                // Check if the touch is within the bounds of the RawImage
                if (rt.rect.Contains(localPoint))
                {
                    TogglePlayPause();
                }
            }
        }
    }

    // This method is called when the Play button is clicked
    void OnPlayButtonClicked()
    {
        thumbnail.gameObject.SetActive(false);
        if (!hasPlayed)
        {
            videoPlayer.isLooping = true;  // Set the video to loop
            hasPlayed = true;              // Mark that the video has started playing
            videoPlayer.Play();            // Start the video
            playButton.gameObject.SetActive(false);  // Hide the Play button
        }
        else
        {
            TogglePlayPause(); // If video has already played, just toggle play/pause
        }
    }

    void TogglePlayPause()
    {
        if (isPaused)
        {
            videoPlayer.Play();  // Resume the video
            playButton.gameObject.SetActive(false);  // Hide the Play button
        }
        else
        {
            videoPlayer.Pause();  // Pause the video
            playButton.gameObject.SetActive(true);   // Show the Play button
        }

        // Toggle the paused state
        isPaused = !isPaused;
    }
}
