using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fakemon", menuName = "Fakemon/Create new fakemon")]
public class FakemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] FakemonType type1;
    [SerializeField] FakemonType type2;

    // Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;

    public static int MaxNumberOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return (level * level * level);
        }

        return -1;
    }

    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public FakemonType Type1
    {
        get { return type1; }
    }
    public FakemonType Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public List<MoveBase> LearnableByItems => learnableByItems;
    public List<Evolution> Evolutions => evolutions;

    public int CatchRate => catchRate;
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base {
        get { return moveBase; }
    }
    public int Level {
        get { return level; } 
    }
}

[System.Serializable]
public class Evolution
{
    [SerializeField] FakemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItems requiredItem;

    public FakemonBase EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public EvolutionItems RequiredItem => requiredItem;
}

public enum FakemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Poison
}

public enum GrowthRate
{
    Fast, MediumFast
}

public enum Stat
{
    Attack,
    Defense,
    Speed,

    //These2 are not actual stats, they're used to boost the moveAccuracy
    Accuracy,
    Evasion
}
public class TypeChart
{
    static float[][] chart = 
    {   //                       NOR   FIR   WAT   ELE  GRA   ICE   POI
        /*NOR*/     new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*FIR*/     new float[] {1f, 0.5f, 0.5f,   2f,   2f,   2f,   1f},
        /*WAT*/     new float[] {1f,   2f, 0.5f,   1f, 0.5f,   1f,   1f},
        /*ELE*/     new float[] {1f,   2f,   2f, 0.5f, 0.5f,   1f,   1f},
        /*GRA*/     new float[] {1f, 0.5f,   2f,   1f, 0.5f, 0.5f, 0.5f},
        /*ICE*/     new float[] {1f, 0.5f,   2f,   1f,   2f, 0,5f,   1f},
        /*POI*/     new float[] {1f,   1f,   1f,   1f,   2f,   1f, 0.5f}
    };

    public static float GetEffectiveness(FakemonType attackType, FakemonType defenseType)
    {
        if (attackType == FakemonType.None || defenseType == FakemonType.None)
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
