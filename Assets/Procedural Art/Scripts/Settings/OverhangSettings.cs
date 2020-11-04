using UnityEngine;

    [CreateAssetMenu(fileName = "Overhang Settings", menuName = "Overhang Settings", order = 0)]
    public class OverhangSettings : WallSettings {
        public float GroundOffset;
        public Vector2 GroundOffsetVariation;
    }