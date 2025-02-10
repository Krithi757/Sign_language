using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class homeCont : MonoBehaviour
{
    public GameObject music;
    public GameObject musicDisabled;
    public GameObject soundEffectDisabled;
    public GameObject soundEffects;
    private int isMuted;
    private int isSoundEffectMuted;
    private bool settingsVisible;
    // Start is called before the first frame update
    void Start()
    {
        music.SetActive(false);
        soundEffects.SetActive(false);
        musicDisabled.SetActive(false);
        soundEffectDisabled.SetActive(false);
        Debug.Log("Script has strated");
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
        SceneManager.LoadScene(SceneData.levelview);
    }
    public void gotoProgress()
    {
        SceneManager.LoadScene(SceneData.progress);
    }
    public void gotoLearning()
    {
        //SceneManager.LoadScene(SceneData);
    }
    public void gotoChallenge()
    {
        SceneManager.LoadScene(SceneData.challengeMenu);
    }
    public void gotoPractice()
    {
        //SceneManager.LoadScene(SceneData);
    }
}
