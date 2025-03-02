using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    private GameObject gameObject;
    private Rigidbody2D rigidbody2D;

    // A Setup method to initialize the GameObject and Rigidbody2D
    [SetUp]
    public void Setup()
    {
        gameObject = new GameObject();
        rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
    }

    // UnityTest to check if the object moves in the right direction when running
    [UnityTest]
    public IEnumerator ObjectMovesInRightDirectionWhenRunning()
    {
        float speed = 5f;
        rigidbody2D.velocity = new Vector2(speed, 0); // Move to the right (positive x-axis)

        // Wait for a short period to allow movement
        yield return new WaitForSeconds(1);

        // Check if the object has moved in the positive x-direction
        Assert.Greater(rigidbody2D.position.x, 0, "Object is not moving in the right direction.");
    }

    [UnityTest]
    public IEnumerator ObjectJumpsCorrectly()
    {
        // Set initial velocity to zero
        rigidbody2D.velocity = Vector2.zero;

        // Apply a force to make the object jump
        float jumpForce = 10f;
        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Wait for a short period to let the object move
        yield return new WaitForSeconds(0.5f);

        // Check if the object has moved up (positive y-direction)
        Assert.Greater(rigidbody2D.position.y, 0, "Object did not jump.");
    }

    [UnityTest]
    public IEnumerator ObjectLandsAfterJump()
    {
        // Set initial velocity to zero
        rigidbody2D.velocity = Vector2.zero;

        // Apply a force to make the object jump
        float jumpForce = 10f;
        rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Wait for the object to reach its peak and start falling
        yield return new WaitForSeconds(0.5f);

        // Now let it land
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.AddForce(Vector2.down * jumpForce, ForceMode2D.Impulse);

        // Wait for the object to land
        yield return new WaitForSeconds(0.5f);

        // Check if the object is back on the ground (y position should return to 0)
        Assert.Less(rigidbody2D.position.y, 1, "Object did not land correctly.");
    }


}
