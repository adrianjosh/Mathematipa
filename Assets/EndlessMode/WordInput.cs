using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordInput : MonoBehaviour
{

    public WordManager wordManager;
    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.GameIsPaused)
        {
            PauseMenu.instance.Pause();
        }
        else
        {
            foreach (char letter in Input.inputString)
            {
                wordManager.TypeLetter(letter);
                Debug.Log(letter);
            }
        }

    }
}
