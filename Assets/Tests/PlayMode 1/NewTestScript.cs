using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    private GameObject gameObject;
    private CharacterController characterController;
    private Vector3 moveDirection;

    // A Setup method to initialize the GameObject and CharacterController
    [SetUp]
    public void Setup()
    {
        gameObject = new GameObject();
        characterController = gameObject.AddComponent<CharacterController>(); // Adding the built-in CharacterController component
    }

    // UnityTest to check if the object moves in the right direction when running
    [UnityTest]
    public IEnumerator ObjectMovesInRightDirectionWhenRunning()
    {
        float speed = 5f;
        moveDirection = new Vector3(speed, 0, 0); // Move to the right (positive x-axis)

        // Simulate movement using CharacterController
        characterController.Move(moveDirection * Time.deltaTime);

        // Wait for a short period to allow movement
        yield return new WaitForSeconds(1);

        // Check if the object has moved in the positive x-direction
        Assert.Greater(gameObject.transform.position.x, 0, "Object is not moving in the right direction.");
    }

    // UnityTest to check if the object jumps correctly
    [UnityTest]
    public IEnumerator ObjectJumpsCorrectly()
    {
        // Reset position and velocity before the jump
        gameObject.transform.position = Vector3.zero;

        float jumpForce = 10f;
        moveDirection = new Vector3(0, jumpForce, 0); // Add upward movement

        // Simulate the jump using CharacterController
        characterController.Move(moveDirection * Time.deltaTime);

        // Wait for a short period to let the object move upwards
        yield return new WaitForSeconds(0.5f);

        // Check if the object has moved up (positive y-direction)
        Assert.Greater(gameObject.transform.position.y, 0, "Object did not jump.");
    }

    // UnityTest to check if the object lands after jumping
    [UnityTest]
    public IEnumerator ObjectLandsAfterJump()
    {
        // Reset position and velocity before the jump
        gameObject.transform.position = Vector3.zero;

        float jumpForce = 10f;
        moveDirection = new Vector3(0, jumpForce, 0); // Add upward movement

        // Simulate the jump using CharacterController
        characterController.Move(moveDirection * Time.deltaTime);

        // Wait for the object to reach its peak and start falling
        yield return new WaitForSeconds(0.5f);

        // Add gravity or downward force for the landing
        moveDirection = new Vector3(0, -jumpForce, 0); // Simulating gravity effect

        // Simulate the landing with downward movement
        characterController.Move(moveDirection * Time.deltaTime);

        // Wait for the object to land
        yield return new WaitForSeconds(0.5f);

        // Check if the object is back on the ground (y position should return to 0)
        Assert.Less(gameObject.transform.position.y, 1, "Object did not land correctly.");
    }
}
