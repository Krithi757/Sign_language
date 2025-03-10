using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoaderScript : MonoBehaviour
{
    public Slider slider;             // Assign in Inspector
    public float loadingTime = 10f;   // Total time to load

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("LoadingMusic"); // Play sound only once
        }
        float elapsedTime = 0f;
        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            slider.value = elapsedTime / loadingTime; // Fill slider gradually
            yield return null; // Wait for next frame
        }

        SceneManager.LoadScene(1); // Change to your scene name
    }
}
