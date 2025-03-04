using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttonscript : MonoBehaviour
{
    public void goToLearn()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        // You can implement any other actions or conditions you need here
        StartCoroutine(LoadSceneAfterSound(10));
    }

    public void goToChallenge()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }

        // Start the coroutine to wait for the sound to finish before loading the scene
        StartCoroutine(LoadSceneAfterSound(4));
    }

    public void goToHome()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }

        // Start the coroutine to wait for the sound to finish before loading the scene
        StartCoroutine(LoadSceneAfterSound(0));
    }

    public void goToPractice()
    {
        // If you need to load a scene here, you can do it similarly:
        // StartCoroutine(LoadSceneAfterSound("yourSceneNameHere"));
    }

    // Coroutine to wait for the sound to finish
    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }
}
