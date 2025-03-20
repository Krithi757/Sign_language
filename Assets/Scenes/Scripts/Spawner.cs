using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Spawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public float spawnRate = 4.5f;
    public float minY = -3500f;
    public float maxY = -100f;

    private Dictionary<string, string> videoData; // Dictionary containing video paths as keys and words as values
    private string currentCorrectAnswer; // Store the current correct answer

    private void Start()
    {
        // Load dictionary from VideoPathManager
        videoData = VideoPathManager.GetVideoPaths();
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(Spawn), 2f, spawnRate);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    private void Spawn()
    {
        if (videoData.Count == 0)
        {
            Debug.LogError("No video paths found!");
            return;
        }

        List<string> keys = new List<string>(videoData.Keys);
        int correctIndex = Random.Range(0, keys.Count);
        string correctVideoPath = keys[correctIndex];
        currentCorrectAnswer = videoData[correctVideoPath]; // Get the correct word

        string incorrectAnswer;
        do
        {
            int randomIndex = Random.Range(0, keys.Count);
            incorrectAnswer = videoData[keys[randomIndex]];
        } while (incorrectAnswer == currentCorrectAnswer);

        // Spawn pipe with the correct and incorrect answers
        GameObject pipePair = Instantiate(pipePrefab, transform.position, Quaternion.identity, transform.parent);
        pipePair.transform.position += Vector3.up * Random.Range(minY, maxY);

        bool isCorrectPipe = Random.value > 0.5f; // Randomly decide which pipe gets the correct answer
        string assignedAnswer = isCorrectPipe ? currentCorrectAnswer : incorrectAnswer;

        pipePair.GetComponent<Pipes>().SetAnswer(assignedAnswer);

        Debug.Log($"Pipe spawned. Assigned Answer: {assignedAnswer} (Correct: {isCorrectPipe})"); // Debugging info
    }

}
