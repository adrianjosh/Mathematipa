using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameTextTMP;
    [SerializeField] TMP_Text levelTextTMP;
    [SerializeField] HPBar hpBar;
    [SerializeField] TMP_Text messageTextTMP;

    Fakemon _fakemon;

    public void Init(Fakemon fakemon)
    {
        _fakemon = fakemon;
        UpdateData();
        SetMessage("");
        _fakemon.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameTextTMP.text = _fakemon.Base.Name;
        levelTextTMP.text = "Lvl " + _fakemon.Level;
        
        hpBar.SetHp((float)_fakemon.HP / _fakemon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameTextTMP.color = GlobalSettings.i.HighlightedColor;
        }
        else
        {
            nameTextTMP.color = Color.black;
        }
    }

    public void SetMessage(string message)
    {
        messageTextTMP.text = message;
    }
}
