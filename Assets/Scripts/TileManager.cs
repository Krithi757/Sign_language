using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Video; // Import Video namespace

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public float zSpawn = 0;
    public float tileLength = 45.4f;
    public int numberOfTiles = 5;
    public Transform playerTransform;
    private List<GameObject> activeTiles = new List<GameObject>();

    private List<string> originalVideoNames = new List<string>(); // Original list of video names
    private List<string> availableVideoNames = new List<string>(); // Names available for assignment
    private List<string> videoPaths = new List<string>(); // Store video paths for playback

    public VideoPlayer videoPlayer; // Attach VideoPlayer in the Inspector 
    private string currentVideoName;
    private int currentVideoIndex = 0;

    void Start()
    {
        // Get video paths and store both names and paths
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();
        originalVideoNames = videoData.Values.ToList();
        videoPaths = videoData.Keys.ToList(); // Keys are the video paths

        // Shuffle and initialize the available names
        ShuffleAndResetVideoNames();

        // Start playing the first video
        PlayRandomVideo();

        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i == 0)
                SpawnTile(0);
            else
                SpawnTile(Random.Range(0, tilePrefabs.Length));
        }
    }

    void Update()
    {
        if (playerTransform.position.z - 50 > zSpawn - (numberOfTiles * tileLength))
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
            DeleteTile();
        }
    }

    public void SpawnTile(int tileIndex)
    {
        Vector3 originalPosition = tilePrefabs[tileIndex].transform.position;
        Vector3 spawnPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z + zSpawn);

        GameObject go = Instantiate(tilePrefabs[tileIndex], spawnPosition, tilePrefabs[tileIndex].transform.rotation);
        activeTiles.Add(go);

        AssignVideoNames(go);

        zSpawn += tileLength;
    }

    private void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }

    private void AssignVideoNames(GameObject tile)
    {
        TextMeshPro[] textComponents = tile.GetComponentsInChildren<TextMeshPro>();

        foreach (TextMeshPro tmp in textComponents)
        {
            if (availableVideoNames.Count == 0)
            {
                ShuffleAndResetVideoNames(); // Reshuffle when all names are used
            }

            tmp.text = availableVideoNames[0]; // Assign the first available name
            availableVideoNames.RemoveAt(0);   // Remove the assigned name
        }
    }

    private void ShuffleAndResetVideoNames()
    {
        availableVideoNames = originalVideoNames.OrderBy(x => Random.value).ToList();
    }

    // 🎥 Video Playback Section
    private void PlayRandomVideo()
    {
        if (videoPaths.Count == 0) return; // No videos to play

        // Select a random video name (with file extension)
        string randomVideoName = videoPaths[Random.Range(0, originalVideoNames.Count)];
        Debug.Log("Playing video name " + currentVideoName);
        currentVideoName = randomVideoName;

        // Load the video from the Resources folder using the full path from the dictionary
        VideoClip videoClip = Resources.Load<VideoClip>(randomVideoName);

        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.isLooping = true;

            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning($"Video not found: {randomVideoName}");
        }
    }




    private void OnVideoFinished(VideoPlayer vp)
    {
        PlayRandomVideo(); // Play the next random video
    }



    public string GetCurrentVideoName()
    {
        return currentVideoName;
    }

    public string GetCurrentVideoValue()
    {
        Dictionary<string, string> videoData = VideoPathManager.GetVideoPaths();

        if (videoData.ContainsKey(currentVideoName))
        {
            Debug.Log("Video Value: " + videoData[currentVideoName]);
            return videoData[currentVideoName];
        }
        else
        {
            Debug.LogWarning("Key not found: " + currentVideoName);
            return null;
        }
    }

    public void PlayNextVideo()
    {
        currentVideoIndex++;
        if (currentVideoIndex < videoPaths.Count)
        {
            PlayRandomVideo();
            Debug.Log("Changed video due to yay " + GetCurrentVideoName());
        }
        else
        {
            Debug.Log("🎉 All videos played!");
        }
    }

}
