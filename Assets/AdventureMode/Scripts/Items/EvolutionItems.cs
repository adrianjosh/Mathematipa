using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Create new evolution item")]
public class EvolutionItems : ItemBase
{
    public override bool Use(Fakemon fakemon)
    {
        return true;
    }
}
