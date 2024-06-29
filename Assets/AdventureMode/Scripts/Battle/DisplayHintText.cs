using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayHintText : MonoBehaviour
{
    public TextMeshProUGUI hintText; // Reference to the UI Text element
    public GameObject letterBoxPrefab; // Prefab for the letter box
    public Transform letterBoxParent; // Parent transform for the letter boxes
    private GridLayoutGroup gridLayoutGroup; // Reference to GridLayoutGroup

    int letterBoxCount = 0;


    public static DisplayHintText instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Cache GridLayoutGroup component
        gridLayoutGroup = letterBoxParent.GetComponent<GridLayoutGroup>();
    }

    public void DisplayHint(string hint)
    {
        // Enable GridLayoutGroup if it's disabled and not destroyed
        if (gridLayoutGroup != null && !gridLayoutGroup.enabled)
        {
            if (gridLayoutGroup.gameObject != null)
            {
                gridLayoutGroup.enabled = true;
            }
        }

        // Clear existing letter boxes
        foreach (Transform child in letterBoxParent)
        {
            Destroy(child.gameObject);
        }

        // Ensure letter box prefab is not null
        if (letterBoxPrefab != null)
        {
            // Loop through each letter in the hint string
            for (int i = 0; i < hint.Length; i++)
            {
                // Create a new letter box
                GameObject letterBox = Instantiate(letterBoxPrefab, letterBoxParent);
                letterBoxCount = i+1;

                // Ensure the letter box was instantiated successfully
                if (letterBox != null)
                {
                    // Enable the parent UI image
                    Image parentImage = letterBox.GetComponent<Image>();
                    if (parentImage != null && parentImage.gameObject != null)
                    {
                        parentImage.enabled = true;
                    }

                    // Enable the layout element
                    LayoutElement layoutElement = letterBox.GetComponent<LayoutElement>();
                    if (layoutElement != null && layoutElement.gameObject != null)
                    {
                        layoutElement.enabled = true;
                    }

                    // Activate the TextMeshProUGUI component of the letter box
                    TextMeshProUGUI textMeshPro = letterBox.GetComponentInChildren<TextMeshProUGUI>();
                    if (textMeshPro != null && textMeshPro.gameObject != null)
                    {
                        textMeshPro.enabled = true;
                    }

                    // Set the text of the letter box to the corresponding letter from the hint
                    if (textMeshPro != null)
                    {
                        textMeshPro.text = hint[i].ToString();
                    }
                }
                else
                {
                    Debug.LogWarning("Failed to instantiate letter box at index " + i);
                }
            }
        }
        else
        {
            Debug.LogError("Letter box prefab is null.");
        }

        LetterBoxGridLayoutControl.instance.AdjustConstraintCount(letterBoxCount);
    }
}
