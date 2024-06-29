using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClassicGameOver : MonoBehaviour
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
            Time.timeScale = 0f;
        }
    }

    public void MainMenu()
    {

        SceneManager.LoadScene(0);
    }

    public void Retry()
    {
        gameOverMenuUI.SetActive(false);
        GameIsOver = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
