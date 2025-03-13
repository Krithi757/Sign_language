using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Globalization;


public class homeController : MonoBehaviour
{
    public GameObject music;
    public GameObject musicDisabled;
    public GameObject soundEffectDisabled;
    public GameObject soundEffects;
    public TextMeshProUGUI challenge1StatusText; // Assign in Inspector 

    public TextMeshProUGUI nameText;
    private int isMuted;
    private int isSoundEffectMuted;
    private bool settingsVisible;
    private const string NotifyShownKey = "NotifyShown";

    public GameObject notify;
    private const string Challenge1Key = "LastChallenge1Time";
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI scoreText;
    private const int cooldownDuration = 259200; // 3 days in seconds
    // Start is called before the first frame update
    void Start()
    {
        int totalCoins = PlayerPrefs.GetInt("AllCoins", 0);
        int totalDiamonds = PlayerPrefs.GetInt("AllDiamonds", 0);
        int score = PlayerPrefs.GetInt("Score", 0);

        coinText.text = totalCoins.ToString();
        diamondText.text = totalDiamonds.ToString();
        scoreText.text = score.ToString();
        if (PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName")))
        {
            // If PlayerName already exists, go to scene 9 


            string playerName = PlayerPrefs.GetString("PlayerName", "DefaultName"); // DefaultName is optional and used if PlayerName is not found

            // Log the PlayerName value
            Debug.Log("Player Name: " + playerName);
            nameText.text = "Welcome back, " + playerName;
        }
        else
        {
            nameText.gameObject.SetActive(false);
        }
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

    private IEnumerator UpdateChallenge1Status()
    {
        while (true)
        {
            if (CanPlayChallenge1())
            {
                challenge1StatusText.text = "Challenge 1 is available!";
            }
            else
            {
                DateTime lastPlayTime = DateTime.Parse(PlayerPrefs.GetString(Challenge1Key, DateTime.UtcNow.ToString()));
                TimeSpan remainingTime = TimeSpan.FromSeconds(cooldownDuration) - (DateTime.UtcNow - lastPlayTime);

                challenge1StatusText.text = $"Available in {remainingTime.Days}d {remainingTime.Hours}h {remainingTime.Minutes}m {remainingTime.Seconds}s";
            }

            yield return new WaitForSeconds(1); // Update every second
        }
    }

    public void HideHelp()
    {
        notify.SetActive(false);
    }

    private bool CanPlayChallenge1()
    {
        if (!PlayerPrefs.HasKey(Challenge1Key)) return true;

        DateTime lastPlayTime = DateTime.Parse(PlayerPrefs.GetString(Challenge1Key));
        TimeSpan timePassed = DateTime.UtcNow - lastPlayTime;

        return timePassed.TotalSeconds >= cooldownDuration;
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
        // Check if PlayerName exists in PlayerPrefs (indicating player has started the game before)
        if (PlayerPrefs.HasKey("PlayerName") && !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName")))
        {
            // If PlayerName already exists, go to scene 9 
            Debug.Log("Player name already set. Going to scene 9.");
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
            }
            StartCoroutine(LoadSceneAfterSound(7));  // Load scene 9
        }
        else
        {
            // If PlayerName is not set, load scene 11
            Debug.Log("Player name not set. Going to scene 11.");
            if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
            {
                FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
            }
            StartCoroutine(LoadSceneAfterSound(9));  // Load scene 11
        }
    }


    public void gotoProgress()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(6));
    }
    public void gotoLearning()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(8));
    }
    public void gotoChallenge()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(4));
    }

    public void gotoShopStore()
    {
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1)
        {
            FindObjectOfType<AudioManager>().PlaySound("TapSound"); // Play sound only once
        }
        StartCoroutine(LoadSceneAfterSound(10));
    }

    private IEnumerator LoadSceneAfterSound(int sceneId)
    {
        // Wait for the sound to finish playing (assuming "TapSound" has a defined duration)

        yield return new WaitForSeconds(0.3f);

        // Load the scene after the sound has finished
        SceneManager.LoadScene(sceneId);
    }
}