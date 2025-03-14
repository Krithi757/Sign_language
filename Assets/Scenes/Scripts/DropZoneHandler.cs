using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZoneHandler : MonoBehaviour, IDropHandler
{
    public string expectedWord; // Set this in Inspector

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject != null)
        {
            Text draggedWordText = droppedObject.GetComponent<Text>();

            if (draggedWordText != null)
            {
                string draggedWord = draggedWordText.text;

                if (draggedWord == expectedWord)
                {
                    Debug.Log("Correct Match!");

                    GameManager.instance.CorrectAnswer();

                    droppedObject.GetComponent<WordDragHandler>().enabled = false;
                    droppedObject.transform.SetParent(transform);
                    droppedObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
                else
                {
                    Debug.Log("Wrong Match!");

                    GameManager.instance.WrongAnswer();

                    droppedObject.GetComponent<WordDragHandler>().ResetPosition();
                }
            }
        }
    }
}

