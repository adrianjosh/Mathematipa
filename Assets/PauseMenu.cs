using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    Coroutine coroutine;

    public static PauseMenu instance;
    private void Awake()
    {
        instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
            if (GameIsPaused)
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
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        StartCoroutine(AudioManager.i.UnPauseBGM());
        GameIsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        StartCoroutine(AudioManager.i.PauseBGM());
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1;
        GameIsPaused = false;

        SceneManager.LoadScene(0);
    }
}
