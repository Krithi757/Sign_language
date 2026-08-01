using UnityEngine;
using UnityEngine.UI;

// Shows/hides a Play icon and greys/ungreys the draggable word buttons as a
// pure VISUAL INDICATOR of whether the video is paused or playing — fully
// automatic, the player never clicks it:
//   - No customer stationed (idle): Play icon visible, words greyed out and
//     unclickable, video paused.
//   - Customer reaches the station (active): Play icon disappears, words
//     become enabled/draggable, and the video starts playing automatically
//     at that same instant.
// OrderManager calls SetPlaying()/SetPaused() the instant the active
// customer changes — there's no separate "wait for a tap" step anymore,
// this purely mirrors state the moment it changes.
//
// Setup:
//   1. The Play icon just needs to be visible/hidden — it does NOT need a
//      Button component anymore since nothing clicks it. Drag whatever
//      GameObject is your play-triangle graphic into Play Icon below.
//   2. Add a CanvasGroup component to the PARENT panel that holds your 4
//      draggable word buttons (the "Drag the correct word" panel) — drag
//      that CanvasGroup into Words Canvas Group below. One CanvasGroup on
//      the shared parent greys out + disables all 4 word buttons at once,
//      with zero changes needed to DraggableWord.cs itself.
//   3. Add this component anywhere (e.g. on OrderManager's GameObject) and
//      drag it into OrderManager's "Video Play Gate" field.
public class VideoPlayGate : MonoBehaviour
{
    [Tooltip("Drag your TapToChangeVideo component here.")]
    public TapToChangeVideo videoController;

    [Tooltip("The Play icon — visible while paused (no customer stationed), hidden " +
             "while playing. Pure indicator now, doesn't need a Button component.")]
    public GameObject playIcon;

    [Tooltip("CanvasGroup on the parent panel holding your draggable word buttons. " +
             "Interactable + BlocksRaycasts + Alpha all toggle together here, so the " +
             "whole panel visibly greys out and stops accepting input in one step.")]
    public CanvasGroup wordsCanvasGroup;

    [Range(0f, 1f)]
    [Tooltip("Alpha applied to the words panel while greyed out / paused.")]
    public float greyedOutAlpha = 0.4f;

    void Awake()
    {
        SetPaused(); // start paused/greyed until the first customer arrives
    }

    /// <summary>Call the instant a customer becomes the active/stationed order — video plays automatically, no click needed.</summary>
    public void SetPlaying()
    {
        if (playIcon != null) playIcon.SetActive(false);
        SetWordsInteractable(true);
        if (videoController != null) videoController.ResumeVideo();
    }

    /// <summary>Call whenever there's no active/stationed customer.</summary>
    public void SetPaused()
    {
        if (playIcon != null) playIcon.SetActive(true);
        SetWordsInteractable(false);
        if (videoController != null) videoController.PauseVideo();
    }

    private void SetWordsInteractable(bool on)
    {
        if (wordsCanvasGroup == null) return;
        wordsCanvasGroup.interactable   = on;
        wordsCanvasGroup.blocksRaycasts = on;
        wordsCanvasGroup.alpha = on ? 1f : greyedOutAlpha;
    }
}