using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;

    public float currentTime = 0f;
    float startingTime = 45f;
    public bool isTimerRunning = false; // Track if the timer is currently running

    public static BattleTimer i { get; set; }
    private void Awake()
    {
        i = this;
    }

    void Update()
    {
        if (isTimerRunning) // Only update the timer if it's running
        {
            currentTime -= 1 * Time.deltaTime;
            timerText.text = currentTime.ToString("0");

            if (currentTime <= 10)
            {
                timerText.color = Color.red;
            }

            if (currentTime <= 0)
            {
                isTimerRunning = false; // Stop the timer when it reaches zero
            }
            BattleSystem.playerAnswerInterval = currentTime;
        }
    }

    // Function to stop the timer
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // Function to reset the timer to 0
    public void ResetTimer()
    {
        currentTime = 0;
        timerText.color = Color.black;
        timerText.text = currentTime.ToString("0");
        isTimerRunning = false; // Stop the timer after resetting to 0
    }

    public void StartTimer()
    {
        currentTime = startingTime;
        isTimerRunning = true;
    }
}
