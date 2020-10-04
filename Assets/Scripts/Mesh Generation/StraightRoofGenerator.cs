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
        width = parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"];
        length = parameters.ContainsKey("length") ? parameters["length"] : defaultParameters["length"];
        height = parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"];
        thickness = parameters.ContainsKey("thickness") ? parameters["thickness"] : defaultParameters["thickness"];
        extrusion = parameters.ContainsKey("extrusion") ? parameters["extrusion"] : defaultParameters["extrusion"];
        extrusionLeft = parameters.ContainsKey("extrusionLeft") ? parameters["extrusionLeft"] : defaultParameters["extrusionLeft"];
        extrusionRight = parameters.ContainsKey("extrusionRight") ? parameters["extrusionRight"] : defaultParameters["extrusionRight"];
        thicknessBasedOnRoofAngle = parameters.ContainsKey("thicknessBasedOnRoofAngle") ? parameters["thicknessBasedOnRoofAngle"] : defaultParameters["thicknessBasedOnRoofAngle"];
        addCap = parameters.ContainsKey("addCap") ? parameters["addCap"] : defaultParameters["addCap"];
        capOffset = parameters.ContainsKey("capOffset") ? parameters["capOffset"] : defaultParameters["capOffset"];
        flip = parameters.ContainsKey("flip") ? parameters["flip"] : defaultParameters["flip"];
        closeRoof = parameters.ContainsKey("closeRoof") ? parameters["closeRoof"] : defaultParameters["closeRoof"];
    }

    /*
    private void OnDrawGizmos() {
        var cap10 = new Vector3(0, RoofThickness, 0);
        var cap11 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofThickness, 0);
        var cap12 = new Vector3(Flip ? -RoofWidth : RoofWidth, 0, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap20 = new Vector3(0, RoofHeight - RoofThickness, RoofLength);
        var cap21 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofHeight - RoofThickness, RoofLength);
        var cap22 = new Vector3(Flip ? -RoofWidth : RoofWidth, RoofHeight, RoofLength);
        var cap23 = new Vector3(0, RoofHeight, RoofLength);
        var cap30 = new Vector3(0 + CapOffset.z, CapOffset.y, -RoofThickness + CapOffset.x);
        var cap31 = new Vector3(Flip ? -RoofWidth : RoofWidth + CapOffset.z, CapOffset.y, -RoofThickness + CapOffset.x);

        if (ThicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap21 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = RoofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = RoofHeight - actualRoofThickness;
            cap21.y = RoofHeight - actualRoofThickness;
            cap30.z = -actualRoofThickness + CapOffset.x;
            cap31.z = -actualRoofThickness + CapOffset.x;
        }

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(cap10, 0.05f);
        Gizmos.DrawWireSphere(cap11, 0.05f);
        Gizmos.DrawWireSphere(cap12, 0.05f);
        Gizmos.DrawWireSphere(cap13, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cap20, 0.05f);
        Gizmos.DrawWireSphere(cap21, 0.05f);
        Gizmos.DrawWireSphere(cap22, 0.05f);
        Gizmos.DrawWireSphere(cap23, 0.05f);
        Gizmos.color = Color.green;
        if (AddCap) {
            Gizmos.DrawWireSphere(cap30, 0.05f);
            Gizmos.DrawWireSphere(cap31, 0.05f);
        }

        var guiStyle = new GUIStyle {fontSize = 16, fontStyle = FontStyle.Bold, normal = {textColor = Color.white}};
        Handles.matrix = transform.localToWorldMatrix;
        Handles.Label(cap10, "1.0", guiStyle);
        Handles.Label(cap11, "1.1", guiStyle);
        Handles.Label(cap12, "1.2", guiStyle);
        Handles.Label(cap13, "1.3", guiStyle);

        Handles.Label(cap20, "2.0", guiStyle);
        Handles.Label(cap21, "2.1", guiStyle);
        Handles.Label(cap22, "2.2", guiStyle);
        Handles.Label(cap23, "2.3", guiStyle);

        if (AddCap) {
            Handles.Label(cap30, "3.0", guiStyle);
            Handles.Label(cap31, "3.1", guiStyle);
        }
    }
    */

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

        AddQuad(cap10, cap11, cap12, cap13, flip);
        AddQuad(cap20, cap21, cap22, cap23, flip);
        AddQuad(cap13, cap12, cap21, cap20, flip);
        AddQuad(cap12, cap11, cap22, cap21, flip);
        AddQuad(cap10, cap13, cap20, cap23, flip);
        AddQuad(cap11, cap10, cap23, cap22, flip);

        if (addCap) {
            AddTriangle(cap12, cap31, cap11, flip);
            AddTriangle(cap13, cap10, cap30, flip);
            AddQuad(cap12, cap13, cap30, cap31, flip);
            AddQuad(cap10, cap11, cap31, cap30, flip);
        }

        if (closeRoof) {
            AddTriangle(cap40, cap41, cap42, flip);
            AddTriangle(cap50, cap51, cap52, !flip);
        }
    }
}