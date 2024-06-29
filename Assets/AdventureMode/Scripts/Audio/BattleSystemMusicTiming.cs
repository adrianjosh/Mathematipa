using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystemMusicTiming : MonoBehaviour
{
    [SerializeField] public float loopStartPointTrainer = 0f;
    [SerializeField] public float loopEndPointTrainer = 0f;
    [SerializeField] public float loopStartPointBoss = 0f;
    [SerializeField] public float loopEndPointBoss = 0f;
    [SerializeField] public float loopStartPointWild = 0f;
    [SerializeField] public float loopEndPointWild = 0f;

    public static BattleSystemMusicTiming i;

    private void Awake()
    {
        i = this;
    }


}
