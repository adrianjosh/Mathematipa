using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorTrigger : MonoBehaviour
{
    public MapController mapController;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            mapController.EnterIndoors();
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            mapController.ExitIndoors();
        }
    }
}
