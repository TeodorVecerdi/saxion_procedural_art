using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaneGenerator : MeshGenerator {
    private PlaneOrientation orientation;
    private UVSettings extraUvSettings;
    private float sizeA;
    private float sizeB;
    private int submeshIndex;
    private bool flip;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"orientation", PlaneOrientation.XZ},
            {"extraUvSettings", UVSettings.None},
            {"sizeA", 1},
            {"sizeB", 1},
            {"submeshIndex", 0},
            {"flip", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        orientation = parameters.ContainsKey("orientation") ? parameters["orientation"] : defaultParameters["orientation"];
        extraUvSettings = parameters.ContainsKey("extraUvSettings") ? parameters["extraUvSettings"] : defaultParameters["extraUvSettings"];
        sizeA = (parameters.ContainsKey("sizeA") ? parameters["sizeA"] : defaultParameters["sizeA"]) * GlobalSettings.Instance.GridSize;
        sizeB = (parameters.ContainsKey("sizeB") ? parameters["sizeB"] : defaultParameters["sizeB"]) * GlobalSettings.Instance.GridSize;
        submeshIndex = parameters.ContainsKey("submeshIndex") ? parameters["submeshIndex"] : defaultParameters["submeshIndex"];
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

        AddQuad(p1, p4, p3, p2, submeshIndex, flip, extraUvSettings);
    }

    public enum PlaneOrientation {
        XY,
        XZ,
        YZ
    }
}