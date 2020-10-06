using System.Collections.Generic;
using UnityEngine;

public abstract class MeshGenerator {
    protected List<Vector3> vertices;
    protected List<int> triangles;
    protected Vector3 position;
    protected Quaternion rotation;
    protected Dictionary<string, dynamic> defaultParameters;
    

    protected abstract void SetDefaultSettings();
    protected abstract void DeconstructSettings(Dictionary<string, dynamic> parameters);
    protected virtual void ApplyCustomSettings(){}
    protected abstract void Generate();
    

    public static (List<Vector3> vertices, List<int> triangles) GetMesh<T>(Vector3 position, Quaternion rotation, Dictionary<string, dynamic> parameters) where T : MeshGenerator, new() {
        var generator = new T();
        return generator.GetMesh(position, rotation, parameters);
    }


    private (List<Vector3> vertices, List<int> triangles) GetMesh(Vector3 position, Quaternion rotation, Dictionary<string, dynamic> parameters) {
        this.position = position;
        this.rotation = rotation;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        SetDefaultSettings();
        DeconstructSettings(parameters);
        ApplyCustomSettings();
        Generate();
        ApplyTransformation();
        return (vertices, triangles);
    }

    private void ApplyTransformation() {
        for (var i = 0; i < vertices.Count; i++) {
            vertices[i] = rotation * vertices[i] + position * GlobalSettings.Instance.GridSize;
        }
    }
    
    protected void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, bool flip = false) {
        var quadIndex = vertices.Count;
        if (flip) {
            vertices.Add(v3);
            vertices.Add(v2);
            vertices.Add(v1);
            vertices.Add(v0);
        } else {
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
        }

        triangles.Add(quadIndex);
        triangles.Add(quadIndex + 1);
        triangles.Add(quadIndex + 2);
        triangles.Add(quadIndex);
        triangles.Add(quadIndex + 2);
        triangles.Add(quadIndex + 3);
    }

    protected void AddTriangle(Vector3 v0, Vector3 v1, Vector3 v2, bool flip = false) {
        var triangleIndex = vertices.Count;
        if (flip) {
            vertices.Add(v2);
            vertices.Add(v1);
            vertices.Add(v0);
        } else {
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
        }

        triangles.Add(triangleIndex);
        triangles.Add(triangleIndex + 1);
        triangles.Add(triangleIndex + 2);
    }
}