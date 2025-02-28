using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BarGraph : MonoBehaviour
{
    public GameObject barPrefab; // Assign the "Bar" prefab in the Inspector
    public RectTransform graphContainer; // Assign "GraphPanel"
    public int maxBars = 10; // Number of past scores to display

    private List<float> scoreHistory = new List<float>();

    void Start()
    {
        LoadScoreHistory();
        DrawGraph();
    }

    void LoadScoreHistory()
    {
        scoreHistory.Clear();

        for (int i = 0; i < maxBars; i++)
        {
            float score = PlayerPrefs.GetFloat("Score_" + i, 0f);
            scoreHistory.Add(score);
        }
    }

    public void DrawGraph()
    {
        // Remove old bars
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }

        float maxScore = Mathf.Max(scoreHistory.ToArray());
        float barSpacing = 80f; // Spacing between bars

        for (int i = 0; i < scoreHistory.Count; i++)
        {
            float normalizedScore = (maxScore > 0) ? scoreHistory[i] / maxScore : 0;
            CreateBar(i, normalizedScore, barSpacing);
        }
    }

    void CreateBar(int index, float normalizedHeight, float spacing)
    {
        GameObject newBar = Instantiate(barPrefab, graphContainer);
        RectTransform rt = newBar.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(index * spacing, 0);
        rt.sizeDelta = new Vector2(50, normalizedHeight * 300); // Scale height dynamically

        newBar.GetComponent<Image>().color = Color.Lerp(Color.red, Color.green, normalizedHeight); // Low scores red, high green
    }

    public void AddNewScore(float newScore)
    {
        if (scoreHistory.Count >= maxBars)
            scoreHistory.RemoveAt(0);

        scoreHistory.Add(newScore);

        for (int i = 0; i < scoreHistory.Count; i++)
        {
            PlayerPrefs.SetFloat("Score_" + i, scoreHistory[i]);
        }
        PlayerPrefs.Save();

        DrawGraph();
    }
}

