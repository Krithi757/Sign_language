using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WordDisplayHandler : MonoBehaviour
{
    public TextMeshProUGUI wordDisplayText; // Reference to TextMeshProUGUI component
    public GameObject box;                  // Reference to the corresponding box GameObject

    private List<string> videoNames = new List<string>();
    private static Dictionary<GameObject, string> boxWords = new Dictionary<GameObject, string>();
    private static List<string> availableWords = new List<string>();
    private static bool isWordDisplayed = false;

    void Start()
    {
        wordDisplayText.gameObject.SetActive(false);
        UpdateVideoNames();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;

            // Convert the touch position to the local space of the UI element
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPos, null, out localPoint);

            // Check if the touch is within the bounds of the UI element
            if (rectTransform.rect.Contains(localPoint))
            {
                HandleTouch();
            }
        }
    }

    void HandleTouch()
    {
        // Ensure the `box` is updated dynamically based on the user's click.
        if (box == null)
        {
            Debug.LogError("Box is null. Ensure the correct box is assigned.");
            return;
        }

        Debug.Log($"Box clicked: {box.name}");

        // Start rotating the box and display the word
        StartCoroutine(RotateBox(() =>
        {
            // Check if the word exists for the current box
            if (boxWords.ContainsKey(box))
            {
                string wordToDisplay = boxWords[box];
                wordDisplayText.text = wordToDisplay;
                wordDisplayText.gameObject.SetActive(true);
                Debug.Log($"Displayed word: {wordToDisplay} for box: {box.name}");

                // Ensure we're getting the correct `BoxClickHandler` for the current box
                var boxClickHandler = box.GetComponent<BoxClickHandler>();
                if (boxClickHandler != null)
                {
                    Debug.Log($"Checking match for word: {wordToDisplay} and box: {box.name}");
                    // Set selected word in BoxClickHandler
                    BoxClickHandler.selectedWord = wordToDisplay;  // Assign word to the BoxClickHandler (static access) // Assign word to the BoxClickHandler
                    boxClickHandler.CheckMatchWithWord(wordToDisplay);  // Always check the match
                }
                else
                {
                    Debug.LogWarning($"No BoxClickHandler found for box: {box.name}");
                }

                // After displaying the word, set a delay to hide it
                StartCoroutine(DisappearWordAfterDelay(2.0f, () =>
                {
                    wordDisplayText.gameObject.SetActive(false); // Hide the word
                    StartCoroutine(RotateBackAfterDelay(0f));     // Optionally rotate back
                }));
            }
            else
            {
                Debug.LogWarning($"No word found for box: {box.name}");
            }
        }));
    }



    void UpdateVideoNames()
    {
        if (BoxClickHandler.boxVideoAssignments == null || !BoxClickHandler.boxVideoAssignments.ContainsKey(box))
        {
            Debug.LogError("BoxVideoAssignments not initialized or box not assigned!");
            return;
        }

        availableWords.Clear();

        // Instead of BoxClickHandler.availableVideoPaths, we use VideoPathManager
        foreach (var boxAssignment in BoxClickHandler.boxVideoAssignments)
        {
            GameObject currentBox = boxAssignment.Key;
            string assignedVideoPath = boxAssignment.Value;

            // Get the video name using VideoPathManager based on the assigned video path
            if (VideoPathManager.GetVideoPaths().TryGetValue(assignedVideoPath, out string videoName))
            {
                availableWords.Add(videoName);
                Debug.Log($"Added video name: {videoName} to available words list");
            }
            else
            {
                Debug.LogError($"Video path {assignedVideoPath} not found in VideoPathManager!");
            }
        }

        ShuffleWords();
        AssignWordsToBoxes();
    }

    void ShuffleWords()
    {
        for (int i = 0; i < availableWords.Count; i++)
        {
            string temp = availableWords[i];
            int randomIndex = Random.Range(i, availableWords.Count);
            availableWords[i] = availableWords[randomIndex];
            availableWords[randomIndex] = temp;
        }
    }

    void AssignWordsToBoxes()
    {
        foreach (var currentBox in BoxClickHandler.boxVideoAssignments.Keys)
        {
            if (!boxWords.ContainsKey(currentBox) && availableWords.Count > 0)
            {
                string assignedWord = availableWords[0];
                boxWords[currentBox] = assignedWord;
                availableWords.RemoveAt(0);
                Debug.Log($"Assigned word: {assignedWord} to box: {currentBox.name}");
            }
        }
    }

    IEnumerator RotateBox(System.Action onRotationComplete)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 180, 0);

        float duration = 0.5f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        MoveWordToBoxBackside(gameObject);
        onRotationComplete?.Invoke();
    }

    void MoveWordToBoxBackside(GameObject clickedBox)
    {
        wordDisplayText.rectTransform.SetParent(clickedBox.transform);
        wordDisplayText.rectTransform.localPosition = new Vector3(-50f, 0, -0.5f);
        wordDisplayText.rectTransform.localRotation = Quaternion.Euler(0, 180, 0);
        wordDisplayText.gameObject.SetActive(true);
        Debug.Log($"Word moved to backside of clicked box: {clickedBox.name}");
    }

    IEnumerator DisappearWordAfterDelay(float delay, System.Action onDisappearComplete)
    {
        yield return new WaitForSeconds(delay);
        onDisappearComplete?.Invoke();
    }

    IEnumerator RotateBackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.identity;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = endRotation;
    }

    IEnumerator RotateBoxBack()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, 0);

        float duration = 0.5f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;

    }
}
