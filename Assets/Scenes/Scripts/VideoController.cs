using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Globalization;
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

        // Get video paths from VideoPathManager
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();

        // Store video names in the order provided (NO SORTING)
        videoPaths = new List<string>(videoData.Keys);

        // Store captions
        videoCaptions = new Dictionary<string, string>(videoData);

        if (videoPaths.Count == 0)
        {
            Debug.LogWarning("No videos found in VideoPathManager.");
            return;
        }

        // Button event listeners
        nextButton.onClick.AddListener(PlayNextVideo);
        previousButton.onClick.AddListener(PlayPreviousVideo);

        // Play the first video
        PlayVideo(currentVideoIndex);
    }

    void PlayVideo(int index)
    {
        if (videoPaths.Count == 0) return;

        currentVideoIndex = Mathf.Clamp(index, 0, videoPaths.Count - 1);
        string videoName = videoPaths[currentVideoIndex];
        Debug.Log($"🎥 Loading video: {videoName}");

        VideoClip videoClip = Resources.Load<VideoClip>(videoName);

        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
            Debug.Log($"Now playing: {videoName}");
        }
        else
        {
            Debug.LogWarning($"Video not found in Resources/Videos: {videoName}");
        }

        // Update captions
        captionText.text = videoCaptions.ContainsKey(videoName) ? videoCaptions[videoName] : "No caption available";
    }

    public void PlayNextVideo()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound");
        }
        if (currentVideoIndex < videoPaths.Count - 1)
        {
            currentVideoIndex++;
            PlayVideo(currentVideoIndex);
        }
    }

    public void PlayPreviousVideo()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound");
        }
        if (currentVideoIndex > 0)
        {
            currentVideoIndex--;
            PlayVideo(currentVideoIndex);
        }
    }
    public void gotoChallenge()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(4));
        //SceneManager.LoadScene(4);
    }

    public void gotoProgress()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(6));
        //SceneManager.LoadScene(4);
    }

    public void gotoHome()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(1));
        //SceneManager.LoadScene(1);
    }

    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }
}
