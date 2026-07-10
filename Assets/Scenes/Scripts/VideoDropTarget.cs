using UnityEngine;
using UnityEngine.EventSystems;

// Lives on "VideoScreen" alongside the Raw Image.
// Detects word chip drops, checks correctness, routes through OrderManager.
public class VideoDropTarget : MonoBehaviour, IDropHandler
{
    [Tooltip("Drag the WordRoundManager here.")]
    public WordRoundManager wordRoundManager;

    [Tooltip("Drag the OrderManager GameObject here.")]
    public OrderManager orderManager;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        DraggableWord dragged = droppedObj.GetComponent<DraggableWord>();
        if (dragged == null) return;

        if (wordRoundManager == null)
        {
            Debug.LogError("❌ VideoDropTarget: WordRoundManager not assigned.");
            return;
        }

        dragged.MarkAsDropped();
        Destroy(droppedObj);

        string correctWord = wordRoundManager.currentCorrectWord;
        bool isCorrect = string.Equals(dragged.Word, correctWord, System.StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            Debug.Log("✅ Correct word: '" + dragged.Word + "'");
            if (orderManager != null)
                orderManager.OnCorrectAnswer();
            else
                wordRoundManager.videoController.NextVideo(); // safe fallback if no OrderManager
        }
        else
        {
            Debug.Log("❌ Wrong: '" + dragged.Word + "' — correct was '" + correctWord + "'");
            if (orderManager != null)
                orderManager.OnWrongAnswer();
            else
                wordRoundManager.videoController.NextVideo(); // safe fallback
        }
    }
}
