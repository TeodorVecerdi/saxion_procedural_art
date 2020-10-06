using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Generator Settings", order = 0)]
public class RandomSettings : ScriptableObject {
    public GeneralSettingsData GeneralSettings;
    public SquareBuildingSettingsData SquareBuildingSettings;
    public LBuildingSettingsData LBuildingSettings;
    public TBuildingSettingsData TBuildingSettings;

    public RandomSettings Clone() {
        var copy = CreateInstance<RandomSettings>();
        copy.GeneralSettings = GeneralSettings.Clone();
        copy.SquareBuildingSettings = SquareBuildingSettings.Clone();
        copy.LBuildingSettings = LBuildingSettings.Clone();
        copy.TBuildingSettings = TBuildingSettings.Clone();
        return copy;
    }

    [Serializable]
    public class GeneralSettingsData {
        public long Seed = 0;
        public bool AutoSeed = true;
        [Space]
        public float SquareChance = 0.3333333F;
        public float LChance = 0.3333333F;
        public float TChance = 0.3333334F;

        public GeneralSettingsData Clone() {
            var gs = new GeneralSettingsData {
                Seed = Seed,
                AutoSeed = AutoSeed,
                SquareChance = SquareChance,
                LChance = LChance,
                TChance = TChance
            };
            return gs;
        }
    }

    [Serializable]
    public class SquareBuildingSettingsData {
        public Vector3Int MinSize;
        public Vector3Int MaxSize;
        [Space]
        public float OverhangChance;

        public SquareBuildingSettingsData Clone() {
            var sbs = new SquareBuildingSettingsData {
                MinSize = MinSize,
                MaxSize = MaxSize,
                OverhangChance = OverhangChance
            };
            return sbs;
        }
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
        public Vector2Int OverhangMinMaxStart;
        public Vector2Int OverhangMinMaxHeight;
        public float OverhangChance;

        public LBuildingSettingsData Clone() {
            var lbs = new LBuildingSettingsData {
                MinMaxHeight = MinMaxHeight,
                MinMaxWidthA = MinMaxWidthA,
                MinMaxLengthA = MinMaxLengthA,
                MinMaxWidthB = MinMaxWidthB,
                MinMaxLengthB = MinMaxLengthB,
                OverhangMinMaxStart = OverhangMinMaxStart,
                OverhangMinMaxHeight = OverhangMinMaxHeight,
                OverhangChance = OverhangChance
            };
            return lbs;
        }
    }

    [Serializable]
    public class TBuildingSettingsData {
        public Vector2Int MinMaxHeight;
        [Space]
        public Vector2Int MinMaxWidth;
        public Vector2Int MinMaxLength;
        public Vector2Int MinMaxExtrusion;
        public Vector2Int MinMaxInset;
        [Space]
        public Vector2Int OverhangMinMax;
        public float OverhangChance;

        public TBuildingSettingsData Clone() {
            var tbs = new TBuildingSettingsData {
                MinMaxHeight = MinMaxHeight,
                MinMaxWidth = MinMaxWidth,
                MinMaxLength = MinMaxLength,
                MinMaxExtrusion = MinMaxExtrusion,
                MinMaxInset = MinMaxInset,
                OverhangMinMax = OverhangMinMax,
                OverhangChance = OverhangChance
            };
            return tbs;
        }
    }
}