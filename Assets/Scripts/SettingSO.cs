using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingSO")]
public class SettingSO : ScriptableObject
{
    [Header("Scoring")]
    public int score = 0;
    public int scoreMulFactor = 175;
    public int numberOfKills = 0;
    public int skillScoreValue = 100;
    public int Cash = 0;

    [Header("Fly Objects")]
    public List<GameObject> flies = new List<GameObject>();

    [Header("Fly Controls")]
    public float eyeSpiralSpeed = 220.0f;
    [Tooltip("keeps track of data intelligence per fly levels")]
    public List<FlySO> flyIntelLevels = new List<FlySO>();

    [Header("Game Play Flow")]
    public float waveWaitTime = 30f;
    public int waveIndex = 0;
    public int[] durationOfWave = { 60, 120, 200, 300 };
    public int[] fliesInWave = { 5, 10, 20, 40 };
    public int[] tntFliesInWave = { 1, 2, 3, 4 };
    public int[] LevelGoals = { 2, 2, 2, 2 };
    public float[] TakeoffChances = { 0.1f, 0.2f, 0.3f, 0.4f };

    [Header("HourGlass")]
    public float divFactor = 2.7f;
    public float distanceFromCamera = 1.0f;

}
