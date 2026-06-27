using UnityEngine;
using UnityEngine.Video;

public class TapToChangeVideo : MonoBehaviour
{
    [Tooltip("Drag the GameObject that has the VideoPlayer component here.")]
    public VideoPlayer videoPlayer;

    private VideoClip[] videoClips;
    private int currentIndex = 0;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("❌ No VideoPlayer assigned!");
            return;
        }

        string folderPath = "Level3";

        Debug.Log("=======================================");
        Debug.Log("Loading videos from Resources/" + folderPath);

        videoClips = Resources.LoadAll<VideoClip>(folderPath);

        Debug.Log("Videos Found: " + videoClips.Length);

        if (videoClips.Length == 0)
        {
            Debug.LogError("❌ No videos found!");

            Debug.LogError("Expected location:");
            Debug.LogError("Assets/TextMesh Pro/Resources/Level3/");

            return;
        }

        Debug.Log("----------- Loaded Videos -----------");

        for (int i = 0; i < videoClips.Length; i++)
        {
            Debug.Log((i + 1) + ". " + videoClips[i].name);
        }

        Debug.Log("-------------------------------------");

        // Replay the same video until user taps
        videoPlayer.isLooping = true;

        PlayVideoAt(0);
    }

    void Update()
{
    // Mouse click (Editor/PC)
    if (Input.GetMouseButtonDown(0))
    {
        NextVideo();
    }

    // Touch (Android/iOS)
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        NextVideo();
    }
}

    // Hook this up to the Button's OnClick()
    public void NextVideo()
{
    if (videoClips == null || videoClips.Length == 0)
        return;

    currentIndex++;

    if (currentIndex >= videoClips.Length)
        currentIndex = 0;

    PlayVideoAt(currentIndex);
}



    private void PlayVideoAt(int index)
    {
        Debug.Log("▶ Now Playing: " + videoClips[index].name);

        videoPlayer.Stop();
        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();
    }
}