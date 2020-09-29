using System.Collections.Generic;
using UnityEngine;

public static class PillarGenerator {
    public static (List<Vector3> vertices, List<int> triangles) Generate(float height, float thickness, Vector3 vertexOffset, Quaternion rotation, bool thicknessInwards = false, bool thicknessOutwards = false) {
        var wallGen = new WallGenerator(thickness, height, thickness, vertexOffset + Vector3.right * (thickness/2f), rotation, thicknessInwards, thicknessOutwards);
        return (wallGen.vertices, wallGen.triangles);
    }
}