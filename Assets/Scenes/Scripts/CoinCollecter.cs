using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CoinCollector : MonoBehaviour
{
    public GameObject coinPrefab;          // Coin prefab
    public Transform spawnPoint;           // Fixed spawn position
    public Transform target;               // Target position
    public float floatHeight = 2f;         // Height coin floats before moving
    public float duration = 1.5f;          // Total animation time
    public int numberOfCoins = 5;          // Number of coins to spawn
    public float spawnInterval = 0.2f;     // Delay between each coin spawn
    public float rotationSpeed = 360f;     // Speed of rotation (degrees per second)

    private bool _isFinished = true;
    public bool IsFinished => _isFinished;

    public void CollectCoins()
    {
        if (!_isFinished) return; // Prevent multiple triggers while running
        StartCoroutine(SpawnCoins());
    }

    private IEnumerator SpawnCoins()
    {
        _isFinished = false; // Mark as in progress

        for (int i = 0; i < numberOfCoins; i++)
        {
            SpawnCoin();
            yield return new WaitForSeconds(spawnInterval);
        }

        // Wait for the last coin's animation to complete
        yield return new WaitForSeconds(duration);

        _isFinished = true; // Mark as finished
    }

    private void SpawnCoin()
    {
        // Ensure that the coin has a fixed z-position of -0.25 during instantiation
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, -0.25f);

        // Instantiate the coin at the new spawn position with the same rotation as the prefab
        GameObject coin = Instantiate(coinPrefab, spawnPosition, coinPrefab.transform.rotation);

        // Calculate the midpoint of the coin's floating path
        Vector3 midPoint = spawnPosition + Vector3.up * floatHeight;

        // Start a DOTween sequence for the coin's movement and rotation
        Sequence coinSequence = DOTween.Sequence();

        // Floating with rotation while ensuring the z-position remains -0.25
        coinSequence.Append(coin.transform.DOMove(new Vector3(midPoint.x, midPoint.y, -0.25f), 0.5f).SetEase(Ease.OutQuad))  // Move up
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), 0.5f, RotateMode.FastBeyond360)) // Rotate while floating

                    // Move to the target position while still rotating, maintaining z = -0.25
                    .Append(coin.transform.DOMove(new Vector3(target.position.x, target.position.y, -0.25f), duration - 0.5f).SetEase(Ease.InQuad))
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), duration - 0.5f, RotateMode.FastBeyond360)) // Rotate while moving

                    // Final rotation to lock coin at a 45-degree angle (fixed position)
                    .Append(coin.transform.DORotate(new Vector3(45, 45, 0), 0.2f)) // Fixed 45-degree rotation

                    .OnComplete(() => Destroy(coin));  // Destroy coin after animation is complete
    }
}
