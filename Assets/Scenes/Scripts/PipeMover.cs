using UnityEngine;

public class PipeMover : MonoBehaviour
{
    public float speed = 2f; // Speed at which the pipe moves left

    void Update()
    {
        // Move the pipe to the left
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Destroy the pipe when it goes off-screen
        if (transform.position.x < -10f)
        {
            Destroy(gameObject);
        }
    }
}
