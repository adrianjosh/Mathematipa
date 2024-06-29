using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MapController : MonoBehaviour
{
    public GameObject mapUIPrefab;
    public Camera mapCamera;

    private bool indoors = false;

    private void Start()
    {
        //mapCamera.orthographicSize = 20f;
    }

    private void Update()
    {
        if (!indoors)
        {
            mapUIPrefab.SetActive(true);
        }
        else
        {
            mapUIPrefab.SetActive(false);
        }
    }

    public void EnterIndoors()
    {
        indoors = true;
    }

    public void ExitIndoors()
    {
        StartCoroutine(DelayMapUI());
        
        // Optionally: Resume updating the map UI or show it
    }

    IEnumerator DelayMapUI()
    {
        yield return new WaitForSeconds(0.5f);
        indoors = false;
    }

}
