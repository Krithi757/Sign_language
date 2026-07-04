using UnityEngine;
using UnityEngine.EventSystems;

// Lives on "VideoScreen" alongside TapToChangeVideo and the Raw Image.
//
// When a word chip is dropped:
//  - The chip is destroyed immediately (so it doesn't float around during animation).
//  - The fox plays its correct or wrong sequence.
//  - NextVideo() fires only AFTER the fox finishes — passed as a callback.
//  - If no fox is assigned, NextVideo fires right away (safe fallback).
public class VideoDropTarget : MonoBehaviour, IDropHandler
{
    [Tooltip("Drag the WordRoundManager GameObject here.")]
    public WordRoundManager wordRoundManager;

    [Tooltip("Drag the Fox GameObject (the one with FoxAnimationController) here.")]
    public FoxAnimationController foxController;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        DraggableWord dragged = droppedObj.GetComponent<DraggableWord>();
        if (dragged == null) return;

        if (wordRoundManager == null)
        {
            Debug.LogError("❌ VideoDropTarget: Word Round Manager is not assigned.");
            return;
        }

        dragged.MarkAsDropped();
        Destroy(droppedObj); // remove chip immediately — no floating during animation

        string correctWord = wordRoundManager.currentCorrectWord;
        bool isCorrect = string.Equals(dragged.Word, correctWord, System.StringComparison.OrdinalIgnoreCase);

        // NextVideo is a deferred callback — fires AFTER the fox finishes animating.
        // If there's no fox, it fires right now as a safe fallback.
        System.Action advanceVideo = () => wordRoundManager.videoController.NextVideo();

        if (isCorrect)
        {
            Debug.Log("✅ Yay! '" + dragged.Word + "' is correct!");
            if (foxController != null)
                foxController.PlayCorrectSequence(advanceVideo);
            else
                advanceVideo();
            // TODO next step: customer happy + score up
        }
        else
        {
            Debug.Log("❌ Wrong! '" + dragged.Word + "' — correct was '" + correctWord + "'.");
            if (foxController != null)
                foxController.PlayWrongSequence(advanceVideo);
            else
                advanceVideo();
            // TODO next step: customer angry + score down
        }
    }
}
