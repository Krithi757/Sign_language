using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelData : MonoBehaviour
{
    public int levelId;


    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>(); // Assuming the object has a RectTransform (UI element)
    }

    void Update()
    {
        if (Input.touchCount > 0) // Checks if there is at least one touch
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            if (touch.phase == TouchPhase.Began) // Check if the touch started
            {
                Vector2 touchPosition = touch.position; // Get touch position on screen

                // Check if the touch is inside this object's bounds
                if (rectTransform.rect.Contains(rectTransform.InverseTransformPoint(touchPosition)))
                {
                    OnLevelTapped();
                }
            }
        }
    }

    public void OnLevelTapped()
    {
        Debug.Log($"Level {levelId} tapped!");
        PlayerPrefs.SetInt("SelectedLevelId", levelId);
        PlayerPrefs.Save();
        // You can trigger your level logic here
        //SceneManager.LoadScene(2);
    }
}
