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
    public float eyeSpiralSpeed = 1.0f;

    [Header("Game Play Flow")]
    public float WaveWaitTime = 30f;
    public int numberOfWaves = 4;
    public float timeBtwStarting = 2.0f;
    public float nextWaveTimeGap = 3.0f;
    public int[] maxWaitTime = { 60, 120, 200, 300 };

    public int[] Waves = { 5, 10, 20, 40 };


    [Header("HourGlass")]
    public float divFactor = 2.7f;
    public float distanceFromCamera = 1.0f;


}
