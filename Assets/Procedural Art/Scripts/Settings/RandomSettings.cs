using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Generator Settings", order = 0)]
public class RandomSettings : ScriptableObject {
    public GeneralSettingsData GeneralSettings;
    public SquareBuildingSettingsData SquareBuildingSettings;
    public LBuildingSettingsData LBuildingSettings;


    [Serializable]
    public class GeneralSettingsData {
        public float SquareChance = 0.3333333F;
        public float LChance = 0.3333333F;
    }

    [Serializable]
    public class SquareBuildingSettingsData {
        public Vector3Int MinSize;
        public Vector3Int MaxSize;
        [Space]
        public float OverhangChance;
    }

    [Serializable]
    public class LBuildingSettingsData {
        public Vector2Int MinMaxHeight;
        [Space]
        public Vector2Int MinMaxWidthA;
        public Vector2Int MinMaxLengthA;
        public Vector2Int MinMaxWidthB;
        public Vector2Int MinMaxLengthB;
        [Space]
        public float OverhangChance;
    }
}