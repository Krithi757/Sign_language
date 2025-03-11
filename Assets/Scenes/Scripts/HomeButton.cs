using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    // The build index of your Home scene
    [SerializeField] private int homeSceneIndex = 1;
    public GameObject popup;

    public void Start(){
        popup.SetActive(false);
    }

    // Method to be called by the Home button
    public void GoToHome()
    {
        Debug.Log("Clciked button"); 
        popup.SetActive(true);
        SceneManager.LoadScene(homeSceneIndex);
    }
}