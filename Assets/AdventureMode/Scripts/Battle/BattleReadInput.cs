using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleReadInput : MonoBehaviour
{
    private string battleInput;
    public BattleSystem questionManager;

    private void Update()
    {
        if (BattleTimer.i.isTimerRunning)
        {
            if (BattleTimer.i.currentTime <= 0.1)
            {
                questionManager.InputedBattleAnswer(battleInput);
            }
        }
        
    }

    public void ReadStringBattleInput(string _input)
    {

        battleInput = _input.ToUpper();

        Debug.Log(battleInput);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            questionManager.InputedBattleAnswer(battleInput);
        }

        

        //reset input field to blank on end
        questionManager.inputField.text = "";

    }
    public void OnValueChange(string _input)
    {
        battleInput = _input.ToUpper();
    }
}
