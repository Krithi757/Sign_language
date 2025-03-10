using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSLChallengeGame : MonoBehaviour
{
    [Header("Gameplay Elements")]
    public GameObject wordPrefab; // Prefab for the falling word blocks
    public Transform spawnPoint;  // Point where words spawn
    public Transform[] videoTargets; // Video slots where words should be dropped
    public float spawnInterval = 2f; // Time between spawning word blocks
    public float fallSpeed = 2f; // Speed at which blocks fall

    [Header("UI Elements")]
    public Text scoreText;
    public int score = 0;

    private bool isDragging = false;
    private GameObject draggedBlock;

    void Start()
    {
        StartCoroutine(SpawnWords());
    }

    void Update()
    {
        HandleDrag();
    }

    IEnumerator SpawnWords()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWordBlock();
        }
    }

    void SpawnWordBlock()
    {
        GameObject wordBlock = Instantiate(wordPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = wordBlock.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(0, -fallSpeed);
        wordBlock.GetComponent<WordBlock>().AssignRandomWord();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("WordBlock"))
            {
                draggedBlock = hit.collider.gameObject;
                isDragging = true;
            }
        }

        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            draggedBlock.transform.position = mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CheckDropLocation(draggedBlock);
        }
    }

    void CheckDropLocation(GameObject block)
    {
        foreach (Transform target in videoTargets)
        {
            if (Vector2.Distance(block.transform.position, target.position) < 1f) // Adjust for better precision
            {
                string wordOnBlock = block.GetComponent<WordBlock>().wordText;
                string expectedWord = target.GetComponent<VideoSlot>().expectedWord;

                if (wordOnBlock == expectedWord)
                {
                    score += 10;
                    UpdateScore();
                }
                else
                {
                    score -= 5;
                    UpdateScore();
                }
                Destroy(block);
                return;
            }
        }
        Destroy(block); // If dropped in wrong place, destroy block
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}

public class WordBlock : MonoBehaviour
{
    public Text displayText;
    public string wordText;

    private string[] sslWords = { "apple", "book", "cat", "dog" }; // Example SSL words

    public void AssignRandomWord()
    {
        wordText = sslWords[Random.Range(0, sslWords.Length)];
        displayText.text = wordText;
    }
}

public class VideoSlot : MonoBehaviour
{
    public string expectedWord;
}
