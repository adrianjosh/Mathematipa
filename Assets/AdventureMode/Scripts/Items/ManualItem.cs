using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Manual or Secret Manual")]
public class ManualItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isSM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Fakemon fakemon)
    {
        // learning move is handled from Invetory UI, if it was learned then return true
        return fakemon.HasMove(move);
    }

    public bool CanBeTaught(Fakemon fakemon)
    {
        return fakemon.Base.LearnableByItems.Contains(Move);
    }
    public override bool IsReusable => isSM;

    public override bool CanUseInBattle => false;

    public MoveBase Move => move;
    public bool IsSM => isSM;
}
