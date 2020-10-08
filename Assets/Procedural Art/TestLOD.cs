using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TestLOD : MonoBehaviour {
    public float DistanceMultiplier = 1000;
    private TestArchBuilder builder;

    private void Update() {
        if (builder == null) builder = GetComponent<TestArchBuilder>();
        
        var dist = (builder.transform.position - SceneView.GetAllSceneCameras()[0].transform.position).magnitude;
        var points = Mathf.Min(Mathf.Max(4, Mathf.FloorToInt(DistanceMultiplier / dist)), 200);
        
        if (points == builder.Points)
            return;
        builder.Points = points;
        builder.Generate();
    }
}