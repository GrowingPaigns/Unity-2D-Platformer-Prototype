using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MainMenu
{
    public AudioMixer mixer;
    private AudioSource _audioSource;
    private GameObject[] other;
    private bool NotFirst = false;
    private void Awake()
    {

        if (PlayerPrefs.HasKey("MasterVol"))
        {
            mixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
        }
        if (PlayerPrefs.HasKey("MusicVol"))
        {
            mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
        }
        if (PlayerPrefs.HasKey("SFXVol"))
        {
            mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
        }


        //Music Stuff
        other = GameObject.FindGameObjectsWithTag("Music");

        foreach (GameObject oneOther in other)
        {
            if (oneOther.scene.buildIndex == -1)
            {
                NotFirst = true;
            }
        }

        if (NotFirst == true)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(transform.gameObject);
        _audioSource = GetComponent<AudioSource>();
    }


    private void Update()
    {

        //Music Stuff
        if (SceneManager.GetSceneByName("FirstLevel").isLoaded)
        {
            StopMusic();
        }
    }


    public void PlayMusic()
    {
        if (_audioSource.isPlaying) return;
        _audioSource.Play();
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }
}
