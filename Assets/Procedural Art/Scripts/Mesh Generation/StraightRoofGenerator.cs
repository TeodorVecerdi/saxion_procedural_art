using System.Collections.Generic;
using UnityEngine;

public class StraightRoofGenerator : MeshGenerator {
    private float width;
    private float length;
    private float height;
    private float thickness;
    private float extrusion;
    private bool extrusionLeft;
    private bool extrusionRight;
    private bool thicknessBasedOnRoofAngle;
    private bool closeRoof;
    
    private bool addCap;
    private Vector3 capOffset;
    
    private bool flip;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"width", 1f},
            {"length", 1f},
            {"height", 1f},
            {"thickness", 0.1f},
            {"extrusion", 0.25f},
            {"extrusionLeft", true},
            {"extrusionRight", true},
            {"thicknessBasedOnRoofAngle", false},
            {"addCap", false},
            {"capOffset", Vector3.zero},
            {"flip", false},
            {"closeRoof", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        width = (parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"]) * GlobalSettings.Instance.GridSize;
        length = (parameters.ContainsKey("length") ? parameters["length"] : defaultParameters["length"]) * GlobalSettings.Instance.GridSize;
        height = (parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"]) * GlobalSettings.Instance.GridSize;
        thickness = (parameters.ContainsKey("thickness") ? parameters["thickness"] : defaultParameters["thickness"]) * GlobalSettings.Instance.GridSize;
        extrusion = (parameters.ContainsKey("extrusion") ? parameters["extrusion"] : defaultParameters["extrusion"]) * GlobalSettings.Instance.GridSize;
        extrusionLeft = parameters.ContainsKey("extrusionLeft") ? parameters["extrusionLeft"] : defaultParameters["extrusionLeft"];
        extrusionRight = parameters.ContainsKey("extrusionRight") ? parameters["extrusionRight"] : defaultParameters["extrusionRight"];
        thicknessBasedOnRoofAngle = parameters.ContainsKey("thicknessBasedOnRoofAngle") ? parameters["thicknessBasedOnRoofAngle"] : defaultParameters["thicknessBasedOnRoofAngle"];
        addCap = parameters.ContainsKey("addCap") ? parameters["addCap"] : defaultParameters["addCap"];
        capOffset = (parameters.ContainsKey("capOffset") ? parameters["capOffset"] : defaultParameters["capOffset"]) * GlobalSettings.Instance.GridSize;
        flip = parameters.ContainsKey("flip") ? parameters["flip"] : defaultParameters["flip"];
        closeRoof = parameters.ContainsKey("closeRoof") ? parameters["closeRoof"] : defaultParameters["closeRoof"];
    }

    protected override void Generate() {
        var cap10 = new Vector3(extrusionLeft ? (flip ? extrusion : -extrusion) : 0, thickness, 0);
        var cap11 = new Vector3(flip ? -width - (extrusionRight ? extrusion : 0) : width + (extrusionRight ? extrusion : 0), thickness, 0);
        var cap12 = new Vector3(flip ? -width - (extrusionRight ? extrusion : 0) : width + (extrusionRight ? extrusion : 0), 0, 0);
        var cap13 = new Vector3(extrusionLeft ? (flip ? extrusion : -extrusion) : 0, 0, 0);
        var cap20 = new Vector3(extrusionLeft ? (flip ? extrusion : -extrusion) : 0, height - thickness, length);
        var cap21 = new Vector3(flip ? -width - (extrusionRight ? extrusion : 0) : width + (extrusionRight ? extrusion : 0), height - thickness, length);
        var cap22 = new Vector3(flip ? -width - (extrusionRight ? extrusion : 0) : width + (extrusionRight ? extrusion : 0), height, length);
        var cap23 = new Vector3(extrusionLeft ? (flip ? extrusion : -extrusion) : 0, height, length);
        var cap30 = new Vector3(extrusionLeft ? (flip ? extrusion : -extrusion) : 0 + capOffset.z, capOffset.y, -thickness + capOffset.x);
        var cap31 = new Vector3((flip ? -width - (extrusionRight ? extrusion : 0) : width + (extrusionRight ? extrusion : 0)) + capOffset.z, capOffset.y, -thickness + capOffset.x);

        var cap40 = new Vector3(flip ? -width : width, 0, 0);
        var cap41 = new Vector3(flip ? -width : width, height - thickness, length);
        var cap42 = new Vector3(flip ? -width : width, 0, length);
        
        var cap50 = new Vector3(0, 0, 0);
        var cap51 = new Vector3(0, height - thickness, length);
        var cap52 = new Vector3(0, 0, length);
        

        if (thicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap21 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = thickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = height - actualRoofThickness;
            cap21.y = height - actualRoofThickness;
            cap30.z = -actualRoofThickness + capOffset.x;
            cap31.z = -actualRoofThickness + capOffset.x;
            cap41.y = height - actualRoofThickness;
            cap51.y = height - actualRoofThickness;
        }

        AddQuad(cap10, cap11, cap12, cap13, 1, flip);
        AddQuad(cap20, cap21, cap22, cap23, 1, flip);
        AddQuad(cap13, cap12, cap21, cap20, 1, flip);
        AddQuad(cap12, cap11, cap22, cap21, 1, flip);
        AddQuad(cap10, cap13, cap20, cap23, 1, flip);
        AddQuad(cap11, cap10, cap23, cap22, 1, flip);

        if (addCap) {
            AddTriangle(cap12, cap31, cap11, 1, flip);
            AddTriangle(cap13, cap10, cap30, 1, flip);
            AddQuad(cap12, cap13, cap30, cap31, 1, flip);
            AddQuad(cap10, cap11, cap31, cap30, 1, flip);
        }

        if (closeRoof) {
            AddTriangle(cap40, cap41, cap42, 0, flip);
            AddTriangle(cap50, cap51, cap52, 0, !flip);
        }
    }
}