using UnityEngine;
using UnityEngine.Video;

// Lives on "VideoScreen" (same object as the Raw Image / Video Player reference).
//
// What changed from before: tapping/clicking no longer advances the video.
// The video now only advances when a word gets dropped onto it - that logic
// lives in VideoDropTarget.cs, which calls NextVideo() below. This script's
// only job now is: load this level's videos, play them, and tell anyone who's
// listening (WordRoundManager) whenever the current video changes.
public class TapToChangeVideo : MonoBehaviour
{
    [Tooltip("Drag the GameObject that has the VideoPlayer component here.")]
    public VideoPlayer videoPlayer;

    [Tooltip("Used only if PlayerPrefs has no SelectedLevelId yet.")]
    public int fallbackLevelId = 1;
    private VideoClip[] videoClips;
    private int currentIndex = 0;

    // Read-only access for other scripts (WordRoundManager needs the full list
    // to build the word pool; VideoDropTarget/others can check what's playing now).
    public VideoClip[] VideoClips => videoClips;
    public int CurrentIndex => currentIndex;
    public VideoClip CurrentClip => (videoClips != null && videoClips.Length > 0) ? videoClips[currentIndex] : null;

    // Fired every time a new video starts (including the very first one).
    // WordRoundManager subscribes to this to know when to put up a fresh set of words.
    public event System.Action<VideoClip> OnVideoChanged;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("❌ No VideoPlayer assigned!");
            return;
        }
        int levelId = PlayerPrefs.GetInt("SelectedLevelId", fallbackLevelId);

        string folderPath = GetFolderPath(levelId);

        Debug.Log("=======================================");
        Debug.Log("Selected Level : " + levelId);
        Debug.Log("Loading videos from Resources/" + folderPath);

        videoClips = Resources.LoadAll<VideoClip>(folderPath);

        Debug.Log("Videos Found: " + videoClips.Length);

        if (videoClips.Length == 0)
        {
            Debug.LogError("❌ No videos found!");
            Debug.LogError("Expected location: Assets/.../Resources/" + folderPath + "/");
            return;
        }

        Debug.Log("----------- Loaded Videos -----------");
        for (int i = 0; i < videoClips.Length; i++)
        {
            Debug.Log((i + 1) + ". " + videoClips[i].name + "  (word: " + GetWordFromClipName(videoClips[i].name) + ")");
        }
        Debug.Log("-------------------------------------");

        videoPlayer.isLooping = true;
        PlayVideoAt(0);
    }

    // Call this to advance to the next video. Wraps back to the first after the last.
    // Now called by VideoDropTarget after a word is dropped - not by tapping the screen.
    public void NextVideo()
    {
        if (videoClips == null || videoClips.Length == 0) return;

        currentIndex++;
        if (currentIndex >= videoClips.Length) currentIndex = 0;

        PlayVideoAt(currentIndex);
    }

    private string GetFolderPath(int levelId)
    {
        switch (levelId)
        {
            case 1: return "Sample";
            case 2: return "Level2";
            case 3: return "Level3";
            case 4: return "Level4";
            case 5: return "Level5";
            case 6: return "Level6";
            case 7: return "Level7";
            case 8: return "Level8";
            case 9: return "Level9";
            case 10: return "Level10";
            default: return "Sample";
        }
    }

    private void PlayVideoAt(int index)
    {
        Debug.Log("▶ Now Playing: " + videoClips[index].name);

        videoPlayer.Stop();
        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();

        OnVideoChanged?.Invoke(videoClips[index]);
    }

    // Turns a clip file name like "Beautiful_002" into the displayed word "Beautiful".
    // Strips a trailing underscore + number, if there is one. If a clip is just
    // named "Onion" with no number suffix, it's returned unchanged.
    public static string GetWordFromClipName(string clipName)
    {
        return System.Text.RegularExpressions.Regex.Replace(clipName, @"_\d+$", "");
    }
}
