using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncorrectAnswerSavable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText; // Reference to your TextMeshPro game object where the log will be displayed

    public static IncorrectAnswerSavable instance {  get; set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


}
