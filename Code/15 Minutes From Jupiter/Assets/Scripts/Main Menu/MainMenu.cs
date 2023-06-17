using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    //-----------------------btn functions--------------------------
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
    }
    public void Options()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);

    }
    public void HighScores()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);

    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void Info()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 4);

    }
}
