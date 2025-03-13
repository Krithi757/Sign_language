using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Linq;

public class MultiVideoPlayer : MonoBehaviour
{
    public VideoPlayer[] videoPlayers;
    public RawImage[] rawImages;
    public Text[] videoInfoTexts;

    private List<string> videoPaths;
    private List<string> videoTexts;
    private int[] currentVideoIndices;

    void Start()
    {
        if (videoPlayers.Length != rawImages.Length || videoPlayers.Length != videoInfoTexts.Length)
        {
            Debug.LogError("Mismatch in the number of VideoPlayers, RawImages, or Text UI elements!");
            return;
        }

        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();
        videoPaths = videoData.Keys.ToList();
        videoTexts = videoData.Values.ToList();

        if (videoPaths.Count == 0)
        {
            Debug.LogError("No video paths found from VideoPathManager!");
            return;
        }

        ShuffleVideos();
        currentVideoIndices = new int[videoPlayers.Length];
        
        for (int i = 0; i < videoPlayers.Length; i++)
        {
            rawImages[i].texture = videoPlayers[i].targetTexture;
            videoPlayers[i].loopPointReached += OnVideoEnd;
            PlayVideo(i);
        }
    }

    void ShuffleVideos()
    {
        for (int i = 0; i < videoPaths.Count; i++)
        {
            int randomIndex = Random.Range(i, videoPaths.Count);
            (videoPaths[i], videoPaths[randomIndex]) = (videoPaths[randomIndex], videoPaths[i]);
            (videoTexts[i], videoTexts[randomIndex]) = (videoTexts[randomIndex], videoTexts[i]);
        }
    }

    void PlayVideo(int playerIndex)
    {
        if (videoPaths.Count == 0)
            return;

        int videoIndex = currentVideoIndices[playerIndex];
        string selectedVideoPath = videoPaths[videoIndex];
        string selectedText = videoTexts[videoIndex];

        VideoClip videoClip = Resources.Load<VideoClip>(selectedVideoPath);
        if (videoClip == null)
        {
            Debug.LogError("Video clip not found at path: " + selectedVideoPath);
            return;
        }

        videoPlayers[playerIndex].clip = videoClip;
        videoPlayers[playerIndex].Play();
        
        if (videoInfoTexts[playerIndex] != null)
        {
            videoInfoTexts[playerIndex].text = selectedText;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        int playerIndex = System.Array.IndexOf(videoPlayers, vp);
        if (playerIndex == -1) return;

        currentVideoIndices[playerIndex] = (currentVideoIndices[playerIndex] + 1) % videoPaths.Count;
        PlayVideo(playerIndex);
    }
}