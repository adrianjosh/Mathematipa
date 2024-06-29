using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FakemonParty : MonoBehaviour
{
    [SerializeField] List<Fakemon> fakemons;

    public event Action OnUpdated;
    public List<Fakemon> Fakemons
    {
        get
        {
            return fakemons;
        }
        set
        {
            fakemons = value;
            OnUpdated?.Invoke();
        }
    }
    private void Awake()
    {
        foreach (var fakemon in fakemons)
        {
            fakemon.Init();
        }
    }
    private void Start()
    {
        
    }

    public Fakemon GetHealthyFakemon()
    {
        return fakemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddFakemon(Fakemon newFakemon)
    {
        if (fakemons.Count < 6)
        {
            
            fakemons.Add(newFakemon);
            OnUpdated?.Invoke();
        }
        else
        {
            
            // TODO: Add to the PC once that's implemented
        }
    }

    public bool CheckForEvolutions()
    {
        return fakemons.Any(p => p.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolutions()
    {
        foreach (var fakemon in fakemons)
        {
            var evolution = fakemon.CheckForEvolution();
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(fakemon, evolution); ;
            }
        }
    }

    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }

    public static FakemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<FakemonParty>();
    }
}
