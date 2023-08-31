using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool gamePaused = false;
    [SerializeField] private GameObject pauseMenu;
    private AttackAnimationManager aimAttack;
    private PlayerAttack playerAttack;

    void Start()
    {
        aimAttack = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<AttackAnimationManager>();
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAttack>();
        aimAttack.enabled = true;
        playerAttack.enabled = true;
        Resume();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;

        aimAttack.enabled = true;
        playerAttack.enabled = true;
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        gamePaused = true;

        aimAttack.enabled = false;
        playerAttack.enabled = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

    }
}
