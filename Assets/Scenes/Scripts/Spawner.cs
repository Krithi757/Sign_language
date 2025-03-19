using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public float spawnRate = 4.5f; // Lower value for more frequent spawns
    public float minY = -3500f;  // Adjust for your UI scaling
    public float maxY = -100f;   // Adjust for your UI scaling

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
        // Instantiate the pipe prefab
        GameObject pipes = Instantiate(prefab, transform.position, Quaternion.identity, transform.parent);

        // Adjust Y position to fit large UI scaling
        RectTransform rt = pipes.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition += new Vector2(0, Random.Range(minY, maxY));
        }
        else
        {
            pipes.transform.position += Vector3.up * Random.Range(minY, maxY);
        }
    }
}
