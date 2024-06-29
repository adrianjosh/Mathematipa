using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameTextTMP;
    [SerializeField] TMP_Text levelTextTMP;
    [SerializeField] TMP_Text statusTextTMP;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Fakemon _fakemon;

    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Fakemon fakemon)
    {
        if (_fakemon != null)
        {
            _fakemon.OnHPChanged -= UpdateHP;
            _fakemon.OnStatusChanged -= SetStatusText;
        }

        _fakemon = fakemon;

        nameTextTMP.text = fakemon.Base.Name;
        SetLevel();
        hpBar.SetHp((float) fakemon.HP / fakemon.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor }
        };
        
        SetStatusText();
        _fakemon.OnStatusChanged += SetStatusText;
        _fakemon.OnHPChanged += UpdateHP;

    }

    void SetStatusText()
    {
        if (_fakemon.Status == null)
        {
            statusTextTMP.text = "";
        }
        else
        {
            statusTextTMP.text = _fakemon.Status.Id.ToString().ToUpper();
            statusTextTMP.color = statusColors[_fakemon.Status.Id];
        }
        
    }

    public void SetLevel()
    {
        levelTextTMP.text = "Lvl " + _fakemon.Level;
    }

    public void SetExp()
    {
        if (expBar == null)
        {
            return;
        }
        float normalizedExp =GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null)
        {
            yield break;
        }
        if (reset)
        {
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }
        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _fakemon.Base.GetExpForLevel(_fakemon.Level);
        int nextLevelExp = _fakemon.Base.GetExpForLevel(_fakemon.Level + 1);

        float normalizedExp = (float)(_fakemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }
    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_fakemon.HP / _fakemon.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public void ClearData()
    {
        if (_fakemon != null)
        {
            _fakemon.OnHPChanged -= UpdateHP;
            _fakemon.OnStatusChanged -= SetStatusText;
        }
    }
}
