using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlotCreator : MonoBehaviour {
    [FormerlySerializedAs("DragColor")]
    public Color NormalColor = Color.green;
    public Color UnselectColor = Color.red;
    public Color SelectedColor = Color.blue;
    public bool ShowPlots;
    public bool UseSubgrid;
    [HideInInspector] public bool IsEnabled;
    [HideInInspector] public List<Plot> Plots = new List<Plot>();
}