using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterBoxGridLayoutControl : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup; // Reference to the GridLayoutGroup component
    public int defaultConstraintCount = 3; // Default constraint count
    public int minLetterBoxCount = 18; // Maximum number of letter boxes before constraint count increases

    public static LetterBoxGridLayoutControl instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Set the default constraint count
        SetConstraintCount(defaultConstraintCount);
    }

    public void SetConstraintCount(int constraintCount)
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayoutGroup.constraintCount = constraintCount;
        }
        else
        {
            Debug.LogWarning("Grid Layout Group component not assigned.");
        }
    }

    public void AdjustConstraintCount(int letterBoxCount)
    {
        // Check if letter box count exceeds the maximum count
        if (letterBoxCount < minLetterBoxCount)
        {
            // Increase the constraint count to 3
            SetConstraintCount(2);
        }
        else
        {
            // Use the default constraint count
            SetConstraintCount(defaultConstraintCount);
        }
    }
}
