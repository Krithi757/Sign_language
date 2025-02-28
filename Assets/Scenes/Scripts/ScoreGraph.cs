using UnityEngine;
using System.Collections.Generic;

public class ScoreGraph : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int maxPoints = 10; // Number of past scores to display
    public float xSpacing = 1.0f; // Spacing between points on X-axis

    private List<float> scoreHistory = new List<float>();

    void Start()
    {
        // Get the LineRenderer component
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;

        // Set material (make sure you have an Unlit/Color material in your project)
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));

        // Set line width
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Set color
        lineRenderer.material.color = Color.blue;

        // Load previous scores from PlayerPrefs (if available)
        LoadScoreHistory();
        DrawGraph();
    }

    void LoadScoreHistory()
    {
        scoreHistory.Clear();
        Debug.Log("Loading score history...");

        for (int i = 0; i < maxPoints; i++)
        {
            float score = PlayerPrefs.GetFloat("Score_" + i, -1f); // Default to -1 to check if it exists
            if (score != -1f) // Only log if there's a saved score
            {
                Debug.Log($"Loaded Score_{i}: {score}");
            }
            scoreHistory.Add(score);
        }
    }


    public void DrawGraph()
    {
        int count = scoreHistory.Count;
        Debug.Log("Score History Count: " + count); // Check if scores exist

        if (count == 0)
        {
            Debug.LogWarning("No scores available to draw the graph.");
            return;
        }

        lineRenderer.positionCount = count;
        Debug.Log("Setting Line Renderer Position Count: " + count);

        for (int i = 0; i < count; i++)
        {
            float x = i * xSpacing;
            float y = scoreHistory[i]; // Score value as Y coordinate
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            Debug.Log($"Point {i}: ({x}, {y})"); // Print each point
        }
    }


    public void AddNewScore(float newScore)
    {
        Debug.Log("Adding new score: " + newScore);

        if (scoreHistory.Count >= maxPoints)
        {
            Debug.Log("Removing oldest score: " + scoreHistory[0]);
            scoreHistory.RemoveAt(0);
        }

        scoreHistory.Add(newScore);

        for (int i = 0; i < scoreHistory.Count; i++)
        {
            PlayerPrefs.SetFloat("Score_" + i, scoreHistory[i]);
            Debug.Log($"Saving Score_{i}: {scoreHistory[i]}");
        }
        PlayerPrefs.Save();

        DrawGraph(); // Redraw after adding a new score
    }

}
