using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelData : MonoBehaviour
{
    public int levelId;
    private RectTransform rectTransform;

    // Instead of a modePanel, we now have an array of 3 buttons.
    public GameObject[] levelButtons; // Assign your 3 button objects in the Inspector.

    public GameObject[] otherLevelObjects; // Array of other level objects to deactivate

    private Camera mainCamera;
    public float moveSpeed = 3.0f; // Adjust speed for smoother transition

    // Static variable to track which level is currently centered (or last moved to)
    private static int currentCameraLevelId = -1;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // Initially disable the buttons.
        foreach (GameObject btn in levelButtons)
        {
            btn.SetActive(false);
        }
        mainCamera = Camera.main; // Get main camera reference
    }

    void Update()
    {
        if (Input.touchCount > 0) // Check if there's at least one touch
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            if (touch.phase == TouchPhase.Began) // Only check on touch start
            {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform) // Check if this object was touched
                    {
                        OnLevelTapped();
                    }
                }
            }
        }
    }

    public void OnLevelTapped()
    {
        Debug.Log($"Level {levelId} tapped!");
        PlayerPrefs.SetInt("SelectedLevelId", levelId);
        PlayerPrefs.Save();

        // Show the buttons with sequential pop animations.
        StartCoroutine(ShowButtonsCoroutine());

        // Only move the camera if tapping a level different from the current one.
        if (currentCameraLevelId == levelId)
        {
            Debug.Log("Camera already centered on this level. Not moving.");
            return;
        }
        currentCameraLevelId = levelId;

        // Deactivate other level objects if needed.
        DeactivateOtherLevels(true);

        // Move camera towards the tapped level.
        StartCoroutine(MoveCameraToPosition(transform.position));
    }

    public void DeactivateOtherLevels(bool deactivate)
    {
        foreach (GameObject level in otherLevelObjects)
        {
            level.SetActive(!deactivate);
        }
    }

    IEnumerator ShowButtonsCoroutine()
    {
        // Activate all buttons and reset their scale.
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].SetActive(true);
            levelButtons[i].transform.localScale = Vector3.zero;
        }

        // Pop each button with a short delay between them.
        for (int i = 0; i < levelButtons.Length; i++)
        {
            // Delay: 0 sec for the first, 0.2 sec for the second, 0.4 sec for the third.
            yield return new WaitForSeconds(i * 0.2f);
            StartCoroutine(PopAnimation(levelButtons[i].transform));
        }
    }

    IEnumerator PopAnimation(Transform target)
    {
        float duration = 0.2f; // Faster pop animation duration.
        float elapsed = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Use an ease-out-back function for a fun, bouncy pop.
            float scaleFactor = EaseOutBack(t);
            target.localScale = Vector3.LerpUnclamped(initialScale, finalScale, scaleFactor);
            yield return null;
        }
        target.localScale = finalScale;
    }

    // Ease-out-back easing function for a bouncy effect.
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    IEnumerator MoveCameraToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        targetPosition.z = startPosition.z; // Keep the camera's original z position

        // For level 1, don't move the camera at all.
        if (levelId == 1)
        {
            yield break; // Exit the coroutine without moving the camera.
        }

        // For levels 2, 3, and 4, reduce the vertical movement to 20% of the intended move.
        if (levelId >= 2 && levelId <= 4)
        {
            float deltaY = targetPosition.y - startPosition.y;
            targetPosition.y = startPosition.y + (deltaY * 0.2f);
        }

        // For levels 10, 11, 12, and 13, adjust the downward movement.
        if (levelId >= 10 && levelId <= 13)
        {
            float deltaY = targetPosition.y - startPosition.y;
            if (deltaY < 0) // Only adjust if the movement is downward.
            {
                if (levelId == 13)
                {
                    // For level 13, do not move down at all.
                    targetPosition.y = startPosition.y;
                }
                else if (levelId == 12)
                {
                    // For level 12, reduce downward movement to 10% of the intended move.
                    targetPosition.y = startPosition.y + (deltaY * 0.1f);
                }
                else // For levels 10 and 11
                {
                    // Reduce downward movement to 20% of the intended move.
                    targetPosition.y = startPosition.y + (deltaY * 0.2f);
                }
            }
        }

        // Calculate the camera's extents (half dimensions in world units)
        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * mainCamera.aspect;

        // Define the background boundaries
        float bgMinX = -220.8f;
        float bgMaxX = bgMinX + 231.8f; // 11.0f
        float bgMinY = -26.9f;
        float bgMaxY = bgMinY + 199.2f; // 172.3f

        // Clamp the target position so the camera's view stays within the background
        targetPosition.x = Mathf.Clamp(targetPosition.x, bgMinX + horzExtent, bgMaxX - horzExtent);
        targetPosition.y = Mathf.Clamp(targetPosition.y, bgMinY + vertExtent, bgMaxY - vertExtent);

        float elapsedTime = 0f;
        float duration = 1.5f; // Adjust for a smoother transition

        while (elapsedTime < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }

        mainCamera.transform.position = targetPosition; // Ensure exact position at the end
    }
}
