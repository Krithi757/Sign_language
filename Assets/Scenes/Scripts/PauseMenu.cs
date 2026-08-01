using UnityEngine;
using UnityEngine.SceneManagement;

// Pause/Resume icon pair sit in the same screen spot (same pattern as your
// Music/SFX toggle icons) — only one is visible at a time, swapped by this
// script. Clicking Pause freezes the game (Time.timeScale = 0) and shows a
// small confirm panel with two options:
//   - Give Up: saves this run's coins/score into PlayerPrefs and goes to
//     ChallengeFeedback, which adds them into your running totals (that add
//     step is the ChallengeFeedback.cs fix from earlier — make sure that
//     updated file is the one in your project).
//   - Continue: hides the panel, un-freezes, and swaps back to the Pause icon.
//
// Setup:
//   Pause icon's Button      -> PauseGame()
//   Resume icon's Button     -> ResumeGame()   (in case clicking the Resume
//     icon itself, not just Continue inside the panel, should also resume)
//   Confirm panel's Continue button -> ResumeGame()
//   Confirm panel's Give Up button  -> GiveUp()
//   Drag OrderManager into Order Manager below.
public class PauseMenu : MonoBehaviour
{
    [Header("Pause / Resume icon pair (same screen position, one visible at a time)")]
    public GameObject pauseIcon;
    public GameObject resumeIcon;

    [Header("Confirm panel — the small RawImage with Give Up / Continue")]
    public GameObject confirmPanel;

    [Header("Wiring")]
    [Tooltip("Drag OrderManager here — Give Up routes through it so the coins/score " +
             "earned so far get saved and shown on ChallengeFeedback.")]
    public OrderManager orderManager;
    [Tooltip("Fallback scene to load from Give Up if Order Manager is left empty. " +
             "Only used as a safety net.")]
    public int fallbackFeedbackSceneIndex = 5;

    private bool isPaused;

    void Start()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        SetIcons(paused: false);
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
        SetIcons(paused: true);
        if (confirmPanel != null) confirmPanel.SetActive(true);
        PlayTap();
    }

    /// <summary>Both the Resume icon AND the panel's Continue button call this.</summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;
        SetIcons(paused: false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        PlayTap();
    }

    /// <summary>The confirm panel's "Give Up" button.</summary>
    public void GiveUp()
    {
        Time.timeScale = 1f; // always restore before leaving the scene
        PlayTap();

        if (orderManager != null)
        {
            Debug.Log("🎬 PauseMenu.GiveUp -> routing through OrderManager.EndLevelAndGoToFeedback");
            orderManager.EndLevelAndGoToFeedback(completed: false);
        }
        else
        {
            Debug.Log("🎬 PauseMenu.GiveUp -> Order Manager not wired, using fallback scene index: " + fallbackFeedbackSceneIndex);
            SceneManager.LoadScene(fallbackFeedbackSceneIndex); // safety net only — wire Order Manager for the real flow
        }
    }

    /// <summary>Home button — wire this to a Home icon's Button OnClick().</summary>
    public void GoHome()
    {
        Time.timeScale = 1f; // always restore before leaving the scene
        PlayTap();
        Debug.Log("🎬 PauseMenu.GoHome -> loading scene index: 1");
        SceneManager.LoadScene(1);
    }

    private void SetIcons(bool paused)
    {
        if (pauseIcon != null) pauseIcon.SetActive(!paused);
        if (resumeIcon != null) resumeIcon.SetActive(paused);
    }

    private void PlayTap()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            FindObjectOfType<AudioManager>()?.PlaySound("TapSound");
    }
}