using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelData : MonoBehaviour, IPointerClickHandler
{
    public int levelId;
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Level {levelId} tapped!");
        // You can trigger your level logic here
        //SceneManager.LoadScene(2);
    }
}
