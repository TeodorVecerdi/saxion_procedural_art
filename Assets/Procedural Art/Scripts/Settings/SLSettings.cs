using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "SL Settings", order = 0)]
public class SLSettings : ScriptableObject {
    public GeneralBuildingSettingsData GeneralSettings;
    [FormerlySerializedAs("SquareBuildingSettings")]
    public SquareBuildingSettingsData SquareSettings;
    [FormerlySerializedAs("LBuildingSettings")]
    public LBuildingSettingsData LSettings;

    [Serializable]
    public class GeneralBuildingSettingsData {
        public float RoofThickness = 0.15f;
        public float RoofExtrusion = 0.25f;
        public float ChimneyChance = 0.9f;
        public float ChimneySmokeChance = 0.3f;
        public Vector2 ChimneyThicknessMinMax = 0.25f*(Vector2.up + Vector2.one); 
        [Header("Very Poor wall features")]
        public float FillWallSegmentChance = 0.1f;
        public Vector2 FillWallSegmentSpacing = 0.01f*(Vector2.one + Vector2.up);
       
        [Header("<= Poor wall features")]
        public float FeatureThickness = 0.1f;
        public float DiagonalAChance = 0.5f;
        public float DiagonalBChance = 0.5f;
        public float WindowChance = 0.2f;
        public Vector2 HorizontalSplitMinMax = 2*Vector2.one + Vector2.up;
        public Vector2 VerticalSplitMinMax = 2*Vector2.one + 2*Vector2.up;

        [Header(">= Normal wall features")]
        public float PillarThickness = 0.25f;
    }

    [Serializable]
    public class SquareBuildingSettingsData {
        public Vector2Int MinMaxHeight;
        public Vector2 MinMaxRoofHeight;
        [Space]
        public float StraightRoofChance = 0.33334F;
        public float DoubleCornerRoofChance = 0.33333F;
        public float CornerRoofChance =   0.33333F;
        [Space]
        public Vector2 RoofWidthToBuildingWidthRatio = Vector2.up;
    }

    [Serializable]
    public class LBuildingSettingsData {
        public Vector2Int MinMaxHeight;
        public Vector2 MinMaxRoofHeight;
        [Space]
        public float CornerEndingChance = 0.3333F;
        public Vector2 CornerInsetRatio = Vector2.one - 0.5f * Vector2.right;
        [Space]
        public Vector2Int MinMaxWidthA;
        public Vector2Int MinMaxLengthA;
        public Vector2Int MinMaxWidthB;
        public Vector2Int MinMaxLengthB;
        [Space]
        public float OverhangChance;
    }
}