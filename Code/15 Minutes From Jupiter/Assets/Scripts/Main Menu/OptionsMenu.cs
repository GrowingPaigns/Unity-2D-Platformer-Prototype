using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class OptionsMenu : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public List<ResItem> resolutions = new List<ResItem>();
    private int selectedResolution;
    
    //public AudioMixer mixer;
    //public Slider masterSlider, musicSlider, sfxSlider;


    // Start is called before the first frame update
    private void Start()
    {


        bool foundRes = false;
        for(int i =0; i < resolutions.Count; i++)
        {
            if(Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
            {
                foundRes = true;
                selectedResolution = i;
                UpdateResLabel();
            }
        }

        if (!foundRes)
        {
            ResItem newRes = new ResItem();
            newRes.horizontal = Screen.width;
            newRes.vertical = Screen.height;
            
            resolutions.Add(newRes);
            selectedResolution = resolutions.Count - 1;
            
            UpdateResLabel();

        }
        //float vol = 0f;
        //mixer.GetFloat("MasterVol", out vol);
        //masterSlider.value = vol;
        //mixer.GetFloat("MusicVol", out vol);
        //musicSlider.value = vol;
        //mixer.GetFloat("SFXVol", out vol);
        //sfxSlider.value = vol;
    }


    public void ResRight()
    {
        selectedResolution--;
        if(selectedResolution < 0)
        {
            selectedResolution = resolutions.Count - 1;
        }
        UpdateResLabel();
    }

    public void ResLeft()
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Count -1)
        {
            selectedResolution = resolutions.Count-3;
        }
        UpdateResLabel();

    }

    public void UpdateResLabel()
    {

    }





    public void ApplyGraphics()
    {
    }

    public void SetMasterVolume()
    {
        //mixer.SetFloat("MasterVol", masterSlider.value);
        //PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
    }
    public void SetMusicVolume()
    {
        //mixer.SetFloat("MusicVol", musicSlider.value);
        //PlayerPrefs.SetFloat("MusicVol", musicSlider.value);
    }
    public void SetSFXVolume()
    {
        //mixer.SetFloat("SFXVol", sfxSlider.value);
        //PlayerPrefs.SetFloat("SFXVol", sfxSlider.value);
    }


    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);

    }
}
[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}
