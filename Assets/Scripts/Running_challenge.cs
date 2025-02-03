using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;  // Import Video namespace

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

    public GameObject videoObject; // Reference to the GameObject holding the Quad for the VideoPlayer
    public Vector3 videoOffset = new Vector3(0, 5, -10); // Optional offset for video position

    private float videoPosX = 126.1f;  // Constant X position for the video
    private float videoPosY = 16.75f;  // Constant Y position for the video
    private float videoPosZ;  // Store the Z position of the video

    private float lerpSpeed = 0.1f; // Speed of the interpolation for video movement

    public float jumpCooldown = 0.5f;  // Cooldown time between jumps
    private float lastJumpTime = -1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Ensure the character starts exactly at x = 126.1
        Vector3 startPosition = transform.position;
        startPosition.x = startX;
        transform.position = startPosition;

        // Set the initial target X position to match starting X
        targetX = startX;

        // Ensure the video object starts at a fixed position
        if (videoObject != null)
        {
            videoPosZ = videoObject.transform.position.z;

            // Start the video
            VideoPlayer videoPlayer = videoObject.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.Play();
                videoPlayer.isLooping = true;
            }
        }
    }

    void Update()
    {
        // Ensure the forward speed is positive for movement in the correct direction
        direction.z = forwardSpeed; // Player moves forward automatically

        if (controller.isGrounded)
        {
            // Reset vertical velocity when grounded
            direction.y = 0;

            // Handle jump input only when grounded and after cooldown
            if (Input.GetKey(KeyCode.UpArrow) && Time.time - lastJumpTime >= jumpCooldown)
            {
                Jump();
                lastJumpTime = Time.time;  // Update last jump time
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

        // Update the position of the video object along the Z-axis based on the player's forward speed
        if (videoObject != null)
        {
            // Simply add the player's forward speed to the video object's Z position
            videoPosZ += forwardSpeed * Time.deltaTime;

            // Update the video's position with no changes to X or Y
            videoObject.transform.position = new Vector3(videoPosX, videoPosY, videoPosZ);
        }
    }
    void FixedUpdate()
    {
        controller.Move(direction * Time.fixedDeltaTime); // Move forward smoothly
    }

    private void Jump()
    {
        // Apply upward force for jump
        direction.y = jumpForce;
    }
}
