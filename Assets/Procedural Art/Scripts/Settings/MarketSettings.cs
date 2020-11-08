using UnityEngine;

    [CreateAssetMenu(fileName = "Market Settings", menuName = "Market Settings", order = 0)]
    public class MarketSettings : ScriptableObject {
        public float MinDistanceFromCenter = 2f;
        public Vector2Int MinMaxStalls = (Vector2Int.one + Vector2Int.up) * 4;
        public Vector2Int MinMaxDetails = (Vector2Int.one + Vector2Int.up) * 4;
        public Vector2Int MinMaxTreeClumps = (Vector2Int.one + Vector2Int.up) * 4;
        public Vector2Int MinMaxTreesPerClump = Vector2Int.one * 2 + Vector2Int.up * 2;
        public float MaxTreeDistance = 1f;
        public float MinTreeDistance = 0.25f;
        public float BorderDistance = 3f;
    }