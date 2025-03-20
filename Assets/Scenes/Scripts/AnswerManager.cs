using UnityEngine;
using System.Collections.Generic;

public class AnswerManager : MonoBehaviour
{
    public GameObject pipePrefab;  // Pipe prefab
    public Transform spawnPoint;   // Where pipes spawn
    public List<string> allAnswers; // List of possible answers
    public string correctAnswer;   // Correct answer for this round

    private void Start()
    {
        SpawnAnswers();
    }

    private void SpawnAnswers()
    {
        // Pick a random wrong answer that is NOT the correct one
        string wrongAnswer;
        do
        {
            wrongAnswer = allAnswers[Random.Range(0, allAnswers.Count)];
        }
        while (wrongAnswer == correctAnswer);

        // Randomly choose which pipe is correct (left or right)
        bool correctOnLeft = Random.value > 0.5f;

        // Spawn correct answer
        SpawnPipe(correctOnLeft ? correctAnswer : wrongAnswer, spawnPoint.position + Vector3.left * 2);

        // Spawn wrong answer
        SpawnPipe(correctOnLeft ? wrongAnswer : correctAnswer, spawnPoint.position + Vector3.right * 2);
    }

    private void SpawnPipe(string answerText, Vector3 position)
    {
        GameObject pipe = Instantiate(pipePrefab, position, Quaternion.identity);
        pipe.GetComponent<Pipes>().SetAnswer(answerText);
    }
}
