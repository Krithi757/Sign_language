using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public float zSpawn = 0;
    public float tileLength = 45.4f;
    public int numberOfTiles = 5;
    public Transform playerTransform;
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
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

        // Get the original position of the prefab
        Vector3 originalPosition = tilePrefabs[tileIndex].transform.position;

        // Retain the original x, y and update only the z-axis
        Vector3 spawnPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z + zSpawn);

        // Instantiate the prefab at the new position
        GameObject go = Instantiate(tilePrefabs[tileIndex], spawnPosition, tilePrefabs[tileIndex].transform.rotation);
        activeTiles.Add(go);

        // Increment zSpawn for the next tile
        zSpawn += tileLength;
    }

    private void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}
