using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    public GameObject wordPrefab;
    public Transform wordCanvas;

    

    public Vector3 positionSetter()
    {
        Vector3 randomPosition = new Vector3(Random.Range(-6.5f, 6.5f), 5f);

        return randomPosition;
    }
    public WordDisplay SpawnWord()
    {
        

        GameObject wordObj = Instantiate(wordPrefab, positionSetter(), Quaternion.identity, wordCanvas);
        WordDisplay wordDisplay = wordObj.GetComponent<WordDisplay>();


        return wordDisplay;
    }
}
