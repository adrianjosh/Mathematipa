using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoveryAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Fakemon fakemon)
    {
        // revive
        if (revive || maxRevive)
        {
            if (fakemon.HP > 0)
            {
                return false;
            }
            if (revive)
            {
                fakemon.IncreaseHP(fakemon.MaxHp / 2);
            }
            else if (true)
            {
                fakemon.IncreaseHP(fakemon.MaxHp);
            }

            fakemon.CureStatus();

            return true;
        }

        // no other items can be used on fainted fakemon
        if (fakemon.HP == 0)
        {
            return false;
        }

        //restore hp
        if (restoreMaxHP || hpAmount > 0)
        {
            if (fakemon.HP == fakemon.MaxHp)
            {
                return false;
            }
            if (restoreMaxHP)
            {
                fakemon.IncreaseHP(fakemon.MaxHp);
            }
            else
            {
                fakemon.IncreaseHP(hpAmount);
            }

            fakemon.IncreaseHP(hpAmount);
        }

        // restore status
        if (recoveryAllStatus || status != ConditionID.none)
        {
            if (fakemon.Status == null && fakemon.VolatileStatus == null)
            {
                return false;
            }

            if (recoveryAllStatus)
            {
                fakemon.CureStatus();
                fakemon.CureVolatileStatus();
            }
            else
            {
                if (fakemon.Status.Id == status)
                {
                    fakemon.CureStatus();
                }
                else if (fakemon.VolatileStatus.Id == status)
                {
                    fakemon.CureVolatileStatus();
                }
                else
                {
                    return false;
                }
            }

        }

        //restore pp
        if (restoreMaxPP)
        {
            fakemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount >0)
        {
            fakemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
