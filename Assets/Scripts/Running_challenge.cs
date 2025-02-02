using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Running_challenge : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;

    public float forwardSpeed;
    private int desiredLane = 1; // 0: left, 1: middle, 2: right
    public float laneDistance = 7; // Distance between lanes
    private float targetX; // Store the target x position
    public float laneChangeSpeed = 10f; // Speed of lane switching
    private float startX = 125.87f; // Fixed starting position
    public float jumpForce;
    public float gravity = -20;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Ensure the character starts exactly at x = 126.1
        Vector3 startPosition = transform.position;
        startPosition.x = startX;
        transform.position = startPosition;

        // Set the initial target X position to match starting X
        targetX = startX;
    }

    void Update()
    {
        direction.z = forwardSpeed; // Move forward automatically
        if (controller.isGrounded)
        {
            direction.y = 0;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Jump();
            }

        }
        else
        {
            direction.y += gravity * Time.deltaTime;
        }
        // Detect lane switch input
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            desiredLane = Mathf.Min(desiredLane + 1, 2); // Ensure max lane is 2
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            desiredLane = Mathf.Max(desiredLane - 1, 0); // Ensure min lane is 0
        }

        // Calculate new X position based on lane selection
        targetX = startX + (desiredLane - 1) * laneDistance;

        // Move character smoothly towards the new lane position
        Vector3 moveDirection = new Vector3(targetX - transform.position.x, 0, 0);
        controller.Move(moveDirection * laneChangeSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        controller.Move(direction * Time.fixedDeltaTime); // Move forward smoothly
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }
}
