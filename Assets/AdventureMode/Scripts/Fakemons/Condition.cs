using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    public Action<Fakemon> OnStart { get; set; }
    public Func<Fakemon, bool> OnBeforeMove { get; set; }

    public Action<Fakemon> OnAfterTurn { get; set; }
}
