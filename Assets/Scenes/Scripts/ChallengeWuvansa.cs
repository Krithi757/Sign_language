using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class FallingWordsChallenge : MonoBehaviour
{
    public GameObject wordPrefab; // Assign a prefab for falling words
    public Transform spawnPoint;  // Spawn position of words
    public float fallSpeed = 3f;  // Falling speed of words
    public float spawnInterval = 2f; // Interval between word spawns
    public int maxMisses = 5; // Max missed words before game over

    public List<string> wordList = new List<string>(); // Words to match
    public List<string> correctWords = new List<string>(); // Correct words for each video
    public List<VideoPlayer> videoPlayers; // Assign SSL videos
    public List<Transform> dropZones; // Drop zones matching videos

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public GameObject gameOverPanel;

    private int score = 0;
    private int lives;
    private float spawnTimer;

    void Start()
    {
        lives = maxMisses;
        gameOverPanel.SetActive(false);
        StartCoroutine(SpawnWords());
        UpdateUI();
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
    }

    IEnumerator SpawnWords()
    {
        while (lives > 0)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWord();
        }
    }

    void SpawnWord()
    {
        string randomWord = wordList[Random.Range(0, wordList.Count)];
        GameObject wordObj = Instantiate(wordPrefab, spawnPoint.position, Quaternion.identity);
        wordObj.GetComponent<Word>().Initialize(randomWord, this);
    }

    public void CheckWordMatch(string word, Transform dropZone)
    {
        int index = dropZones.IndexOf(dropZone);
        if (index != -1 && correctWords[index] == word)
        {
            score += 10;
        }
        else
        {
            lives -= 1;
        }

        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        livesText.text = "Lives: " + lives;
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
    }
}
