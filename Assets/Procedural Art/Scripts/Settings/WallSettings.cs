using UnityEngine;

    [CreateAssetMenu(fileName = "Wall Settings", menuName = "Wall Settings", order = 0)]
    public class WallSettings : ScriptableObject {
        public int Height;
        public Vector2 HeightVariation;
        public Vector2 RoofHeightVariation;
        
        public float RoofThickness = 0.15f;
        public float RoofExtrusion = 0.25f;
    }