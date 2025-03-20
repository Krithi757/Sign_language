using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

public class Pipes : MonoBehaviour
{
    public TMP_Text answerText; // Reference to the floating text
    private string answer;
    public float speed = 5f;
    private float leftEdge;

    private void Start()
    {
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - 900f;
    }

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }

    public void SetAnswer(string newAnswer)
    {
        answer = newAnswer;
        if (answerText != null)
        {
            answerText.text = newAnswer;
            Debug.Log("Pipe answer set to: " + newAnswer); //Debug to check if it's working
        }
        else
        {
            Debug.LogError("answerText is NULL! Ensure the TMP_Text component is assigned.");
        }
    }


    public string GetAnswer()
    {
        return answerText.text;
    }
}
