using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Add this at the top

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;



using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class WordDisplayHandler : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI wordDisplayText; // Reference to TextMeshProUGUI component
    public GameObject box;                  // Reference to the corresponding box GameObject

    private List<string> videoNames = new List<string>(); // List to hold the video names
    private int currentIndex = 0; // Index to track which name to display next

    // Dictionary to store words for each box, so each box can have its own set of words
    private static Dictionary<GameObject, string> boxWords = new Dictionary<GameObject, string>(); // Updated to store single word for each box
    private static List<string> availableWords = new List<string>(); // List to store available words

    // Static flag to track whether a word is currently being displayed
    private static bool isWordDisplayed = false;

    void Start()
    {
        // Initially hide the word display text
        wordDisplayText.gameObject.SetActive(false);

        // Update video names and store them in the dictionary at the start
        UpdateVideoNames();
    }

    // Method to update the video names based on the assigned box
    void UpdateVideoNames()
    {
        // Check if the box is assigned and if there are video assignments for the box
        if (BoxClickHandler.boxVideoAssignments == null || !BoxClickHandler.boxVideoAssignments.ContainsKey(box))
        {
            Debug.LogError("BoxVideoAssignments not initialized or box not assigned!");
            return;
        }

        // Clear the available words list before adding new ones
        availableWords.Clear();

        // Get all assigned video paths from the BoxClickHandler
        foreach (var boxAssignment in BoxClickHandler.boxVideoAssignments)
        {
            GameObject currentBox = boxAssignment.Key;
            string assignedVideoPath = boxAssignment.Value;

            // Fetch the corresponding name from availableVideoPaths in BoxClickHandler
            if (BoxClickHandler.availableVideoPaths.TryGetValue(assignedVideoPath, out string videoName))
            {
                // Add the video name to the availableWords list
                availableWords.Add(videoName);
                Debug.Log($"Added video name: {videoName} to available words list");
            }
            else
            {
                Debug.LogError($"Video path {assignedVideoPath} not found in availableVideoPaths!");
            }
        }

        // Shuffle the available words
        ShuffleWords();

        // Now assign words to boxes, ensuring no repetition
        AssignWordsToBoxes();
    }

    // Shuffle the list of available words
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

    // Assign words to boxes, ensuring no repetition
    void AssignWordsToBoxes()
    {
        foreach (var currentBox in BoxClickHandler.boxVideoAssignments.Keys)
        {
            // If the box doesn't already have a word assigned, assign it one from the shuffled available words
            if (!boxWords.ContainsKey(currentBox) && availableWords.Count > 0)
            {
                string assignedWord = availableWords[0];
                boxWords[currentBox] = assignedWord;

                // Remove the assigned word from the available words list to avoid repetition
                availableWords.RemoveAt(0);

                Debug.Log($"Assigned word: {assignedWord} to box: {currentBox.name}");
            }
        }
    }

    // Method to handle box click events and display the corresponding word
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Box clicked: {gameObject.name}");

        // If a word is already being displayed, exit the function
        if (isWordDisplayed)
        {
            Debug.Log("A word is already displayed. Please wait until it disappears.");
            return;
        }

        // Set the flag that a word is now being displayed
        isWordDisplayed = true;

        // Start the rotation of the box and move the word to the backside after rotation
        StartCoroutine(RotateBox(() =>
        {
            // After rotation, display the word
            if (boxWords.ContainsKey(box))
            {
                wordDisplayText.text = boxWords[box];
                wordDisplayText.gameObject.SetActive(true);
                Debug.Log($"Displayed word: {wordDisplayText.text} for box: {box.name}");

                // Start the disappear and rotation back coroutines
                StartCoroutine(DisappearWordAfterDelay(2.0f, () =>
                {
                    // Rotate the box back after making the word disappear
                    StartCoroutine(RotateBackAfterDelay(0f)); // No delay between disappearing and rotation
                }));
            }
        }));
    }

    // Method to handle box rotation
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

        // Move the word to the backside after the rotation
        MoveWordToBoxBackside(gameObject);

        // Call the callback after rotation
        onRotationComplete?.Invoke();
    }

    // Method to move the word display to the backside of the clicked box
    void MoveWordToBoxBackside(GameObject clickedBox)
    {
        // Set the wordDisplayText as a child of the clicked box
        wordDisplayText.rectTransform.SetParent(clickedBox.transform);

        // Position the word at the backside of the clicked box
        wordDisplayText.rectTransform.localPosition = new Vector3(-50f, 0, -0.5f);  // Adjust based on your scene's scale and box position
        wordDisplayText.rectTransform.localRotation = Quaternion.Euler(0, 180, 0); // Flip to face the backside of the clicked box

        // Enable the word display text if it's not already visible
        wordDisplayText.gameObject.SetActive(true);

        Debug.Log($"Word moved to backside of clicked box: {clickedBox.name}");
    }

    // Coroutine to make the word disappear after the specified delay
    IEnumerator DisappearWordAfterDelay(float delay, System.Action onDisappearComplete)
    {
        yield return new WaitForSeconds(delay);

        // Disable the word display text
        wordDisplayText.gameObject.SetActive(false);
        Debug.Log("Word disappeared after delay");

        // Call the callback after the word disappears
        onDisappearComplete?.Invoke();

        // Reset the flag that a word has been displayed
        isWordDisplayed = false;
    }

    // Coroutine to rotate the box back to the original position after the delay
    IEnumerator RotateBackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Rotate the box back to its original position
        yield return StartCoroutine(RotateBoxBack());
    }

    // Rotate the box back to its original position
    IEnumerator RotateBoxBack()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, 0);

        float duration = 1.0f;
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
