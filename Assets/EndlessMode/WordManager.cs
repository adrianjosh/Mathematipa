using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordManager : MonoBehaviour
{
    [SerializeField] public AudioClip sceneMusic;
    [SerializeField] public float loopStartPoint = 0f;
    [SerializeField] public float loopEndPoint = 0f;

    public List<Word> words;

    public WordSpawner wordSpawner;

    private bool hasActiveWord;
    private Word activeWord;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;
    private int score = 0;

    public Transform mTargetObject;
    WordDisplay display;

    public static WordManager i;
    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
    }

    private void Start()
    {
        highscoreText.text = $"Highscore: {PlayerPrefs.GetInt("HighScore", 0)}";
        AudioManager.i.PlayMusic(sceneMusic);
    }
    private void Update()
    {
        scoreText.text = $"Score: {score}";
    }


    public void CleanChildren()
    {
        
        foreach (Word word in words)
        {
            Destroy(word.display.gameObject);
        }
        words.Clear();
        score = 0;
        hasActiveWord = false;
        StartCoroutine(AudioManager.i.StopBGM());
    }

    public void AddWord()
    {
        Word word = new Word(WordGenerator.GetRandomWord(), wordSpawner.SpawnWord());
        Debug.Log(word.word);


        words.Add(word);
    }

    public void TypeLetter (char letter)
    {
        if (hasActiveWord)
        {
            if (activeWord.GetNextLetter() == letter)
            {
                activeWord.TypeLetter();
            }
        } else
        {
            foreach(Word word in words)
            {
                if (word.GetNextLetter() == letter)
                {
                    activeWord = word;
                    hasActiveWord = true;
                    word.TypeLetter();
                    break;
                }
            }
        }

        if (hasActiveWord && activeWord.WordTyped())
        {
            hasActiveWord = false;
            words.Remove(activeWord);
            ++score;

            if (score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", score);
                highscoreText.text = $"Highscore: {score}";
            }
        }
    }
}
