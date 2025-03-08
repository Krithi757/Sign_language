using UnityEngine;
using System.Collections;  // Add this import for IEnumerator

public class LeafFall : MonoBehaviour
{
    public float fallSpeed = 1.5f;  // Speed of falling
    private float randomFallDelay;   // Time delay before falling starts

    void Start()
    {
        randomFallDelay = Random.Range(1f, 3f); // Random delay before falling starts
        StartCoroutine(FallAfterDelay()); // Start the falling after the random delay
    }

    // Coroutine to handle the delay before the leaf starts falling
    IEnumerator FallAfterDelay()
    {
        yield return new WaitForSeconds(randomFallDelay); // Wait for the random delay

        // Start falling after the delay
        while (transform.position.y > -5f) // Stop when it reaches the ground (adjust as needed)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - fallSpeed * Time.deltaTime, -1.43f);
            yield return null;
        }
    }
}
