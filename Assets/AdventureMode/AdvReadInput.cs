using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvReadInput : MonoBehaviour
{
    public string input;
    public AdvGameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void ReadStringInput(string _input)
    {
        
        input = _input.ToUpper();

        Debug.Log(input);

        gameManager.InputedAnswer(input);

    }
}
