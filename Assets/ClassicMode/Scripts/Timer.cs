using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;

    float currentTime = 0f;
    float startingTime = 1130f;
    // Start is called before the first frame update
    void Start()
    {
        currentTime = startingTime;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime -= 1 * Time.deltaTime;
        timerText.text = currentTime.ToString("0");

        if (currentTime <= 10)
        {
            timerText.color = Color.red;
        }

        if (currentTime <= 0)
        {
            currentTime = 0;
            ClassicGameOver.GameIsOver = true;
        }

    }
}
