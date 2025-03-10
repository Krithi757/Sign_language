using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // Array of scene build indices that we want to navigate between
    private int[] sceneIndices = { 10, 11, 12, 13 };
    private int currentSceneIndex = 0; // Position in our array, not the actual scene index

    private void Start()
    {
        // Find the current position in our array based on active scene
        int activeSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Active Scene Build Index: " + activeSceneBuildIndex);
        
        for (int i = 0; i < sceneIndices.Length; i++)
        {
            if (sceneIndices[i] == activeSceneBuildIndex)
            {
                currentSceneIndex = i;
                Debug.Log("Current Scene Index in Array: " + currentSceneIndex);
                break;
            }
        }
    }

    public void GoToNextScene()
    {
        Debug.Log("GoToNextScene called. Current index before: " + currentSceneIndex);
        // Increment and wrap around if necessary
        currentSceneIndex = (currentSceneIndex + 1) % sceneIndices.Length;
        Debug.Log("New index after increment: " + currentSceneIndex + ", Loading scene: " + sceneIndices[currentSceneIndex]);
        SceneManager.LoadScene(sceneIndices[currentSceneIndex]);
    }

    public void GoToPreviousScene()
    {
        Debug.Log("GoToPreviousScene called. Current index before: " + currentSceneIndex);
        // Decrement and wrap around if necessary
        currentSceneIndex = (currentSceneIndex - 1 + sceneIndices.Length) % sceneIndices.Length;
        Debug.Log("New index after decrement: " + currentSceneIndex + ", Loading scene: " + sceneIndices[currentSceneIndex]);
        SceneManager.LoadScene(sceneIndices[currentSceneIndex]);
    }
    
    // Add this method to test the Next button functionality directly
    public void TestNextButton()
    {
        Debug.Log("Test Next Button pressed");
        GoToNextScene();
    }
}