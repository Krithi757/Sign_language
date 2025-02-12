using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (PlayerPrefs.GetInt("AudioMuted", 1) == 1)
            {
                PlaySound("MainTheme ");
            }
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

}
