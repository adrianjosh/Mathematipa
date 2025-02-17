using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordTimer : MonoBehaviour
{
    public WordManager wordManager;

    public float wordDelay = 4f;
    private float nextWordTime = 0f;

    private void Update()
    {
        if(Time.time >= nextWordTime && !GameOverMenu.GameIsOver)
        {
            wordManager.AddWord();
            nextWordTime = Time.time + wordDelay;
            //Debug.Log(Time.time);
            wordDelay *= .99f;
        }
        if (GameOverMenu.GameIsOver)
        {
            wordDelay = 4f;
        }
    }
}
