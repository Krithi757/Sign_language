using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class Running_challenge : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;

    public float forwardSpeed;
    private int desiredLane = 1;
    public float laneDistance = 7;
    private float targetX;
    public float laneChangeSpeed = 10f;
    private float startX = 125.87f;
    public float jumpForce;
    public float gravity = -70f;

    public GameObject videoObject;
    public Vector3 videoOffset = new Vector3(0, 5, -10);

    private float videoPosX = 126.1f;
    private float videoPosY = 15.7f;
    private float videoPosZ;

    private float lerpSpeed = 0.1f;

    public float jumpCooldown = 0.1f;
    private float lastJumpTime = -1f;
    private bool jumpRequested = false; // For jump buffering

    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0, 5, -10);
    public float cameraFollowDelay = 0.5f;

    private Vector3 cameraVelocity = Vector3.zero;
    private float followTimer = 0f;
    private TileManager tileManager;
    private string currentVideoName;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Vector3 startPosition = transform.position;
        startPosition.x = startX;
        startPosition.y = 8.96f;
        transform.position = startPosition;

        targetX = startX;
        tileManager = FindObjectOfType<TileManager>();

        if (tileManager != null && tileManager.videoPlayer != null)
        {
            currentVideoName = tileManager.videoPlayer.clip.name;
        }

        if (videoObject != null)
        {
            videoPosZ = videoObject.transform.position.z;
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
        direction.z = forwardSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true; // Buffer the jump request
        }

        if (controller.isGrounded)
        {
            direction.y = 0;

            if (jumpRequested && Time.time - lastJumpTime >= jumpCooldown)
            {
                Jump();
                lastJumpTime = Time.time;
                jumpRequested = false; // Reset the jump buffer
            }
        }
        else
        {
            direction.y += gravity * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            desiredLane = Mathf.Min(desiredLane + 1, 2);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            desiredLane = Mathf.Max(desiredLane - 1, 0);
        }

        targetX = startX + (desiredLane - 1) * laneDistance;
        Vector3 moveDirection = new Vector3(targetX - transform.position.x, 0, 0);
        controller.Move(moveDirection * laneChangeSpeed * Time.deltaTime);

        if (videoObject != null)
        {
            videoPosZ += forwardSpeed * Time.deltaTime;
            videoPosX = transform.position.x;
            videoObject.transform.position = new Vector3(videoPosX, videoPosY, videoPosZ);
        }

        followTimer += Time.deltaTime;

        if (followTimer >= cameraFollowDelay)
        {
            if (mainCamera != null)
            {
                Vector3 targetCameraPosition = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z - 10f);
                mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetCameraPosition, ref cameraVelocity, lerpSpeed);
                mainCamera.transform.LookAt(transform);
            }
        }

        controller.center = new Vector3(0, controller.height / 2, 0.1f);
    }

    void FixedUpdate()
    {
        controller.Move(direction * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Collided with: " + hit.collider.gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TextMeshPro textMeshPro = other.GetComponent<TextMeshPro>();

        if (textMeshPro != null && tileManager != null)
        {
            string currentVideoValue = tileManager.GetCurrentVideoValue();

            if (currentVideoValue != null && textMeshPro.text == currentVideoValue)
            {
                tileManager.PlayNextVideo();
            }
        }
    }
}
