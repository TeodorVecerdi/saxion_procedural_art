﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CameraRotator : MonoBehaviour {
    public float RotationSpeed = 2f;

    private void OnEnable() {
        EditorApplication.update += Update;
    }

    private void OnDisable() {
        EditorApplication.update -= Update;
    }

    private void Update() {
        transform.localEulerAngles += new Vector3(0, RotationSpeed, 0);
    }
}