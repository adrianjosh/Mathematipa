using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {   
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Fakemon fakemon) =>
                {
                    fakemon.DecreaseHP(fakemon.MaxHp / 8);
                    fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Fakemon fakemon) =>
                {
                    fakemon.DecreaseHP(Mathf.CeilToInt(fakemon.MaxHp / 16));

                    //fallback if dmg = 0
                    if (fakemon.MaxHp / 16 <= 1)
                    {
                        fakemon.DecreaseHP(fakemon.MaxHp / 16 + 1);
                    }
                    fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} hurt itself due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Fakemon fakemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name}'s paralyzed and can't move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Fakemon fakemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        fakemon.CureStatus();
                        fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} broke the ice");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Fakemon fakemon) =>
                {
                    //sleep for 1-3 turns
                    fakemon.StatusTime = Random.Range(1, 4);
                },
                OnBeforeMove = (Fakemon fakemon) =>
                {
                    if (fakemon.StatusTime <= 0)
                    {
                        fakemon.CureStatus();
                        fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} woke up!");
                        return true;
	                }
                    fakemon.StatusTime--;
                    fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} is sleeping");
                    return false;
                }
            }
        },

        //Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Fakemon fakemon) =>
                {
                    //confused for 1-4 turns
                    fakemon.VolatileStatusTime = Random.Range(1, 5);
                },
                OnBeforeMove = (Fakemon fakemon) =>
                {
                    if (fakemon.VolatileStatusTime <= 0)
                    {
                        fakemon.CureVolatileStatus();
                        fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} has been cured of confusion!");
                        return true;
                    }
                    fakemon.VolatileStatusTime--;

                    //50% chance to do a move
                    if (Random.Range(1, 3) == 1)
                    {
                        return true;
                    }

                    //get hurt by confusion
                    fakemon.StatusChanges.Enqueue($"{fakemon.Base.Name} is confused");
                    fakemon.DecreaseHP(fakemon.MaxHp / 8);
                    fakemon.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
        {
            return 1.5f;
        }
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
        {
            return 2f;
        }
        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
