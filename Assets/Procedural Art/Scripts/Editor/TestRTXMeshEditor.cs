using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestRTXMesh))]
public class TestRTXMeshEditor : Editor {
    
    public override void OnInspectorGUI() {
        var obj = target as TestRTXMesh;
        if (GUILayout.Button("BuildMesh")) {
            obj.Generate();
        }

        if (GUILayout.Button("Fix Mesh References")) {
            obj.FixRef();
        }
        
        base.OnInspectorGUI();
    }
}