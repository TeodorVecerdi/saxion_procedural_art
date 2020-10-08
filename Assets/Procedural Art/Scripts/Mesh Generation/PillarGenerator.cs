using System.Collections.Generic;
using UnityEngine;

public class PillarGenerator : MeshGenerator {
    private float width;
    private float height;
    private bool thicknessInwards;
    private bool thicknessOutwards;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"width", 0.1f},
            {"height", 1f},
            {"thicknessInwards", false},
            {"thicknessOutwards", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        width = (parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"]) * GlobalSettings.Instance.GridSize;
        height = (parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"]) * GlobalSettings.Instance.GridSize;
        thicknessInwards = parameters.ContainsKey("thicknessInwards") ? parameters["thicknessInwards"] : defaultParameters["thicknessInwards"];
        thicknessOutwards = parameters.ContainsKey("thicknessOutwards") ? parameters["thicknessOutwards"] : defaultParameters["thicknessOutwards"];
    }

    protected override void ApplyCustomSettings() {
        position += Vector3.right * (width / 2f);
    }

    protected override void Generate() {
        // Outwards thickness
        var bottomLeft1 = Vector3.zero;
        var bottomRight1 = bottomLeft1 + Vector3.right * width;
        var topLeft1 = bottomLeft1 + Vector3.up * height;
        var topRight1 = bottomRight1 + Vector3.up * height;
        var bottomLeft2 = bottomLeft1 + Vector3.forward * width;
        var bottomRight2 = bottomRight1 + Vector3.forward * width;
        var topLeft2 = topLeft1 + Vector3.forward * width;
        var topRight2 = topRight1 + Vector3.forward * width;

        // Middle thickness
        if (!(thicknessInwards ^ thicknessOutwards)) {
            bottomLeft1 -= Vector3.forward * (width / 2f);
            bottomRight1 -= Vector3.forward * (width / 2f);
            topLeft1 -= Vector3.forward * (width / 2f);
            topRight1 -= Vector3.forward * (width / 2f);
            bottomLeft2 -= Vector3.forward * (width / 2f);
            bottomRight2 -= Vector3.forward * (width / 2f);
            topLeft2 -= Vector3.forward * (width / 2f);
            topRight2 -= Vector3.forward * (width / 2f);
        } else if (thicknessInwards) {
            bottomLeft1 -= Vector3.forward * width;
            bottomRight1 -= Vector3.forward * width;
            topLeft1 -= Vector3.forward * width;
            topRight1 -= Vector3.forward * width;
            bottomLeft2 -= Vector3.forward * width;
            bottomRight2 -= Vector3.forward * width;
            topLeft2 -= Vector3.forward * width;
            topRight2 -= Vector3.forward * width;
        }

        // Front & Back
        AddQuad(bottomLeft1, topLeft1, topRight1, bottomRight1, 0);
        AddQuad(bottomRight2, topRight2, topLeft2, bottomLeft2, 0);

        // East & West
        AddQuad(bottomRight1, topRight1, topRight2, bottomRight2, 0);
        AddQuad(bottomLeft2, topLeft2, topLeft1, bottomLeft1, 0);

        // North & South
        AddQuad(topLeft1, topLeft2, topRight2, topRight1, 0);
        AddQuad(bottomLeft2, bottomLeft1, bottomRight1, bottomRight2, 0);
    }
}