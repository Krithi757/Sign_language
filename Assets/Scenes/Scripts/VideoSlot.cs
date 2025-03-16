using UnityEngine;
using UnityEngine.EventSystems;

public class VideoSlot : MonoBehaviour, IDropHandler
{
    public string correctWord;
    public GameManager gameManager;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableWord draggedWord = eventData.pointerDrag.GetComponent<DraggableWord>();

        if (draggedWord != null)
        {
            bool isCorrect = draggedWord.name == correctWord;
            gameManager.CheckMatch(isCorrect);
            Destroy(draggedWord.gameObject); // Remove word after checking
        }
    }
}
