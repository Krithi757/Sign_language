using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;
    private bool isPaused = false;
    private bool mute = false;
    private Dictionary<string, bool> soundPauseState = new Dictionary<string, bool>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
        }
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Play main theme only if NOT Scene 10
        if (PlayerPrefs.GetInt("SoundEffectsMuted", 1) == 1 && currentSceneIndex != 9)
        {
            PlaySound("MainTheme ");
        }

    }

    public void PlaySound(string name)
    {
        foreach (Sound s in sounds)
        {
            if (s.name == name)
            {
                s.source.Play();
            }
        }
    }
    public void MuteAllSounds(bool mute)
    {
        foreach (Sound s in sounds)
        {
            if (mute)
                s.source.Stop();
            else
                s.source.Play();
        }
    }

    public void PauseAllSounds()
    {
        if (!isPaused)  // Prevent double pausing
        {
            foreach (Sound s in sounds)
            {
                if (s.source.isPlaying)
                {
                    Debug.Log("Pausing sound: " + s.name);
                    s.source.Pause();  // Pause the sound
                    soundPauseState[s.name] = true;  // Track that this sound is paused
                }
            }
            isPaused = true;  // Set the global pause flag
        }
    }

    public void ResumeAllSounds()
    {
        if (isPaused)  // Prevent double resuming
        {
            foreach (Sound s in sounds)
            {
                if (soundPauseState.ContainsKey(s.name) && soundPauseState[s.name])
                {
                    s.source.UnPause();  // Resume the sound
                    soundPauseState[s.name] = false;  // Reset the pause state
                }
            }
            isPaused = false;  // Set the global resume flag
        }
    }
    private AudioClip GetAudioClipByName(string soundName)
    {
        // Retrieve your AudioClip by its name
        // For example, from a dictionary or an array of sounds
        // This is just an example; modify based on your actual setup
        return Resources.Load<AudioClip>("Audio/" + soundName);
    }

}
