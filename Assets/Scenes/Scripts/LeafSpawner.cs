using UnityEngine;
using System.Collections;

public class LeafSpawner : MonoBehaviour
{
    public GameObject leafPrefab;
    public int numberOfLeaves = 40;  // Total leaves to spawn
    public float spawnWidth = 3f;    // Horizontal spread for leaves
    public float spawnHeight = 5f;   // Height above the camera view
    public float spawnDuration = 20f; // Duration for spawning leaves

    void Start()
    {
        StartCoroutine(SpawnLeavesForDuration()); // Start spawning leaves for a limited time
    }

    // Coroutine to spawn leaves for a specified duration
    IEnumerator SpawnLeavesForDuration()
    {
        float elapsedTime = 0f;
        int leavesSpawned = 0;  // Track how many leaves have been spawned

        // Loop to spawn exactly 'numberOfLeaves' leaves within the spawnDuration
        while (elapsedTime < spawnDuration && leavesSpawned < numberOfLeaves)
        {
            // Spawn one leaf at a time
            Vector3 spawnPos = new Vector3(
                Random.Range(-spawnWidth / 2, spawnWidth / 2),  // Random x position
                Camera.main.transform.position.y + spawnHeight, // Spawn above the camera view
                -1.43f  // Z position fixed for all leaves
            );

            // Instantiate the leaf prefab
            Instantiate(leafPrefab, spawnPos, Quaternion.identity);

            leavesSpawned++;  // Increment the leaf counter

            // Wait for a brief period before spawning the next leaf
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f)); // Adjust the delay as needed

            // Update elapsed time
            elapsedTime += Time.deltaTime;
        }
    }
}
