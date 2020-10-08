using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaneGenerator : MeshGenerator {
    private PlaneOrientation orientation;
    private float sizeA;
    private float sizeB;
    private bool flip;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"orientation", PlaneOrientation.XZ},
            {"sizeA", 1},
            {"sizeB", 1},
            {"flip", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        orientation = parameters.ContainsKey("orientation") ? parameters["orientation"] : defaultParameters["orientation"];
        sizeA = (parameters.ContainsKey("sizeA") ? parameters["sizeA"] : defaultParameters["sizeA"]) * GlobalSettings.Instance.GridSize;
        sizeB = (parameters.ContainsKey("sizeB") ? parameters["sizeB"] : defaultParameters["sizeB"]) * GlobalSettings.Instance.GridSize;
        flip = parameters.ContainsKey("flip") ? parameters["flip"] : defaultParameters["flip"];
    }

    protected override void Generate() {
        var p1 = Vector3.zero;
        var p2 = p1 + Vector3.right * sizeA;
        var p3 = p2 + Vector3.forward * sizeB;
        var p4 = p1 + Vector3.forward * sizeB;
        
        if (orientation == PlaneOrientation.XY) {
            p3 = p2 + Vector3.up * sizeB;
            p4 = p1 + Vector3.up * sizeB;
        } else if (orientation == PlaneOrientation.YZ) {
            p2 = p1 + Vector3.up * sizeA;
            p3 = p2 + Vector3.forward * sizeB;
        }

        AddQuad(p1, p4, p3, p2, 0, flip);
    }

    public enum PlaneOrientation {
        XY,
        XZ,
        YZ
    }
}