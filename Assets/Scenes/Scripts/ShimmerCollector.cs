using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ShimmerCollector : MonoBehaviour
{
    public GameObject coinPrefab;           // The coin prefab (3D model of the coin)
    public Transform target;                // The target (coin label or score panel)
    public float floatHeight = 2f;          // How high the coin floats before moving
    public float duration = 1.5f;           // Total animation time
    public int numberOfCoins = 5;           // Number of coins to spawn
    public float spawnInterval = 0.2f;      // Delay between each coin spawn
    public float rotationSpeed = 360f;      // Speed of rotation (degrees per second)

    // Method to trigger the shimmer animation from dragged object position
    public void CollectShimmer(Transform draggedObjectTransform)
    {
        StartCoroutine(SpawnCoins(draggedObjectTransform));
    }

    private IEnumerator SpawnCoins(Transform draggedObjectTransform)
    {
        for (int i = 0; i < numberOfCoins; i++)
        {
            SpawnCoin(draggedObjectTransform.position);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCoin(Vector3 spawnPosition)
    {
        GameObject coin = Instantiate(coinPrefab, spawnPosition, coinPrefab.transform.rotation);

        Vector3 midPoint = spawnPosition + Vector3.up * floatHeight;

        Sequence coinSequence = DOTween.Sequence();

        // Coin animation: float, rotate, and move to target
        coinSequence.Append(coin.transform.DOMove(midPoint, 0.5f).SetEase(Ease.OutQuad))
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), 0.5f, RotateMode.FastBeyond360))
                    .Append(coin.transform.DOMove(target.position, duration - 0.5f).SetEase(Ease.InQuad))
                    .Join(coin.transform.DORotate(new Vector3(0, rotationSpeed, 0), duration - 0.5f, RotateMode.FastBeyond360))
                    .OnComplete(() => Destroy(coin));  // Destroy after animation
    }
}
