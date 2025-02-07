using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordDisplayHandler : MonoBehaviour
{
    public TextMeshProUGUI wordDisplayText; // Reference to TextMeshProUGUI component
    public GameObject box;                  // Reference to the corresponding box GameObject

    private List<string> videoNames = new List<string>();
    public static Dictionary<GameObject, string> boxWords = new Dictionary<GameObject, string>();
    private static List<string> availableWords = new List<string>();

    void Start()
    {
        ResetGame();
        wordDisplayText.gameObject.SetActive(true); // Ensure all TextMeshProUGUI components are active
        UpdateVideoNames();
        AssignWordsToBoxes();
        UpdateWordDisplays();
    }

    void ResetGame()
    {
        //Retry();
        wordDisplayText.gameObject.SetActive(true);
        wordDisplayText.text = ""; // Clear previous word display
        UpdateVideoNames();
        AssignWordsToBoxes();
        UpdateWordDisplays();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;

            RectTransform rectTransform = GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPos, null, out Vector2 localPoint) &&
                rectTransform.rect.Contains(localPoint))
            {
                HandleTouch();
            }
        }
    }

    void HandleTouch()
    {
        if (box == null)
        {
            Debug.LogError("Box is null. Ensure the correct box is assigned.");
            return;
        }

        Debug.Log($"Word Box clicked: {gameObject.name}");

        if (boxWords.ContainsKey(gameObject))
        {
            string wordToDisplay = boxWords[gameObject];
            wordDisplayText.text = wordToDisplay;
            wordDisplayText.gameObject.SetActive(true);
            Debug.Log($"Displayed word: {wordToDisplay} for box: {gameObject.name}");

            var boxClickHandler = box.GetComponent<BoxClickHandler>();
            if (boxClickHandler != null)
            {
                Debug.Log($"Checking match for word: {wordToDisplay} and box: {gameObject.name}");
                BoxClickHandler.selectedWord = wordToDisplay;
                boxClickHandler.CheckMatchWithWord(wordToDisplay);
            }
            else
            {
                Debug.LogWarning($"No BoxClickHandler found for box: {gameObject.name}");
            }


        }
        else
        {
            Debug.LogWarning($"No word found for box: {gameObject.name}");
        }
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
            string assignedVideoPath = boxAssignment.Value;

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
        foreach (var videoPath in BoxClickHandler.boxVideoAssignments.Values)
        {
            foreach (var currentBox in BoxClickHandler.boxVideoAssignments.Keys)
            {
                string boxName = currentBox.name.Replace("Video", "Word");
                GameObject wordBox = GameObject.Find(boxName);

                if (wordBox != null && !boxWords.ContainsKey(wordBox) && availableWords.Count > 0)
                {
                    string assignedWord = availableWords[0];
                    boxWords[wordBox] = assignedWord;
                    availableWords.RemoveAt(0);

                    Debug.Log($"Assigned word: {assignedWord} to box: {boxName}");
                }
            }
        }
    }



    void UpdateWordDisplays()
    {
        foreach (KeyValuePair<GameObject, string> pair in boxWords)
        {
            GameObject wordBox = pair.Key;
            string assignedWord = pair.Value;

            TextMeshProUGUI textComponent = wordBox.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = assignedWord;
                Debug.Log($"Updated TextMeshProUGUI for box: {wordBox.name} with word: {assignedWord}");
            }
            else
            {
                Debug.LogError($"TextMeshProUGUI not found in box: {wordBox.name}");
            }
        }
    }

}