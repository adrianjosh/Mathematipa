using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        {
            //if there is a grid, then spawn at its center
            // para sa streets var spawnPos = new Vector3(14f, 23f, 0);
            var spawnPos = new Vector3(0, 0, 0);
            var grid = FindObjectOfType<Grid>();
            if (grid != null)
            {
                spawnPos = grid.transform.position;
            }
            
            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
