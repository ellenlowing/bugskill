using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingSO")]
public class SettingSO : ScriptableObject
{
    [Header("Scoring")]
    public int totalKills = 0;
    public int localKills = 0;
    public int Cash = 0;

    [Header("Fly Objects")]
    public List<GameObject> flies = new List<GameObject>();

    [Header("Fly Controls")]
    public float eyeSpiralSpeed = 220.0f;
    [Tooltip("keeps track of data intelligence per fly levels")]
    public List<FlySO> flyIntelLevels = new List<FlySO>();

    [Header("Game Play Flow")]
    public int waveIndex = 0;
    public int[] durationOfWave = { 60, 120, 200, 300 };
    public int[] fliesInWave = { 5, 10, 20, 40 };
    public int[] LevelGoals = { 2, 2, 2, 2 };
    public float[] TakeoffChances = { 0.1f, 0.2f, 0.3f, 0.4f };
    public float[] minNearbyFlyDistances = { 1.5f, 1.5f, 1.5f, 1.5f };

    [Header("Presets")]
    public float divFactor = 2.7f;
    public float nearDistanceFromCamera = 1.0f;
    public float farDistanceFromCamera = 1.0f;
    public float SplatDistanceOffset = 0.05f;
    public float EnvironmentDepthBias = 0f;

}
