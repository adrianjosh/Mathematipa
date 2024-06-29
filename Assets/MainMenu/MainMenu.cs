using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] float delay = 0.75f;
    [SerializeField] AudioClip sceneMusic;
    [SerializeField] public float loopStartPoint = 0f;
    [SerializeField] public float loopEndPoint = 0f;


    public static MainMenu i;

    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        AudioManager.i.PlayMusic(sceneMusic);
    }

    public void playClassicMode()
    {
        StartCoroutine(AudioManager.i.FadeMusic());

        StartCoroutine(DelayLoadingScene(delay, 2));


    }

    public void playEndlessMode()
    {

        StartCoroutine(AudioManager.i.FadeMusic());

        StartCoroutine(DelayLoadingScene(delay, 1));


        
    }
    public void playAdventureMode()
    {
        StartCoroutine(AudioManager.i.FadeMusic());

        StartCoroutine(DelayLoadingScene(delay, 3));
    }

    public void quitGame()
    {
        Application.Quit();
    }

    IEnumerator DelayLoadingScene(float delay, int indexNum)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(indexNum);
    }
}
