using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;

public class QuestionManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; // 🎥 Reference to the Video Player
    public List<VideoClip> questionVideos; // 📹 List of video questions
    public List<string> answers; // 📝 List of answers
    public TextMeshProUGUI[] answerTexts; // 🔤 UI Text objects for answers

    private int currentQuestionIndex = 0;

    private void Start()
    {
        if (questionVideos.Count == 0 || answers.Count == 0)
        {
            Debug.LogError("No questions or answers assigned!");
            return;
        }

        DisplayNextQuestion();
    }

    public void DisplayNextQuestion()
    {
        if (currentQuestionIndex >= questionVideos.Count)
        {
            Debug.LogWarning("No more questions left!");
            return;
        }

        // Set and play the video
        videoPlayer.clip = questionVideos[currentQuestionIndex];
        videoPlayer.Play();

        // Set answer texts
        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (i < answers.Count)
            {
                answerTexts[i].text = answers[i]; // Assign answers to buttons
            }
        }

        currentQuestionIndex++;
    }
}
