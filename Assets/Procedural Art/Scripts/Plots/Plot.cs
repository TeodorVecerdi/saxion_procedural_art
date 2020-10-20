using System;
using UnityEngine;

[Serializable]
public class Plot {
    public Rect Bounds;
    public float Rotation;
    private Plot() {}
    
    public static Plot FromStartEnd(Vector2 start, Vector2 end, float rotation = 0) {
        var plot = new Plot {Bounds = new Rect()};
        plot.Bounds.min = start;
        plot.Bounds.max = end;
        plot.Rotation = rotation;
        return plot;
    }
}