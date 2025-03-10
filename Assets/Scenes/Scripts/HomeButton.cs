using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    // The build index of your Home scene
    [SerializeField] private int homeSceneIndex = 1;

    // Method to be called by the Home button
    public void GoToHome()
    {
        SceneManager.LoadScene(homeSceneIndex);
    }
}