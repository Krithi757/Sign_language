using UnityEngine;
using UnityEngine.EventSystems;

// Lives on "VideoScreen" (same object as TapToChangeVideo and the Raw Image).
//
// Catches a word chip when it's dropped onto the video, checks it against the
// correct word for the video that's currently playing, logs the result, then
// moves on to the next video either way.
//
// Later: this is where the fox cooking animation / correct-or-wrong food /
// customer reaction / scoring will get triggered, in the isCorrect / else branches below.
public class VideoDropTarget : MonoBehaviour, IDropHandler
{
    [Tooltip("Drag the WordRoundManager GameObject here.")]
    public WordRoundManager wordRoundManager;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        DraggableWord dragged = droppedObj.GetComponent<DraggableWord>();
        if (dragged == null) return;

        if (wordRoundManager == null)
        {
            Debug.LogError("❌ VideoDropTarget: Word Round Manager is not assigned in the Inspector.");
            return;
        }

        dragged.MarkAsDropped();

        string correctWord = wordRoundManager.currentCorrectWord;
        bool isCorrect = string.Equals(dragged.Word, correctWord, System.StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            Debug.Log("✅ Yay! '" + dragged.Word + "' is correct!");
            // TODO next step: fox cooking animation + serve correct food + customer happy
        }
        else
        {
            Debug.Log("❌ Wrong! You dropped '" + dragged.Word + "', the correct answer was '" + correctWord + "'.");
            // TODO next step: fox serves wrong ingredients + customer angry + lower score
        }

        Destroy(droppedObj);

        // Either way, move on to the next video. This also makes WordRoundManager
        // spawn a fresh set of word options (it's listening for OnVideoChanged).
        wordRoundManager.videoController.NextVideo();
    }
}
