using System;
using UnityEngine;

[Serializable]
public class Plot {
    public Rect Bounds;
    private Plot() {}
    public static Plot FromCenterSize(Vector2 center, Vector2 size) {
        var plot = new Plot {Bounds = {center = center, size = size}};
        return plot;
    }
    
    public static Plot FromStartEnd(Vector2 start, Vector2 end) {
        var plot = new Plot {Bounds = new Rect()};
        plot.Bounds.min = start;
        plot.Bounds.max = end;
        return plot;
    }
}