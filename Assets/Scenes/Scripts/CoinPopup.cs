using System.Collections;
using UnityEngine;
using TMPro;

// Attach to a TextMeshProUGUI GameObject inside a Screen Space Overlay Canvas.
// Call Show("🥚 Egg Kottu!\n+20 coins") to animate it.
// The text floats upward and fades out automatically.
public class CoinPopup : MonoBehaviour
{
    [Tooltip("How long the popup stays visible in seconds.")]
    public float duration = 1.8f;

    [Tooltip("How many pixels it rises before fading.")]
    public float risePixels = 130f;

    private TextMeshProUGUI label;
    private RectTransform rect;
    private Vector2 homePosition;
    private Coroutine current;

    void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        rect  = GetComponent<RectTransform>();
        homePosition = rect.anchoredPosition;
        label.alpha = 0f;
    }

    // Call this from OrderManager with any message string.
    public void Show(string message)
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(Animate(message));
    }

    private IEnumerator Animate(string message)
    {
        label.text = message;
        rect.anchoredPosition = homePosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Rise smoothly upward
            rect.anchoredPosition = homePosition + Vector2.up * (risePixels * t);

            // Fade in quickly (first 15%), hold, then fade out (last 35%)
            if      (t < 0.15f) label.alpha = t / 0.15f;
            else if (t > 0.65f) label.alpha = 1f - (t - 0.65f) / 0.35f;
            else                label.alpha = 1f;

            yield return null;
        }

        label.alpha = 0f;
        rect.anchoredPosition = homePosition;
        current = null;
    }
}
