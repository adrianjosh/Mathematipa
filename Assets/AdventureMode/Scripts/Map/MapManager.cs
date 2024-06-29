using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [SerializeField] private GameObject _miniMap;
    [SerializeField] private GameObject _largeMap;

    public bool IsLargeMapOpen { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        //CloselargeMap();
    }

    private void Update()
    {

    }

    public void OpenlargeMap()
    {
        _miniMap.SetActive(false);
        _largeMap.SetActive(true);
        IsLargeMapOpen = true;
    }

    public void CloselargeMap()
    {
        _miniMap.SetActive(true);
        _largeMap.SetActive(false);
        IsLargeMapOpen = false;
        GameController.Instance.StartFreeRoamState();
    }
}
