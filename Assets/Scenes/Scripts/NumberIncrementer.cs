using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class NumberIncrementer : MonoBehaviour
{
    public TextMeshProUGUI numberText;             // UI Text to display the number
    public float animationDuration = 1f;           // Duration of the increment animation

    private bool _isFinished = true; // Initially true since no animation is running
    public bool IsFinished => _isFinished;

    /// <summary>
    /// Starts the incrementing animation from the current value to the target value.
    /// </summary>
    /// <param name="targetValue">The final number to reach.</param>
    public void IncrementTo(int targetValue)
    {
        int startValue = int.Parse(numberText.text);  // Get current value from the text
        _isFinished = false;  // Mark as not finished

        // Adjust duration if you want to speed up or slow down the overall animation
        float fastDuration = animationDuration * 0.6f; // 60% for the fast part
        float slowDuration = animationDuration * 0.4f; // 40% for the slow part

        // Use DOTween to animate the value with a custom easing pattern
        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            numberText.text = startValue.ToString();  // Update text each frame
        }, targetValue, fastDuration)  // Fast part
        .SetEase(Ease.OutQuad)           // Fast and smooth ease for the first part
        .OnComplete(() =>
        {
            DOTween.To(() => startValue, x =>
            {
                startValue = x;
                numberText.text = startValue.ToString();  // Update text each frame
            }, targetValue, slowDuration)  // Slow part
            .SetEase(Ease.OutCubic)
            .OnComplete(() => _isFinished = true);  // Mark as finished when done
        });
    }
}
