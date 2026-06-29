using UnityEngine;
using UnityEngine.EventSystems;

// Lives on the "WordOption" prefab (the small draggable word chip).
// Makes the chip follow the finger/mouse while being dragged, and snaps it
// back to where it started if it's released somewhere that doesn't accept it.
[RequireComponent(typeof(RectTransform))]
public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Drag the child Text (TextMeshPro) object here so the chip can show its word.")]
    public TMPro.TextMeshProUGUI label;

    [HideInInspector] public string Word;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startAnchoredPosition;
    private Transform startParent;
    private bool wasDropped = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetWord(string word)
    {
        Word = word;
        if (label != null) label.text = word;
    }

    // VideoDropTarget calls this the moment it accepts the drop, so OnEndDrag
    // below knows not to snap the chip back (it's about to be destroyed anyway).
    public void MarkAsDropped()
    {
        wasDropped = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startAnchoredPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;

        // Re-parent to the top of the Canvas while dragging so the chip renders
        // above everything else (the word container, the video, etc.).
        transform.SetParent(canvas.transform, true);

        canvasGroup.blocksRaycasts = false; // let the drop target underneath detect the pointer
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (wasDropped) return; // VideoDropTarget already handled this chip

        // Not dropped on a valid target - snap back to where it came from.
        transform.SetParent(startParent, false);
        rectTransform.anchoredPosition = startAnchoredPosition;
    }
}
