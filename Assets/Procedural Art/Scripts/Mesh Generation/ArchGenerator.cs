using System.Collections.Generic;
using UnityEngine;

public class ArchGenerator : MeshGenerator {
    private float width;
    private float height;
    private float length;
    private int points;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"width", 1},
            {"height", 1},
            {"length", 1},
            {"points", 90}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        width = (parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"]) * GlobalSettings.Instance.GridSize;
        height = (parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"]) * GlobalSettings.Instance.GridSize;
        length = (parameters.ContainsKey("length") ? parameters["length"] : defaultParameters["length"]) * GlobalSettings.Instance.GridSize;
        points = parameters.ContainsKey("points") ? parameters["points"] : defaultParameters["points"];
    }

    protected override void Generate() {
        var b0 = new Vector3(0, 0, 0);
        var b1 = new Vector3(width, 0, 0);
        var b2 = new Vector3(width, 0, length);
        var b3 = new Vector3(0, 0, length);
        var c0 = new Vector3(0, height, 0);
        var c1 = new Vector3(width, height, 0);
        var c2 = new Vector3(width, height, length);
        var c3 = new Vector3(0, height, length);

        var step = 180f / (points - 1) * Mathf.Deg2Rad;
        for (var val = 0f; val <= Mathf.PI - (step / 4f); val += step) {
            var x = ((Mathf.Cos(val) + 1) / 2f) * width;
            var y = Mathf.Sin(val) * height;
            var x2 = ((Mathf.Cos(val + step) + 1) / 2f) * width;
            var y2 = Mathf.Sin(val + step) * height;

            var p0 = new Vector3(x, y, 0);
            var p1 = new Vector3(x2, y2, 0);
            var p2 = new Vector3(x2, y2, length);
            var p3 = new Vector3(x, y, length);
            AddQuad(p3, p2, p1, p0, 0);
            
            if (points % 2 == 0 && Mathf.Abs(y - y2) < 0.00001f) {
                var t0 = new Vector3(x, height, 0);
                var t1 = new Vector3(x2, height, 0);
                var t2 = new Vector3(x2, height, length);
                var t3 = new Vector3(x, height, length);
                AddQuad(p1, t1, t0, p0, 0);
                AddQuad(p3, t3, t2, p2, 0);
                AddTriangle(p1, c0, t1, 0);
                AddTriangle(p0, t0, c1, 0);
                AddTriangle(p3, c2, t3, 0);
                AddTriangle(p2, t2, c3, 0);
            } else {
                if ((x2 + x) / 2f >= width / 2f) {
                    AddTriangle(p0, p1, c1, 0);
                    AddTriangle(c2, p2, p3, 0);
                } else {
                    AddTriangle(p0, p1, c0, 0);
                    AddTriangle(p3, c3,p2, 0);
                }
            }
        }

        AddQuad(c0, c3, c2, c1, 0);
        AddQuad(b0, b3, c3, c0, 0);
        AddQuad(b1, c1, c2, b2, 0);
    }
}