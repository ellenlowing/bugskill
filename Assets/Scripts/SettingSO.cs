using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SettingSO")]
public class SettingSO : ScriptableObject
{
    [Header("Fly Objects")]
    public List<GameObject> flies;

    [Header("Scoring")]
    public int score = 0;
    public int scoreMulFactor = 5;
    public int numberOfKills = 0;
    public int skillScoreValue = 100;

    [Header("Fly Controls")]
    public float flySpawnIntervalMin = 5.0f;
    public float flySpawnIntervalMax = 15.0f;
    public float flySpawnIntervalFactor = 0.5f;
    public float flySlowDownTime = 8.0f;
    public float flySlowDownSpeed = 0.5f;

    [Header("Game Play Flow")]
    public int flyDuplicateNumber = 3;
    public int numberOfWaves = 10;
    public float timeBtwStarting = 2.0f;
    public float nextWaveTimeGap = 3.0f;
    public float waveTimeToComplete = 10.0f;


}
