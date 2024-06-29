using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class DontDestroy : MonoBehaviour
{
    [HideInInspector]
    public string objectID;
    private void Awake()
    {
        objectID = name + transform.position.ToString() + transform.eulerAngles.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Check if this object already exists in the scene
        DontDestroy[] existingObjects = Object.FindObjectsOfType<DontDestroy>();
        foreach (DontDestroy existingObject in existingObjects)
        {
            if (existingObject != this && existingObject.objectID == objectID)
            {
                // If an object with the same ID already exists, destroy this object
                Destroy(gameObject);
                return; // Exit the method to prevent further execution
            }
        }

        // If this object is unique, make sure it persists between scenes
        DontDestroyOnLoad(gameObject);

        // Subscribe to the scene loading event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Method to handle scene loading event
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is the main menu scene
        if (scene.buildIndex == 0) // Assuming main menu is at build index 0
        {
            // Destroy this object when transitioning to the main menu
            Destroy(gameObject);
        }
    }

    // Unsubscribe from the scene loading event when the object is destroyed
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
