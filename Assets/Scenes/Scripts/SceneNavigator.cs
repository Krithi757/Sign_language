using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    private int[] sceneIndices = { 10, 11, 12, 13 };
    public GameObject popupPanel;
    
    private int currentSceneIndex = 0;

    private void Start()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        int activeSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Active Scene Build Index: " + activeSceneBuildIndex);

        // Find the current index in the scene list
        for (int i = 0; i < sceneIndices.Length; i++)
        {
            if (sceneIndices[i] == activeSceneBuildIndex)
            {
                currentSceneIndex = i;
                Debug.Log("Current Scene Index in Array: " + currentSceneIndex);
                PlayerPrefs.SetInt("LastSceneIndex", currentSceneIndex); // Store it persistently
                break;
            }
        }
    }

    public void GoToNextScene()
    {
        Debug.Log("GoToNextScene called. Current index before: " + currentSceneIndex);
        
        currentSceneIndex = (currentSceneIndex + 1) % sceneIndices.Length;
        PlayerPrefs.SetInt("LastSceneIndex", currentSceneIndex); // Store new index
        Debug.Log("New index after increment: " + currentSceneIndex + ", Loading scene: " + sceneIndices[currentSceneIndex]);

        SceneManager.LoadScene(sceneIndices[currentSceneIndex]);
    }

    public void GoToPreviousScene()
    {
        Debug.Log("GoToPreviousScene called. Current index before: " + currentSceneIndex);

        currentSceneIndex = (currentSceneIndex - 1 + sceneIndices.Length) % sceneIndices.Length;
        PlayerPrefs.SetInt("LastSceneIndex", currentSceneIndex); // Store new index
        Debug.Log("New index after decrement: " + currentSceneIndex + ", Loading scene: " + sceneIndices[currentSceneIndex]);

        SceneManager.LoadScene(sceneIndices[currentSceneIndex]);
    }

    public void ShowPopUp()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void GoToHome()
    {
        Debug.Log("GoToHome() called - Switching to Home scene");
        SceneManager.LoadScene(1);
    }
}
