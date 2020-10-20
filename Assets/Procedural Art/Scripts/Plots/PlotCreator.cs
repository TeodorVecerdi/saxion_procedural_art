using System.Collections.Generic;
using UnityEngine;

public class PlotCreator : MonoBehaviour {
    public float NormalAlpha = 0.5f;
    public float SelectedAlpha = 0.75f;
    public float DisabledAlpha = 0.1f;
    public Color NormalColor {
        get {
            var col = SelectedPlotGrid.Color;
            col.a = NormalAlpha;
            return col;
        }
    }
    public Color SelectedColor {
        get {
            var col = NormalColor;
            col.a = SelectedAlpha;
            return col;
        }
    }

    public Color DisabledColor(int gridIndex) {
        var col = PlotGrids[gridIndex].Color;
        col.a = DisabledAlpha;
        return col;
    }

    public Color UnselectColor = Color.red;
    public Color DisabledColorAbs = Color.gray;
    public bool ShowPlots;

    [HideInInspector] public bool IsEnabled;
    [HideInInspector] public int SelectedPlotGridIndex = -1;
    [HideInInspector] public List<PlotGrid> PlotGrids = new List<PlotGrid>();
    public PlotGrid SelectedPlotGrid => SelectedPlotGridIndex == -1 ? null : PlotGrids[SelectedPlotGridIndex];
}