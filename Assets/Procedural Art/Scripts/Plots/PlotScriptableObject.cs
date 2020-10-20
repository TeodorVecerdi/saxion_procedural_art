using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlotData {
    public Rect Bounds;
    public float Rotation;
}
[Serializable]
public struct PlotGridData {
    public List<PlotData> Plots;
    public Color Color;
    public string Name;
}
public class PlotScriptableObject : ScriptableObject {
    public List<PlotGridData> PlotGrids;
}