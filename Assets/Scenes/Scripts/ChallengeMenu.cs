using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ChallengeMenu : MonoBehaviour
{
    public Button challenge1Button; // Assign the Challenge 1 button in Inspector
    public TextMeshProUGUI timerText; // Assign the UI Text for countdown

    private const string Challenge1Key = "LastChallenge1Time"; // Key for PlayerPrefs
    //private const int cooldownDuration = 259200; // 3 days in seconds

    private const int cooldownDuration = 3; // 3 days in seconds
    public Transform draggedPrefab;

    void Start()
    {
        StartCoroutine(UpdateChallenge1Status());
    }

    public void goToChallenge1()
    {
        if (CanPlayChallenge1())
        {
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
            }
            PlayerPrefs.SetString(Challenge1Key, DateTime.UtcNow.ToString()); // Save playtime
            PlayerPrefs.Save();
            StartCoroutine(LoadSceneAfterSound(2));
        }
        else
        {
            Debug.Log("Challenge 1 is on cooldown!");
        }
    }

    private bool CanPlayChallenge1()
    {
        if (!PlayerPrefs.HasKey(Challenge1Key)) return true; // First time playing

        DateTime lastPlayTime = DateTime.Parse(PlayerPrefs.GetString(Challenge1Key));
        TimeSpan timePassed = DateTime.UtcNow - lastPlayTime;

        return timePassed.TotalSeconds >= cooldownDuration;
    }

    public void goToChallenge2()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }

        // Start the coroutine to wait for the sound to finish before loading the scene
        StartCoroutine(LoadSceneAfterSound(3));
    }

    // public void goToChallenge2(){
    //     if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
    //     {
    //         FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
    //     }

    //     // Start the coroutine to wait for the sound to finish before loading the scene
    //     StartCoroutine(LoadSceneAfterSound(3));
    // }

    // public void goToChallenge2(){
    //     if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
    //     {
    //         FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
    //     }

    //     // Start the coroutine to wait for the sound to finish before loading the scene
    //     StartCoroutine(LoadSceneAfterSound(3));
    // }



    private IEnumerator UpdateChallenge1Status()
    {
        bool shimmerPlayed = false; // To prevent repeated shimmer effects

        while (true) // Keep checking every second
        {
            if (CanPlayChallenge1())
            {
                challenge1Button.interactable = true;
                timerText.text = "✅ Challenge 1 is available!";
                timerText.color = Color.green;

                // Play shimmer effect once when challenge becomes available
                if (!shimmerPlayed)
                {
                    FindObjectOfType<ShimmerCollector>().CollectShimmer(draggedPrefab);
                    //FindObjectOfType<ShimmerCollector>().CollectShimmer();
                    shimmerPlayed = true; // Ensure it only plays once per unlock
                }
            }
            else
            {
                challenge1Button.interactable = false;
                DateTime lastPlayTime = DateTime.Parse(PlayerPrefs.GetString(Challenge1Key));
                TimeSpan remainingTime = TimeSpan.FromSeconds(cooldownDuration) - (DateTime.UtcNow - lastPlayTime);

                timerText.text = $"⏳ Available in {remainingTime.Days}d {remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
                timerText.color = Color.red;

                shimmerPlayed = false; // Reset when locked again
            }

            yield return new WaitForSeconds(1);
        }
    }


    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }
}
