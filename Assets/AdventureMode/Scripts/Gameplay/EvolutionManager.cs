using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image fakemonImage;

    [SerializeField] AudioClip evolutionMusic;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }
    private void Awake()
    {
        i = this;
    }
    public IEnumerator Evolve(Fakemon fakemon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        AudioManager.i.PlayMusic(evolutionMusic);

        fakemonImage.sprite = fakemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{fakemon.Base.Name} is gaining enlightenment!");

        var oldFakemon = fakemon.Base;
        fakemon.Evolve(evolution);

        fakemonImage.sprite = fakemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldFakemon.Name} gained enlightenment and increased its stats!");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
