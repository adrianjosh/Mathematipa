using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tmp_Object;

    public Text text;
    public float fallSpeed = 1f;

    public void SetWord (string word)
    {
        tmp_Object.text = word;

        //text.text = word;
    }

    public void RemoveLetter()
    {
        tmp_Object.text = tmp_Object.text.Remove(0, 1);
        tmp_Object.color = Color.red;
    }

    public void RemoveWord()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (!PauseMenu.GameIsPaused || !GameOverMenu.GameIsOver)
        {
            transform.Translate(0f, -fallSpeed * Time.deltaTime, 0f);
        }
    }
}
