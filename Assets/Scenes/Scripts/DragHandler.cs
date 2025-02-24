using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Word wordScript;

    void Start()
    {
        wordScript = GetComponent<Word>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        wordScript.StartDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Transform dropZone = eventData.pointerEnter?.transform;
        wordScript.EndDrag(dropZone);
    }
}
