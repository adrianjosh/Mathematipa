using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IncorrectAnswerLogger : MonoBehaviour
{
    private Dictionary<string, List<string>> incorrectAnswers = new Dictionary<string, List<string>>();
    public TextMeshProUGUI logText; // Reference to your TextMeshPro game object where the log will be displayed

    public static IncorrectAnswerLogger Instance { get; set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Call this function whenever an incorrect answer is detected
    public void LogIncorrectAnswer(string question, string userAnswer,string correctAnswer)
    {
        if (!incorrectAnswers.ContainsKey(question))
        {
            incorrectAnswers.Add(question, new List<string>());
        }

        string entry = $"Your Answer: {userAnswer} (Correct Answer: {correctAnswer})";
        incorrectAnswers[question].Add(entry);

        // Update the display with the updated log
        UpdateLogDisplay();
    }

    // Function to update the display with the log of incorrect answers
    public void UpdateLogDisplay()
    {
        // Clear the existing text
        logText.text = "";

        // Iterate through the dictionary and append each entry to the log text
        foreach (KeyValuePair<string, List<string>> entry in incorrectAnswers)
        {
            logText.text += "<b>Question:</b> " + entry.Key + "\n";
            foreach (string answer in entry.Value)
            {
                logText.text += answer + "\n";
            }
            logText.text += "\n";
        }

        // If log text is empty, display a message indicating no wrong answers
        if (logText.text == "")
        {
            logText.text = "No wrong answers currently, you must be smart!";
        }
    }

    public bool IsQuestionLogged(string question)
    {
        // Check if the question is already logged
        return incorrectAnswers.ContainsKey(question);
    }

    public void RemoveIncorrectAnswer(string question)
    {
        // Remove the incorrect answer from the log
        incorrectAnswers.Remove(question);
        // Update the display after removing the incorrect answer
        UpdateLogDisplay();
    }

}