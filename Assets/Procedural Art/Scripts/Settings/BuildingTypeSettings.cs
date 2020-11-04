using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "Building Settings", menuName = "Building Settings", order = 0)]
    public class BuildingTypeSettings : ScriptableObject {
        public MaterialSettings MaterialSetting;
        public ScriptableObject GeneratorSettings;
        public BuildingGenerator GeneratorPrefab;
    }