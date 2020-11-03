using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "Building Settings", menuName = "Building Settings", order = 0)]
    public class BuildingTypeSettings : ScriptableObject {
        public List<MaterialSettings> RichnessBasedMaterials;
        public ScriptableObject GeneratorSettings;
        public BuildingGenerator GeneratorPrefab;
        public string PlotLayerName;
    }