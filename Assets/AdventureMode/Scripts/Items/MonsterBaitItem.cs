using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new monster bait")]
public class MonsterBaitItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;
    public override bool Use(Fakemon fakemon)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifier => catchRateModifier;
}
