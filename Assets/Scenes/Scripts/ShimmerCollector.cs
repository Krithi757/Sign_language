using UnityEngine;
using DG.Tweening;  // Ensure DOTween is included
using System.Collections;

public class ShimmerCollector : MonoBehaviour
{
    public GameObject coinPrefab;           // The coin prefab (3D model of the coin)
    public Transform spawnPoint;            // Where the coin starts
    public Transform target;                // The target (coin label or score panel)
    public float floatHeight = 2f;          // How high the coin floats before moving
    public float duration = 1.5f;           // Total animation time
    public int numberOfCoins = 5;           // Number of coins to spawn
    public float spawnInterval = 0.2f;      // Delay between each coin spawn
    public float rotationSpeed = 360f;      // Speed of rotation (degrees per second)

    // Method to trigger the coin animation
    public void CollectCoins()
    {
        // Start a coroutine that will handle all coin spawns with delay
        StartCoroutine(SpawnCoins());
    }

    private IEnumerator SpawnCoins()
    {
        // Loop to create multiple coins with delay
        for (int i = 0; i < numberOfCoins; i++)
        {
            // Delay each spawn by the spawnInterval
            yield return new WaitForSeconds(i * spawnInterval);

            // Spawn the coin
            SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        // Instantiate the coin at the spawn point
        GameObject coin = Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity);

        // Define the midpoint where the coin will float upwards before heading to the target
        Vector3 midPoint = spawnPoint.position + Vector3.up * floatHeight;

        // Create a sequence for smooth animation: float up → fly to target → vanish
        Sequence coinSequence = DOTween.Sequence();

        // Add animations to the sequence
        coinSequence.Append(coin.transform.DOMove(midPoint, 0.5f).SetEase(Ease.OutQuad))   // Float upward
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), 0.5f, RotateMode.FastBeyond360)) // Rotate while floating
                    .Append(coin.transform.DOMove(target.position, duration - 0.5f).SetEase(Ease.InQuad))  // Fly towards the target
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), duration - 0.5f, RotateMode.FastBeyond360)) // Rotate while moving to the target
                    .OnComplete(() => Destroy(coin));  // Destroy the coin once animation is complete
    }
}
