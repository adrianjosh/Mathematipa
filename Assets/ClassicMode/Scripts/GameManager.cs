using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    //int grade5Countdown;
    public Question[] questions;
    private static List<Question> unansweredQuestions;

    private Question currentQuestion;
    public ReadInput readInput;

    [SerializeField] private Text tanongText;
    [SerializeField] private TextMeshProUGUI logText; // Reference to your TextMeshPro game object where the log will be displayed

    private IncorrectAnswerLogger answerLogger; // Reference to the IncorrectAnswerLogger script


    [SerializeField] private float timeBetweenQuestions = 1f;
    private void Start()
    {
        answerLogger = GetComponent<IncorrectAnswerLogger>(); // Get reference to IncorrectAnswerLogger script

        if (unansweredQuestions == null || unansweredQuestions.Count == 0)
        {
            unansweredQuestions = questions.ToList<Question>();
        }
        //Debug.Log("Countdown to Grade 5: " + grade5Countdown);
        SetCurrentQuestion();
        
        //Debug.Log("Countdown to Grade 5: " + grade5Countdown);
    }

    void SetCurrentQuestion()
    {
        

        int randomQuestionIndex = Random.Range(0, unansweredQuestions.Count);
        currentQuestion = unansweredQuestions[randomQuestionIndex];

        tanongText.text = currentQuestion.tanong;

        Debug.Log(currentQuestion.tanong + " is " + currentQuestion.sagot);
        //grade5Countdown++;

    }

    IEnumerator TransitionToNextQuestion()
    {
        unansweredQuestions.Remove(currentQuestion);

        if (unansweredQuestions == null || unansweredQuestions.Count == 0)
        {
            unansweredQuestions = questions.ToList<Question>();
            Debug.Log("0 alr");
        }

        yield return new WaitForSeconds(timeBetweenQuestions);

        SetCurrentQuestion();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void InputedAnswer(string _answer)
    {
        if (_answer == currentQuestion.sagot)
        {
            Debug.Log("Correct!!!");

            if (answerLogger.IsQuestionLogged(currentQuestion.tanong))
            {
                // Remove the incorrect answer from the log
                answerLogger.RemoveIncorrectAnswer(currentQuestion.tanong);
            }


            StartCoroutine(TransitionToNextQuestion());
        }
        else
        {
            Debug.Log("ENGENGGGG!!!");

            answerLogger.LogIncorrectAnswer(currentQuestion.tanong, _answer,currentQuestion.sagot);
            StartCoroutine(TransitionToNextQuestion());
        }
    }
}
