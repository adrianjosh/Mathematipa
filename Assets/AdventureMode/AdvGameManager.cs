using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class AdvGameManager : MonoBehaviour
{
    public AdvQuestion[] questions;
    private static List<AdvQuestion> unansweredQuestions;

    private AdvQuestion currentQuestion;
    public AdvReadInput readInput;

    [SerializeField] private Text tanongText;

    public TextMeshProUGUI output;

    [SerializeField] private float timeBetweenQuestions = 1f;
    private void Start()
    {
        if (unansweredQuestions == null || unansweredQuestions.Count == 0)
        {
            unansweredQuestions = questions.ToList<AdvQuestion>();
        }

        SetCurrentQuestion();
        output.text = currentQuestion.sagot;
        Debug.Log(currentQuestion.tanong + " is " + output.text);
        //Debug.Log(currentQuestion.tanong + " is " + currentQuestion.sagot);

    }

    void SetCurrentQuestion()
    {
        int randomQuestionIndex = Random.Range(0, unansweredQuestions.Count);
        currentQuestion = unansweredQuestions[randomQuestionIndex];

        tanongText.text = currentQuestion.tanong;



        
    }

    IEnumerator TransitionToNextQuestion()
    {
        unansweredQuestions.Remove(currentQuestion);

        yield return new WaitForSeconds(timeBetweenQuestions);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void InputedAnswer(string _answer)
    {
        if (_answer == currentQuestion.sagot)
        {
            Debug.Log("Correct!!!");

            StartCoroutine(TransitionToNextQuestion());
        }
        else
        {
            Debug.Log("ENGENGGGG!!!");
        }
    }
}
