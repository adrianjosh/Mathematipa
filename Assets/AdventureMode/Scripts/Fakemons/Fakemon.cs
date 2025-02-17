using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Fakemon
{
    [SerializeField] FakemonBase _base;
    [SerializeField] int level;

    public Fakemon(FakemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }
    public FakemonBase Base
    {
        get
        {
            return _base;
        }
    }
   
    public int Level
    {
        get
        {
            return level;
        }
    }

    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }

    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }

    public int StatusTime { get; set; }

    public Condition VolatileStatus { get; private set; }

    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; }

    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        //generate moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count >= FakemonBase.MaxNumberOfMoves)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(level);

        CalculateStats();

        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }

    public Fakemon(FakemonSaveData saveData)
    {
        _base = FakemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        }
        else
        {
            Status = null;
        }

        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;
    }

    public FakemonSaveData GetSaveData()
    {
        var saveData = new FakemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };
        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.CeilToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.CeilToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.CeilToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHP = MaxHp;
        MaxHp = Mathf.CeilToInt((Base.MaxHp * Level) / 100f) + 10 + Level;

        if (oldMaxHP != 0)
        {
            HP += MaxHp - oldMaxHP;
        }
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0 },
            {Stat.Defense, 0 },
            {Stat.Speed, 0 },
            {Stat.Accuracy, 0 },
            {Stat.Evasion, 0 }
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        //apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};

        if (boost >= 0)
        {
            statVal = Mathf.CeilToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.CeilToInt(statVal / boostValues[-boost]);
        }
        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            CalculateStats();
            return true;
        }

        return false;
    }
    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > FakemonBase.MaxNumberOfMoves)
        {
            return;
        }
        Moves.Add(new Move(moveToLearn));
    }

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
    }
    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public void Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        CalculateStats();
    }

    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    public void Heal()
    {
        HP = MaxHp;
        Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        OnHPChanged?.Invoke();
        

        CureStatus();
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Move move, Fakemon attacker)
    {
        //critical hit
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 1.5f;
        }

        //type effectiveness multiplier
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float playerAnswerInterval = BattleSystem.playerAnswerInterval;
        float answerLength = BattleSystem.answerLength;
        bool playerTurnOrder = BattleSystem.playerTurnOrder;

        float answerIntervalCalc;
        if (playerTurnOrder)
        {
            answerLength = answerLength / 50;
            answerLength += 1;

            if (BattleSystem.isTrainerBattle2)
            {
                if (playerAnswerInterval >= 30 && playerAnswerInterval <= 45)
                {
                    answerIntervalCalc = 1.2f;
                }
                else if (playerAnswerInterval >= 15 && playerAnswerInterval <= 30)
                {
                    answerIntervalCalc = 1.1f;
                }
                else
                {
                    answerIntervalCalc = 1f;
                }
            }
            else
            {
                answerIntervalCalc = 1f;
            }
            
        }
        else
        {
            answerLength = 1;
            answerIntervalCalc = 1f;
        }

        //damage calculation
        float modifiers = Random.Range(0.85f, 1f) * type * answerLength * answerIntervalCalc * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.CeilToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)
        {
            return;
        }

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null)
        {
            return;
        }

        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
    public void BoostStatsAfterLevelUp()
    {
        var oldMaxHp = MaxHp;
        CalculateStats();
        var diff = MaxHp - oldMaxHp;
        HP = HP + diff;

        OnHPChanged?.Invoke();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }

    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class FakemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}