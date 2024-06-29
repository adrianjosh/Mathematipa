using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoiceText : MonoBehaviour
{
    TMP_Text text;
    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }
    public void SetSelected(bool selected)
    {
        text.color = (selected) ? GlobalSettings.i.HighlightedColor : Color.black;
    }

    public TMP_Text TextField => text;
}
