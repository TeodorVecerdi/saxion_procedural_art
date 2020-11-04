using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "Building Versions", menuName = "Building Versions", order = 0)]
public class BuildingVersions : ScriptableObject {
    [Header("Richness (0 - very poor, 3 - rich)")]
    public BuildingTypeSettings[] Versions = new BuildingTypeSettings[4];
    public string PlotLayerName;

    private void OnValidate() {
        if (Versions.Length != 4) {
            Array.Resize(ref Versions, 4);
        }
    }

    public BuildingTypeSettings Select(int richness) {
        for (int i = richness; i > 0; i--) {
            if (Versions[i] != null) return Versions[i];
        }

        return Versions[0];
    }
}