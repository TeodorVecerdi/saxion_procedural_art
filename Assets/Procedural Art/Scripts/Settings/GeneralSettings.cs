using UnityEngine;

    [CreateAssetMenu(fileName = "General Settings", menuName = "General Settings", order = 0)]
    public class GeneralSettings : ScriptableObject {
        public long Seed = 0;
        public bool AutoSeed = true;
    }