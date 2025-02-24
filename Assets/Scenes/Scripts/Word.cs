using UnityEngine;
using TMPro;

public class Word : MonoBehaviour
{
    public TextMeshProUGUI wordText;
    private FallingWordsChallenge gameManager;
    private bool isDragging = false;
    private Vector3 startPosition;

    public void Initialize(string word, FallingWordsChallenge manager)
    {
        wordText.text = word;
        gameManager = manager;
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isDragging)
        {
            transform.Translate(Vector3.down * gameManager.fallSpeed * Time.deltaTime);
            if (transform.position.y < -5) // Adjust based on screen
            {
                gameManager.CheckWordMatch(wordText.text, null);
                Destroy(gameObject);
            }
        }
    }

    public void StartDrag()
    {
        isDragging = true;
    }

    public void EndDrag(Transform dropZone)
    {
        isDragging = false;
        gameManager.CheckWordMatch(wordText.text, dropZone);
        Destroy(gameObject);
    }
}
