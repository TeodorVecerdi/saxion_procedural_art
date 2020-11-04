using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class SceneCameraController : MonoBehaviour {
    public Transform Position;
    public Transform Rotation;

    private void OnEnable() {
        EditorApplication.update += Update;
    }

    private void OnDisable() {
        EditorApplication.update -= Update;
    }

    private void Update() {
        if (Position == null || Rotation == null) return;

        SceneView.lastActiveSceneView.pivot = Rotation.position;
        SceneView.lastActiveSceneView.rotation = Quaternion.LookRotation(Rotation.position - Position.position);
        SceneView.lastActiveSceneView.Repaint();
    }
}