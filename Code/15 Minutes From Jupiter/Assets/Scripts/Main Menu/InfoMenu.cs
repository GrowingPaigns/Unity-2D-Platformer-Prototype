using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoMenu : MonoBehaviour
{
    public void SamGit()
    {
        Application.OpenURL("https://github.com/GrowingPaigns/");
    }

    public void AlexGit()
    {
        Application.OpenURL("https://github.com/Alexander-Flores-Martinez/");
    }

    public void SamLinkedin()
    {
        Application.OpenURL("https://www.linkedin.com/in/samuel-hilfer/");
    }

    public void AlexLinkedin()
    {
        Application.OpenURL("https://www.linkedin.com/in/alexander-emanuel-flores-martinez-315ab7270/");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 4);

    }
}
