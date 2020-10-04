using System.Collections.Generic;
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

    /*
    private void OnDrawGizmos() {
        var cap10 = new Vector3(0, roofThickness, 0);
        var cap13 = new Vector3(0, 0, 0);
        var cap11 = new Vector3(flipX ? -roofWidth : roofWidth, roofThickness, 0);
        var cap12 = new Vector3(flipX ? -roofWidth : roofWidth, 0, 0);

        var cap20 = new Vector3(0, roofThickness, flipZ ? -roofLength : roofLength);
        var cap23 = new Vector3(0, 0, flipZ ? -roofLength : roofLength);
        var cap21 = new Vector3(0, roofThickness, 0);
        var cap22 = new Vector3(0, 0, 0);

        var cap30 = new Vector3(0 + capOffset.z, capOffset.y, flipZ ? roofThickness : -roofThickness + capOffset.x);
        var cap31 = new Vector3(flipX ? -roofWidth : roofWidth + capOffset.z, capOffset.y, flipZ ? roofThickness : -roofThickness + capOffset.x);

        var cap40 = new Vector3(flipX ? roofThickness : -roofThickness + capOffset.x, capOffset.y, flipZ ? -roofLength : roofLength + capOffset.z);
        var cap41 = new Vector3(flipX ? roofThickness : -roofThickness + capOffset.x, capOffset.y, 0 + capOffset.z);

        var cap50 = new Vector3(flipX ? -roofWidth : roofWidth, roofHeight, flipZ ? -roofLength : roofLength);
        var cap51 = new Vector3(flipX ? -roofWidth : roofWidth, roofHeight - roofThickness, flipZ ? -roofLength : roofLength);

        if (thicknessBasedOnRoofAngle) {
            var v1 = cap11 - cap12;
            var v2 = cap51 - cap12;
            var angle = Mathf.Deg2Rad * Vector3.Angle(v1, v2);
            var multiplier = 1f / Mathf.Sin(angle);
            var actualRoofThickness = roofThickness * multiplier;
            cap10.y = actualRoofThickness;
            cap11.y = actualRoofThickness;
            cap20.y = actualRoofThickness;
            cap21.y = actualRoofThickness;
            cap30.z = flipZ ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap31.z = flipZ ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap40.x = flipX ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap41.x = flipX ? actualRoofThickness : -actualRoofThickness + capOffset.x;
            cap51.y = roofHeight - actualRoofThickness;
        }

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(cap10, 0.03f);
        Gizmos.DrawWireSphere(cap11, 0.03f);
        Gizmos.DrawWireSphere(cap12, 0.03f);
        Gizmos.DrawWireSphere(cap13, 0.03f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cap20, 0.03f);
        Gizmos.DrawWireSphere(cap21, 0.03f);
        Gizmos.DrawWireSphere(cap22, 0.03f);
        Gizmos.DrawWireSphere(cap23, 0.03f);
        if (addCap) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cap30, 0.03f);
            Gizmos.DrawWireSphere(cap31, 0.03f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(cap40, 0.03f);
            Gizmos.DrawWireSphere(cap41, 0.03f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(cap50, 0.03f);
        Gizmos.DrawWireSphere(cap51, 0.03f);

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

        if (addCap) {
            Handles.Label(cap30, "3.0", guiStyle);
            Handles.Label(cap31, "3.1", guiStyle);
            Handles.Label(cap40, "4.0", guiStyle);
            Handles.Label(cap41, "4.1", guiStyle);
        }

        Handles.Label(cap50, "5.0", guiStyle);
        Handles.Label(cap51, "5.1", guiStyle);
    }
    */

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
        width = parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"];
        length = parameters.ContainsKey("length") ? parameters["length"] : defaultParameters["length"];
        height = parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"];
        thickness = parameters.ContainsKey("thickness") ? parameters["thickness"] : defaultParameters["thickness"];
        thicknessBasedOnRoofAngle = parameters.ContainsKey("thicknessBasedOnRoofAngle") ? parameters["thicknessBasedOnRoofAngle"] : defaultParameters["thicknessBasedOnRoofAngle"];
        addCap = parameters.ContainsKey("addCap") ? parameters["addCap"] : defaultParameters["addCap"];
        capOffset = parameters.ContainsKey("capOffset") ? parameters["capOffset"] : defaultParameters["capOffset"];
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

        AddQuad(cap10, cap11, cap12, cap13, flipX ^ flipZ);
        AddQuad(cap20, cap21, cap22, cap23, flipX ^ flipZ);
        AddTriangle(cap10, cap50, cap11, flipX ^ flipZ);
        AddTriangle(cap20, cap50, cap21, flipX ^ flipZ);
        AddQuad(cap11, cap50, cap51, cap12, flipX ^ flipZ);
        AddQuad(cap50, cap20, cap23, cap51, flipX ^ flipZ);
        AddTriangle(cap22, cap12, cap51, flipX ^ flipZ);
        AddTriangle(cap22, cap51, cap23, flipX ^ flipZ);

        if (addCap) {
            AddTriangle(cap12, cap31, cap11, flipX ^ flipZ);
            AddQuad(cap12, cap13, cap30, cap31, flipX ^ flipZ);
            AddQuad(cap10, cap11, cap31, cap30, flipX ^ flipZ);
            AddTriangle(cap23, cap20, cap40, flipX ^ flipZ);
            AddQuad(cap22, cap23, cap40, cap41, flipX ^ flipZ);
            AddQuad(cap20, cap21, cap41, cap40, flipX ^ flipZ);
            AddTriangle(cap21, cap30, cap41, flipX ^ flipZ);
            AddTriangle(cap41, cap30, cap22, flipX ^ flipZ);
        }
    }
}