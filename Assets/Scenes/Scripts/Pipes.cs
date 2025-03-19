using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

public class Pipe : MonoBehaviour
{
    public TMP_Text answerText; // Reference to the floating text
    public string answer;
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
        }
    }
}
