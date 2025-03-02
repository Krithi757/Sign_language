using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class homeCont : MonoBehaviour
{
    public GameObject music;
    public GameObject musicDisabled;
    public GameObject soundEffectDisabled;
    public GameObject soundEffects;
    public TextMeshProUGUI challenge1StatusText;
    private int isMuted;
    private int isSoundEffectMuted;
    private bool settingsVisible;
    // Start is called before the first frame update

    private const string Challenge1Key = "LastChallenge1Time";
    private const int cooldownDuration = 259200; // 3 days in seconds
    private const string NotifyShownKey = "NotifyShown"; // Key for tracking notification display

    void Start()
    {
        music.SetActive(false);
        soundEffects.SetActive(false);
        musicDisabled.SetActive(false);
        soundEffectDisabled.SetActive(false);
        notify.SetActive(false); // Ensure it's hidden initially

        Debug.Log("Script has started");

        // Check if the notification should appear
        if (PlayerPrefs.GetInt(NotifyShownKey, 0) == 0) // If it's 0, it means it hasn't been shown yet
        {
            float randomDelay = UnityEngine.Random.Range(3f, 6f); // Random delay between 3 to 6 seconds
            Invoke(nameof(ShowNotifyPopup), randomDelay);
        }

        StartCoroutine(UpdateChallenge1Status());
    }

    private void ShowNotifyPopup()
    {
        notify.SetActive(true);
        PlayerPrefs.SetInt(NotifyShownKey, 1); // Mark it as shown
        PlayerPrefs.Save();

        // Play the pop-up sound if sound effects are enabled
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound");
        }
    }

    public void HideHelp()
    {
        notify.SetActive(false);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetInt(NotifyShownKey, 0); // Reset the flag when the game is closed
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        isMuted = PlayerPrefs.GetInt("AudioMuted"); // 
        Debug.Log(isMuted);
        isSoundEffectMuted = PlayerPrefs.GetInt("SoundEffectsMuted");
        Debug.Log(isSoundEffectMuted);
    }

    public void clickedSettings()
    {
        settingsVisible = !settingsVisible; // Toggle visibility

        if (settingsVisible)
        {
            // Load current states
            isMuted = PlayerPrefs.GetInt("AudioMuted", 1); // Default to 1 (enabled)
            isSoundEffectMuted = PlayerPrefs.GetInt("SoundEffectsMuted", 1); // Default to 1 (enabled)

            // Set UI elements based on saved state
            music.SetActive(isMuted == 1);
            musicDisabled.SetActive(isMuted == 0);
            soundEffects.SetActive(isSoundEffectMuted == 1);
            soundEffectDisabled.SetActive(isSoundEffectMuted == 0);
        }
        else
        {
            // Hide all UI elements when toggled off
            music.SetActive(false);
            musicDisabled.SetActive(false);
            soundEffects.SetActive(false);
            soundEffectDisabled.SetActive(false);
        }
    }


    public void disableMusic()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        // Set audio state to muted (0)
        PlayerPrefs.SetInt("AudioMuted", 0);
        PlayerPrefs.Save();

        // Update AudioManager to mute sounds
        FindObjectOfType<AudioManager>().MuteAllSounds(true);

        // Update UI elements based on the new state
        musicDisabled.SetActive(true);  // Show "disabled" icon when muted
        music.SetActive(false);  // Hide "enabled" icon when muted
    }

    public void enableMusic()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        // Set audio state to enabled (1)
        PlayerPrefs.SetInt("AudioMuted", 1);
        PlayerPrefs.Save();

        // Update AudioManager to unmute sounds
        FindObjectOfType<AudioManager>().MuteAllSounds(false);

        // Update UI elements based on the new state
        musicDisabled.SetActive(false);  // Hide "disabled" icon when unmuted
        music.SetActive(true);  // Show "enabled" icon when unmuted
    }

    public void disableSoundEffects()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        // Set audio state to muted (0)
        PlayerPrefs.SetInt("SoundEffectsMuted", 0);
        PlayerPrefs.Save();

        // Update AudioManager to mute sounds
        FindObjectOfType<AudioManager>().MuteAllSounds(true);

        // Update UI elements based on the new state
        soundEffectDisabled.SetActive(true);  // Show "disabled" icon when muted
        soundEffects.SetActive(false);  // Hide "enabled" icon when muted
    }

    public void enableSoundEffects()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        // Set audio state to enabled (1)
        PlayerPrefs.SetInt("SoundEffectsMuted", 1);
        PlayerPrefs.Save();

        // Update AudioManager to unmute sounds
        FindObjectOfType<AudioManager>().MuteAllSounds(false);

        // Update UI elements based on the new state
        soundEffectDisabled.SetActive(false);  // Hide "disabled" icon when unmuted
        soundEffects.SetActive(true);  // Show "enabled" icon when unmuted
    }






    public void gotoLevel()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(1));
    }
    public void gotoProgress()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(7));
    }
    public void gotoLearning()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(10));
    }
    public void gotoChallenge()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(4));
    }
    public void gotoPractice()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(7));
    }

    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }
}
