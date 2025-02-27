using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;
    private bool mute = false;

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

    public float GetSoundDuration(string soundName)
    {
        // Implement your logic to get the sound clip duration
        // Example: Assuming each sound is an AudioClip
        AudioClip clip = GetAudioClipByName(soundName); // Get the clip by name
        if (clip != null)
        {
            return clip.length;  // Return the duration of the clip in seconds
        }
        return 0f; // Default value if clip is not found
    }

    private AudioClip GetAudioClipByName(string soundName)
    {
        // Retrieve your AudioClip by its name
        // For example, from a dictionary or an array of sounds
        // This is just an example; modify based on your actual setup
        return Resources.Load<AudioClip>("Audio/" + soundName);
    }

}
