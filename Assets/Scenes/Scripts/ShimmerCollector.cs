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
        StartCoroutine(SpawnCoins());
    }

    public void CollectShimmer()
    {
        StartCoroutine(SpawnCoins());
    }

    private IEnumerator SpawnCoins()
    {
        for (int i = 0; i < 10; i++)
        {
            SpawnCoin();
            yield return new WaitForSeconds(spawnInterval);  // Proper delay
        }
    }

    private void SpawnCoin()
    {
        // Instantiate the coin while keeping its original prefab rotation
        GameObject coin = Instantiate(coinPrefab, spawnPoint.position, coinPrefab.transform.rotation);

        Vector3 midPoint = spawnPoint.position + Vector3.up * floatHeight;

        Sequence coinSequence = DOTween.Sequence();

        // Do NOT set any explicit rotation; let it use the prefab's original rotation
        coinSequence.Append(coin.transform.DOMove(midPoint, 0.5f).SetEase(Ease.OutQuad))   // Float upward
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), 0.5f, RotateMode.FastBeyond360)) // Rotate while floating
                    .Append(coin.transform.DOMove(target.position, duration - 0.5f).SetEase(Ease.InQuad))  // Fly to target
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), duration - 0.5f, RotateMode.FastBeyond360)) // Rotate while moving
                    .OnComplete(() => Destroy(coin));  // Destroy after animation
    }


}
