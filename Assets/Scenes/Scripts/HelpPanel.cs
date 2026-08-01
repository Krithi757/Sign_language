using UnityEngine;
using UnityEngine.UI;

// Question-mark "Help" button -> ShowHelp() / HideHelp(), matching the same
// pause-the-whole-game pattern your Running_challenge.cs already uses for
// its help panel: freezes Time.timeScale, pauses ALL audio via
// AudioManager.PauseAllSounds()/ResumeAllSounds() (not just a tap sound),
// and shows/hides a separate closeButton object alongside the panel.
//
// On top of that, this also supports "click outside the panel closes it"
// (which Running_challenge.cs's version doesn't have, but you asked for it
// originally) via an optional invisible full-screen backdrop button — leave
// backdropButton empty if you don't want that and just rely on Cancel/closeButton.
//
// Setup:
//   - helpPanel: the RawImage GameObject with your 2-line instructions text.
//     Leave it INACTIVE in the Hierarchy to start.
//   - closeButton: your Cancel/X button, as its own object (same pattern as
//     Running_challenge.cs — shown/hidden alongside the panel, not baked
//     into it). Leave INACTIVE to start.
//   - backdropButton (optional): a full-screen, invisible Button placed
//     ABOVE helpPanel in the Hierarchy list so helpPanel renders on top of
//     it and blocks clicks inside the panel, while clicks anywhere else fall
//     through to it and close the panel. Leave its own OnClick() empty —
//     this script wires it up in code. Leave the field empty entirely to
//     skip click-outside-to-close.
//   - videoController (optional): drag your TapToChangeVideo/VideoPlayGate
//     reference here if you want the sign-language video to pause while
//     help is open too, same as Running_challenge.cs pausing its videoPlayer.
//
//   Question mark button's OnClick() -> ShowHelp()
//   Cancel/closeButton's OnClick()   -> HideHelp()
public class HelpPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject helpPanel;
    public GameObject closeButton;

    [Header("Click-outside-to-close (optional)")]
    public Button backdropButton;

    [Header("Optional — pause the sign-language video too")]
    [Tooltip("Caveat: this blindly resumes the video on HideHelp() even if no " +
             "customer was actually active/playing before you opened Help. Leave " +
             "empty if that edge case matters to you — Time.timeScale alone " +
             "already freezes the video frame in place while Help is open.")]
    public VideoPlayGate videoController;

    void Awake()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);
        if (backdropButton != null)
        {
            backdropButton.gameObject.SetActive(false);
            backdropButton.onClick.AddListener(HideHelp);
        }
    }

    /// <summary>The question-mark button's OnClick().</summary>
    public void ShowHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(true);
        if (closeButton != null) closeButton.SetActive(true);
        if (backdropButton != null) backdropButton.gameObject.SetActive(true);

        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            FindObjectOfType<AudioManager>()?.PlaySound("TapSound");
        FindObjectOfType<AudioManager>()?.PauseAllSounds();

        if (videoController != null) videoController.SetPaused();

        Time.timeScale = 0f; // pause the entire game while help is open
    }

    /// <summary>Cancel/closeButton's OnClick() — also called by the optional backdrop click.</summary>
    public void HideHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (closeButton != null) closeButton.SetActive(false);
        if (backdropButton != null) backdropButton.gameObject.SetActive(false);

        FindObjectOfType<AudioManager>()?.ResumeAllSounds();

        if (videoController != null) videoController.SetPlaying();

        Time.timeScale = 1f; // resume the game
    }
}