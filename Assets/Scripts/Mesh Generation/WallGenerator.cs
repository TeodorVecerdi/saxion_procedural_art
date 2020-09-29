using System.Collections.Generic;
using UnityEngine;

public class WallGenerator {
    private float width;
    private float height;
    private float thickness;
    private Vector3 vertexOffset;
    private Quaternion rotation;

    private bool thicknessInwards;
    private bool thicknessOutwards; // if neither of these two are set, it will be in the middle

    internal List<Vector3> vertices;
    internal List<int> triangles;

    public static (List<Vector3> vertices, List<int> triangles) Generate(float width, float height, float thickness, Vector3 vertexOffset, Quaternion rotation, bool thicknessInwards = false, bool thicknessOutwards = false) {
        var wallGen = new WallGenerator(width, height, thickness, vertexOffset, rotation, thicknessInwards, thicknessOutwards);
        return (wallGen.vertices, wallGen.triangles);
    }

    public WallGenerator(float width, float height, float thickness, Vector3 vertexOffset, Quaternion rotation, bool thicknessInwards = false, bool thicknessOutwards = false) {
        this.width = width;
        this.height = height;
        this.thickness = thickness;
        this.vertexOffset = vertexOffset;
        this.rotation = rotation;
        this.thicknessInwards = thicknessInwards;
        this.thicknessOutwards = thicknessOutwards;
        Generate();
    }

    private void Generate() {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        // Outwards thickness
        var bottomLeft1 = Vector3.zero;
        var bottomRight1 = bottomLeft1 + Vector3.right * width;
        var topLeft1 = bottomLeft1 + Vector3.up * height;
        var topRight1 = bottomRight1 + Vector3.up * height;
        var bottomLeft2 = bottomLeft1 + Vector3.forward * thickness;
        var bottomRight2 = bottomRight1 + Vector3.forward * thickness;
        var topLeft2 = topLeft1 + Vector3.forward * thickness;
        var topRight2 = topRight1 + Vector3.forward * thickness;
        
        // Middle thickness
        if (!(thicknessInwards^thicknessOutwards)) {
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
        AddQuad(bottomLeft1, topLeft1, topRight1, bottomRight1);
        AddQuad(bottomRight2, topRight2, topLeft2, bottomLeft2);
        
        // East & West
        AddQuad(bottomRight1, topRight1, topRight2, bottomRight2);
        AddQuad(bottomLeft2, topLeft2, topLeft1, bottomLeft1);
        
        // North & South
        AddQuad(topLeft1, topLeft2, topRight2, topRight1);
        AddQuad(bottomLeft2, bottomLeft1, bottomRight1, bottomRight2);
        
        // Rotation & Translation
        for (var i = 0; i < vertices.Count; i++) {
            vertices[i] = rotation * vertices[i];
            vertices[i] += vertexOffset;
        }
    }

    private void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3) {
        var quadIndex = vertices.Count;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(quadIndex);
        triangles.Add(quadIndex + 1);
        triangles.Add(quadIndex + 2);
        triangles.Add(quadIndex);
        triangles.Add(quadIndex + 2);
        triangles.Add(quadIndex + 3);
    }

    private void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, bool flip) {
        var triangleIndex = vertices.Count;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);

        triangles.Add(triangleIndex);
        triangles.Add(triangleIndex + 1);
        triangles.Add(triangleIndex + 2);
    }
}