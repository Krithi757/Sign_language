using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

// Shows a "3 2 1 Go!" countdown before the game begins. Freezes the whole
// game (Time.timeScale = 0) for the duration so nothing can spawn/move
// underneath it, then unfreezes and fires onCountdownComplete right after.
//
// No separate panel needed — just the countdown text itself gets shown/hidden.
//
// Setup:
//   - countdownText: the TMP text showing "3" / "2" / "1" / "GO!". This
//     script shows it on Start and hides it again once the countdown finishes.
//   - onCountdownComplete: wire this in the Inspector to whatever should
//     actually kick the game off for you (e.g. your CustomerManager's spawn
//     method). This script doesn't call into CustomerManager directly since
//     it doesn't know its API — the UnityEvent keeps it decoupled.
public class CountdownController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI countdownText;

    [Header("Timing")]
    [Tooltip("Seconds each number (3, 2, 1) stays on screen.")]
    public float secondsPerNumber = 1f;
    [Tooltip("What briefly shows after 1, right before it hides.")]
    public string goText = "GO!";
    [Tooltip("How long GO! stays on screen before hiding.")]
    public float goDisplaySeconds = 0.5f;

    [Header("Events")]
    [Tooltip("Fires the instant the countdown finishes and time unfreezes — wire your game-start logic here.")]
    public UnityEvent onCountdownComplete;

    void Start()
    {
        StartCoroutine(RunCountdown());
    }

    private IEnumerator RunCountdown()
    {
        if (countdownText == null)
        {
            Debug.LogWarning("⚠️ CountdownController: countdownText is NOT assigned in the Inspector — nothing to show.");
            onCountdownComplete?.Invoke();
            yield break;
        }

        Debug.Log("⏳ Countdown starting...");
        Time.timeScale = 0f; // freeze everything else while counting down
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            Debug.Log("Countdown: " + i);
            yield return WaitRealSeconds(secondsPerNumber);
        }

        countdownText.text = goText;
        Debug.Log("Countdown: " + goText);
        yield return WaitRealSeconds(goDisplaySeconds);

        Debug.Log("✅ Countdown finished -> hiding text, resuming Time.timeScale, firing onCountdownComplete.");
        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f;

        onCountdownComplete?.Invoke();
    }

    // Time.timeScale is 0 during the countdown, so a normal WaitForSeconds
    // (which is itself scaled by Time.timeScale) would never advance and the
    // countdown would freeze forever. Wait on unscaled real time instead.
    private IEnumerator WaitRealSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}