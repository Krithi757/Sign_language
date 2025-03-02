using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class NewTestScript
{
    private GameObject gameObject;
    private Rigidbody2D rigidbody2D;
    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        gameObject = new GameObject();
        rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Simulate running movement (e.g., move to the right with speed 5)
        float speed = 5f;
        rigidbody2D.velocity = new Vector2(speed, 0); // Move to the right (positive x-axis)

        // Wait for a short period to check if the object is moving
        yield return new WaitForSeconds(1);

        // Check if the object is moving in the right direction (positive x-axis)
        Assert.Greater(rigidbody2D.position.x, 0, "Object is not moving in the right direction.");
    }
}
