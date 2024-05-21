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

    [Header("Fly Objects")]
    public List<GameObject> flies;

    [Header("Fly Controls")]
    public float eyeSpiralSpeed = 220.0f;
    [Tooltip("keeps track of data intelligence per fly levels")]
    public List<FlySO> flyIntelLevels = new List<FlySO>();

    [Header("Game Play Flow")]
    public float waveWaitTime = 30f;
    public int waveIndex = 0;
    public int[] durationOfWave = { 60, 120, 200, 300 };
    public int[] fliesInWave = { 5, 10, 20, 40 };


    [Header("HourGlass")]
    public float divFactor = 2.7f;
    public float distanceFromCamera = 1.0f;
  
}
