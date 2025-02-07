using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelData : MonoBehaviour
{
    public int levelId;

    private RectTransform rectTransform;
    public GameObject modePanel;
    public GameObject[] otherLevelObjects; // Array of other level objects to deactivate

    void Start()
    {
        rectTransform = GetComponent<RectTransform>(); // Assuming the object has a RectTransform (UI element)
        modePanel.SetActive(false);
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

        // Toggle the modePanel visibility
        modePanel.SetActive(!modePanel.activeSelf);

        // Deactivate other level objects when the mode panel is active
        if (modePanel.activeSelf)
        {
            DeactivateOtherLevels(true);
        }
        else
        {
            DeactivateOtherLevels(false);
        }
    }

    public void DeactivateOtherLevels(bool deactivate)
    {
        foreach (GameObject level in otherLevelObjects)
        {
            level.SetActive(!deactivate); // Deactivate all other levels if modePanel is active
        }
    }

    public void goToLearning()
    {
        //SceneManager.LoadScene();
    }

    public void goToChallenge()
    {
        SceneManager.LoadScene(4);
    }

    public void goToPractice()
    {
        //SceneManager.LoadScene();
    }
}
