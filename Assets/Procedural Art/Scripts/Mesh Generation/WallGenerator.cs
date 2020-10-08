using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class WallGenerator : MeshGenerator {
    private float width;
    private float height;
    private float thickness;
    private bool thicknessInwards;
    private bool thicknessOutwards; // if neither of these two are set, it will be in the middle
    private bool flip;

    protected override void SetDefaultSettings() {
        defaultParameters = new Dictionary<string, dynamic> {
            {"width", 1f},
            {"height", 1f},
            {"thickness", 0.1f},
            {"thicknessInwards", false},
            {"thicknessOutwards", false},
            {"flip", false}
        };
    }

    protected override void DeconstructSettings(Dictionary<string, dynamic> parameters) {
        width = (parameters.ContainsKey("width") ? parameters["width"] : defaultParameters["width"]) * GlobalSettings.Instance.GridSize;
        height = (parameters.ContainsKey("height") ? parameters["height"] : defaultParameters["height"]) * GlobalSettings.Instance.GridSize;
        thickness = (parameters.ContainsKey("thickness") ? parameters["thickness"] : defaultParameters["thickness"]) * GlobalSettings.Instance.GridSize;
        thicknessInwards = parameters.ContainsKey("thicknessInwards") ? parameters["thicknessInwards"] : defaultParameters["thicknessInwards"];
        thicknessOutwards = parameters.ContainsKey("thicknessOutwards") ? parameters["thicknessOutwards"] : defaultParameters["thicknessOutwards"];
        flip = parameters.ContainsKey("flip") ? parameters["flip"] : defaultParameters["flip"];
    }

    protected override void Generate() {
        // Outwards thickness
        var bottomLeft1 = Vector3.zero;
        var bottomRight1 = bottomLeft1 + (flip ? Vector3.left : Vector3.right) * width;
        var topLeft1 = bottomLeft1 + Vector3.up * height;
        var topRight1 = bottomRight1 + Vector3.up * height;
        var bottomLeft2 = bottomLeft1 + Vector3.forward * thickness;
        var bottomRight2 = bottomRight1 + Vector3.forward * thickness;
        var topLeft2 = topLeft1 + Vector3.forward * thickness;
        var topRight2 = topRight1 + Vector3.forward * thickness;

        // Middle thickness
        if (!(thicknessInwards ^ thicknessOutwards)) {
            bottomLeft1 -= Vector3.forward * (thickness / 2f);
            bottomRight1 -= Vector3.forward * (thickness / 2f);
            topLeft1 -= Vector3.forward * (thickness / 2f);
            topRight1 -= Vector3.forward * (thickness / 2f);
            bottomLeft2 -= Vector3.forward * (thickness / 2f);
            bottomRight2 -= Vector3.forward * (thickness / 2f);
            topLeft2 -= Vector3.forward * (thickness / 2f);
            topRight2 -= Vector3.forward * (thickness / 2f);
        } else if (thicknessInwards) {
            bottomLeft1 -= Vector3.forward * thickness;
            bottomRight1 -= Vector3.forward * thickness;
            topLeft1 -= Vector3.forward * thickness;
            topRight1 -= Vector3.forward * thickness;
            bottomLeft2 -= Vector3.forward * thickness;
            bottomRight2 -= Vector3.forward * thickness;
            topLeft2 -= Vector3.forward * thickness;
            topRight2 -= Vector3.forward * thickness;
        }

        // Front & Back
        AddQuad(bottomLeft1, topLeft1, topRight1, bottomRight1, 0, flip);
        // meshData.UVs.AddRange(new List<Vector2> {Vector2.zero, Vector2.up * height, Vector2.up * height + Vector2.right * width, Vector2.right * width});
        AddQuad(bottomRight2, topRight2, topLeft2, bottomLeft2, 0, flip);
        // meshData.UVs.AddRange(new List<Vector2> {Vector2.zero, Vector2.up * height, Vector2.up * height + Vector2.right * width, Vector2.right * width});

        // East & West
        AddQuad(bottomRight1, topRight1, topRight2, bottomRight2, 0, flip);
        // meshData.UVs.AddRange(new List<Vector2> {Vector2.zero, Vector2.up * height, Vector2.up * height + Vector2.right * thickness, Vector2.right * thickness});
        AddQuad(bottomLeft2, topLeft2, topLeft1, bottomLeft1, 0, flip);
        // meshData.UVs.AddRange(new List<Vector2> {Vector2.zero, Vector2.up * height, Vector2.up * height + Vector2.right * thickness, Vector2.right * thickness});

        // North & South
        AddQuad(topLeft1, topLeft2, topRight2, topRight1, 0, flip);
        // meshData.UVs.AddRange(new List<Vector2> {Vector2.zero, Vector2.up * thickness, Vector2.up * thickness + Vector2.right * width, Vector2.right * width});
        AddQuad(bottomLeft2, bottomLeft1, bottomRight1, bottomRight2, 0, flip);
        // meshData.UVs.AddRange(new List<Vector2> {Vector2.zero, Vector2.up * thickness, Vector2.up * thickness + Vector2.right * width, Vector2.right * width});
    }
}