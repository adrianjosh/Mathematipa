using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{

    public GameObject gameOverMenuUI;

    public static bool GameIsOver;
    // Start is called before the first frame update

    private void Awake()
    {
        GameIsOver = false;
    }
    void Start()
    {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameIsOver)
        {
            gameOverMenuUI.SetActive(true);
            Time.timeScale = 1f;
        }
    }

    public void MainMenu()
    {
        GameIsOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Retry()
    {
        gameOverMenuUI.SetActive(false);
        GameIsOver = false;
        Time.timeScale = 1;
        AudioManager.i.PlayMusic(WordManager.i.sceneMusic);
        //StartCoroutine(AudioManager.i.UnPauseMusic(0));
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
