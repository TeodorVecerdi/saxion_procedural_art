using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MeshGenerator {
    private Vector3 start;
    private Vector3 end;
    private float thickness;
    private float extrusion;
    private int submeshIndex;
    private bool extrusionOutwards;
    private bool extrusionCenter;
    private bool rotateUV;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"start", Vector3.zero},
            {"end", Vector3.up},
            {"thickness", 0.1f},
            {"extrusion", 0.1f},
            {"submeshIndex", 0},
            {"extrusionOutwards", false},
            {"extrusionCenter", false},
            {"rotateUV", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        start = parameters.ContainsKey("start") ? parameters["start"] : defaultParameters["start"];
        end = parameters.ContainsKey("end") ? parameters["end"] : defaultParameters["end"];
        thickness = parameters.ContainsKey("thickness") ? parameters["thickness"] : defaultParameters["thickness"];
        extrusion = parameters.ContainsKey("extrusion") ? parameters["extrusion"] : defaultParameters["extrusion"];
        submeshIndex = parameters.ContainsKey("submeshIndex") ? parameters["submeshIndex"] : defaultParameters["submeshIndex"];
        extrusionOutwards = parameters.ContainsKey("extrusionOutwards") ? parameters["extrusionOutwards"] : defaultParameters["extrusionOutwards"];
        extrusionCenter = parameters.ContainsKey("extrusionCenter") ? parameters["extrusionCenter"] : defaultParameters["extrusionCenter"];
        rotateUV = parameters.ContainsKey("rotateUV") ? parameters["rotateUV"] : defaultParameters["rotateUV"];
    }

    /*protected override void ApplyCustomSettings() {
        // Translate so it starts from (0,0,0) and add original offset to mesh position
        var startPos = start;
        end -= start;
        start = Vector3.zero;
        position += startPos;
    }*/

    protected override void Generate() {
        var absEnd = end - start;
        var cross = Vector3.Cross((absEnd).normalized, Vector3.forward).normalized;
        var lowerLeft = start - cross * thickness / 2f;
        var lowerRight = start + cross * thickness / 2f;
        var upperLeft = lowerLeft + absEnd;
        var upperRight = lowerRight + absEnd;
        var secondCross = Vector3.Cross(lowerRight - lowerLeft, upperLeft - lowerLeft).normalized;
        var lowerLeftBack = lowerLeft + secondCross * extrusion;
        var lowerRightBack = lowerRight + secondCross * extrusion;
        var upperLeftBack = upperLeft + secondCross * extrusion;
        var upperRightBack = upperRight + secondCross * extrusion;
        if (extrusionCenter) {
            lowerLeft -= secondCross * extrusion / 2f;
            lowerRight -= secondCross * extrusion / 2f;
            upperLeft -= secondCross * extrusion / 2f;
            upperRight -= secondCross * extrusion / 2f;
            lowerLeftBack -= secondCross * extrusion / 2f;
            lowerRightBack -= secondCross * extrusion / 2f;
            upperLeftBack -= secondCross * extrusion / 2f;
            upperRightBack -= secondCross * extrusion / 2f;
        } else if (extrusionOutwards) {
            lowerLeftBack -= secondCross * (2 * extrusion);
            lowerRightBack -= secondCross * (2 * extrusion);
            upperLeftBack -= secondCross * (2 * extrusion);
            upperRightBack -= secondCross * (2 * extrusion);
        }

        AddQuad(lowerLeft, upperLeft, upperRight, lowerRight, submeshIndex, extrusionOutwards, rotateUV ? UVSettings.Rotate : UVSettings.None);
        AddQuad(lowerRightBack, upperRightBack, upperLeftBack, lowerLeftBack, submeshIndex, extrusionOutwards, rotateUV ? UVSettings.Rotate : UVSettings.None);
        AddQuad(lowerLeftBack, upperLeftBack, upperLeft, lowerLeft, submeshIndex, extrusionOutwards, rotateUV ? UVSettings.Rotate : UVSettings.None);
        AddQuad(lowerRight, upperRight, upperRightBack, lowerRightBack, submeshIndex, extrusionOutwards, rotateUV ? UVSettings.Rotate : UVSettings.None);
        AddQuad(upperLeft, upperLeftBack, upperRightBack, upperRight, submeshIndex, extrusionOutwards, rotateUV ? UVSettings.Rotate : UVSettings.None);
        AddQuad(lowerRight, lowerRightBack, lowerLeftBack, lowerLeft, submeshIndex, extrusionOutwards, rotateUV ? UVSettings.Rotate : UVSettings.None);
    }
}