using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<FakemonEncounterRecord> wildFakemons;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    private void OnValidate()
    {
        CalculateChancePercentage();
    }

    private void Start()
    {
        CalculateChancePercentage();
    }

    void CalculateChancePercentage()
    {
        totalChance = 0;
        foreach (var record in wildFakemons)
        {
            record.chanceLower = totalChance;
            record.chanceUpper = totalChance + record.chancePercentage;

            totalChance = totalChance + record.chancePercentage;
        }
    }
    public Fakemon GetRandomWildFakemon()
    {
        int randVal = Random.Range(1, 101);
        var fakemonRecord = wildFakemons.First(p => randVal >= p.chanceLower && randVal <= p.chanceUpper);

        var levelRange = fakemonRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);

        var wildFakemon = new Fakemon(fakemonRecord.fakemon, level);

        wildFakemon.Init();
        return wildFakemon;
    }
}

[System.Serializable]
public class FakemonEncounterRecord
{
    public FakemonBase fakemon;
    public Vector2Int levelRange;
    public int chancePercentage;

    public int chanceLower { get; set; }

    public int chanceUpper { get; set; }
}
