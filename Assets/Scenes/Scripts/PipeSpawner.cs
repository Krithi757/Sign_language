using System.Collections;
using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePrefab; // Assign the pipe prefab in the Inspector
    public float spawnRate = 2f;  // Time between spawns
    public float minHeight = -2f; // Minimum Y position
    public float maxHeight = 2f;  // Maximum Y position
    public float pipeSpeed = 2f;  // Speed at which pipes move left

    private void Start()
    {
        StartCoroutine(SpawnPipes());
    }

    IEnumerator SpawnPipes()
    {
        while (true)
        {
            SpawnPipe();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    void SpawnPipe()
    {
        // Generate a random height for the pipes
        float randomY = Random.Range(minHeight, maxHeight);

        // Spawn the pipe at the right edge of the screen
        GameObject newPipe = Instantiate(pipePrefab, new Vector3(10f, randomY, 0), Quaternion.identity);

        // Assign a movement script to the pipe
        newPipe.AddComponent<PipeMover>().speed = pipeSpeed;
    }
}
