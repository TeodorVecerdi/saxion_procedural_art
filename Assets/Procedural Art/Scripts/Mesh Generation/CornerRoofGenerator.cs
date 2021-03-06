﻿using System.Collections.Generic;
using UnityEngine;

public class CornerRoofGenerator : MeshGenerator {
    private float width;
    private float length;
    private float height;
    private float thickness;
    private bool thicknessBasedOnRoofAngle;
    private bool addCap;
    private Vector3 capOffset;
    private bool flipX;
    private bool flipZ;
    private bool joinCaps;


    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"width", 1f},
            {"length", 1f},
            {"height", 1f},
            {"thickness", 0.1f},
            {"thicknessBasedOnRoofAngle", false},
            {"addCap", false},
            {"capOffset", Vector3.zero},
            {"flipX", false},
            {"flipZ", false},
            {"joinCaps", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        width = (parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"]) * GlobalSettings.Instance.GridSize;
        length = (parameters.ContainsKey("length") ? parameters["length"] : defaultParameters["length"]) * GlobalSettings.Instance.GridSize;
        height = (parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"]) * GlobalSettings.Instance.GridSize;
        thickness = (parameters.ContainsKey("thickness") ? parameters["thickness"] : defaultParameters["thickness"]) * GlobalSettings.Instance.GridSize;
        thicknessBasedOnRoofAngle = parameters.ContainsKey("thicknessBasedOnRoofAngle") ? parameters["thicknessBasedOnRoofAngle"] : defaultParameters["thicknessBasedOnRoofAngle"];
        addCap = parameters.ContainsKey("addCap") ? parameters["addCap"] : defaultParameters["addCap"];
        capOffset = (parameters.ContainsKey("capOffset") ? parameters["capOffset"] : defaultParameters["capOffset"]) * GlobalSettings.Instance.GridSize;
        flipX = parameters.ContainsKey("flipX") ? parameters["flipX"] : defaultParameters["flipX"];
        flipZ = parameters.ContainsKey("flipZ") ? parameters["flipZ"] : defaultParameters["flipZ"];
        joinCaps = parameters.ContainsKey("joinCaps") ? parameters["joinCaps"] : defaultParameters["joinCaps"];
    }

    protected override void Generate() {
        var cap10 = new Vector3(0, thickness, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap11 = new Vector3(flipX ? -width : width, thickness, 0);
        var cap12 = new Vector3(flipX ? -width : width, 0, 0);

        var cap20 = new Vector3(0, thickness, flipZ ? -length : length);
        var cap23 = new Vector3(0, 0, flipZ ? -length : length);
        var cap21 = new Vector3(0, thickness, 0);
        var cap22 = new Vector3(0, 0, 0);
        var capMovement30 = joinCaps ? flipX ? thickness : -thickness : 0;
        var capMovement41 = joinCaps ? flipZ ? thickness : -thickness : 0;
        var cap30 = new Vector3(capOffset.z + capMovement30, capOffset.y, flipZ ? thickness : -thickness + capOffset.x);
        var cap31 = new Vector3(flipX ? -width : width + capOffset.z, capOffset.y, flipZ ? thickness : -thickness + capOffset.x);

        var cap40 = new Vector3(flipX ? thickness : -thickness + capOffset.x, capOffset.y, flipZ ? -length : length + capOffset.z);
        var cap41 = new Vector3(flipX ? thickness : -thickness + capOffset.x, capOffset.y, capOffset.z + capMovement41);

        var cap50 = new Vector3(flipX ? -width : width, height, flipZ ? -length : length);
        var cap51 = new Vector3(flipX ? -width : width, height - thickness, flipZ ? -length : length);

        if (thicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap51 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = thickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = actualRoofThickness;
            cap21.y = actualRoofThickness;
            cap30.z = flipZ ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap31.z = flipZ ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap40.x = flipX ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap41.x = flipX ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap51.y = height - actualRoofThickness;
        }

        AddQuad(cap10, cap11, cap12, cap13, 1, flipX ^ flipZ);
        AddQuad(cap20, cap21, cap22, cap23, 1, flipX ^ flipZ);
        AddTriangle(cap11, cap50, cap21, 1, !(flipX ^ flipZ), UVSettings.FlipBottomPart);
        AddTriangle(cap20, cap50, cap21, 1, flipX ^ flipZ);
        
        AddQuad(cap11, cap50, cap51, cap12, 1, flipX ^ flipZ);
        AddQuad(cap50, cap20, cap23, cap51, 1, flipX ^ flipZ);
        AddTriangle(cap22, cap12, cap51, 1, flipX ^ flipZ);
        AddTriangle(cap22, cap51, cap23, 1, flipX ^ flipZ);

        if (addCap) {
            AddTriangle(cap12, cap31, cap11, 1, flipX ^ flipZ);
            AddQuad(cap12, cap13, cap30, cap31, 1, flipX ^ flipZ);
            AddQuad(cap10, cap11, cap31, cap30, 1, flipX ^ flipZ);
            AddTriangle(cap23, cap20, cap40, 1, flipX ^ flipZ);
            AddQuad(cap22, cap23, cap40, cap41, 1, flipX ^ flipZ);
            AddQuad(cap20, cap21, cap41, cap40, 1, flipX ^ flipZ);
            AddTriangle(cap21, cap30, cap41, 1, flipX ^ flipZ);
            AddTriangle(cap41, cap30, cap22, 1, flipX ^ flipZ);
        }
    }
}