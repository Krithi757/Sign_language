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
        if (isWordDisplayed)
        {
            Debug.Log("A word is already displayed. Please wait until it disappears.");
            return;
        }

        isWordDisplayed = true;

        StartCoroutine(RotateBox(() =>
        {
            if (boxWords.ContainsKey(box))
            {
                wordDisplayText.text = boxWords[box];
                wordDisplayText.gameObject.SetActive(true);
                Debug.Log($"Displayed word: {wordDisplayText.text} for box: {box.name}");

                StartCoroutine(DisappearWordAfterDelay(2.0f, () =>
                {
                    StartCoroutine(RotateBackAfterDelay(0f));
                }));
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

        foreach (var boxAssignment in BoxClickHandler.boxVideoAssignments)
        {
            GameObject currentBox = boxAssignment.Key;
            string assignedVideoPath = boxAssignment.Value;

            if (BoxClickHandler.availableVideoPaths.TryGetValue(assignedVideoPath, out string videoName))
            {
                availableWords.Add(videoName);
                Debug.Log($"Added video name: {videoName} to available words list");
            }
            else
            {
                Debug.LogError($"Video path {assignedVideoPath} not found in availableVideoPaths!");
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
        wordDisplayText.gameObject.SetActive(false);
        onDisappearComplete?.Invoke();
        isWordDisplayed = false;
    }

    IEnumerator RotateBackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(RotateBoxBack());
    }

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
