using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineComponent))]
public class SplineComponentEditor : Editor {
    private int hotIndex = -1;
    private int removeIndex = -1;

    public override void OnInspectorGUI() {
        EditorGUILayout.HelpBox("Hold Shift and click to append and insert curve points. Backspace to delete points.", MessageType.Info);
        var spline = target as SplineComponent;
        GUILayout.BeginHorizontal();
        var closed = GUILayout.Toggle(spline.closed, "Closed", "button");
        if (spline.closed != closed) {
            spline.closed = closed;
            spline.ResetIndex();
        }

        if (GUILayout.Button("Flatten Y Axis")) {
            Undo.RecordObject(target, "Flatten Y Axis");

            Flatten(spline.points);
            spline.ResetIndex();
        }

        if (GUILayout.Button("Center around Origin")) {
            Undo.RecordObject(target, "Center around Origin");

            CenterAroundOrigin(spline.points);
            spline.ResetIndex();
        }

        GUILayout.EndHorizontal();
        spline.Resolution = EditorGUILayout.IntSlider("Resolution", spline.Resolution, 2, 2048);
    }

    private void OnSceneGUI() {
        var spline = target as SplineComponent;
        var e = Event.current;
        GUIUtility.GetControlID(FocusType.Passive);

        var mousePos = Event.current.mousePosition;
        var view = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);
        var mouseIsOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
        if (mouseIsOutside) return;

        var points = serializedObject.FindProperty("points");
        if (Event.current.shift) {
            if (spline.closed)
                ShowClosestPointOnClosedSpline(points);
            else
                ShowClosestPointOnOpenSpline(points);
        }

        for (int i = 0; i < spline.points.Count; i++) {
            var prop = points.GetArrayElementAtIndex(i);
            var point = prop.vector3Value;
            var wp = spline.transform.TransformPoint(point);
            if (hotIndex == i) {
                var newWp = Handles.PositionHandle(wp, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : spline.transform.rotation);
                var delta = spline.transform.InverseTransformDirection(newWp - wp);
                if (delta.sqrMagnitude > 0) {
                    prop.vector3Value = point + delta;
                    spline.ResetIndex();
                }

                HandleCommands(wp);
            }

            Handles.color = i == 0 | i == spline.points.Count - 1 ? Color.red : Color.white;
            var buttonSize = HandleUtility.GetHandleSize(wp) * 0.1f;
            if (Handles.Button(wp, Quaternion.identity, buttonSize, buttonSize, Handles.SphereHandleCap))
                hotIndex = i;
            var v = SceneView.currentDrawingSceneView.camera.transform.InverseTransformPoint(wp);
            var labelIsOutside = v.z < 0;
            if (!labelIsOutside) Handles.Label(wp, i.ToString());
        }

        if (removeIndex >= 0 && points.arraySize > 4) {
            points.DeleteArrayElementAtIndex(removeIndex);
            spline.ResetIndex();
        }

        removeIndex = -1;
        serializedObject.ApplyModifiedProperties();
    }

    private void HandleCommands(Vector3 wp) {
        if (Event.current.type == EventType.ExecuteCommand) {
            if (Event.current.commandName == "FrameSelected") {
                SceneView.currentDrawingSceneView.Frame(new Bounds(wp, Vector3.one * 10), false);
                Event.current.Use();
            }
        }

        if (Event.current.type == EventType.KeyDown) {
            if (Event.current.keyCode == KeyCode.Backspace) {
                removeIndex = hotIndex;
                Event.current.Use();
            }
        }
    }

    private void ShowClosestPointOnClosedSpline(SerializedProperty points) {
        var spline = target as SplineComponent;
        var plane = new Plane(spline.transform.up, spline.transform.position);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float center;
        if (plane.Raycast(ray, out center)) {
            var hit = ray.origin + ray.direction * center;
            Handles.DrawWireDisc(hit, spline.transform.up, 5);
            var p = SearchForClosestPoint(Event.current.mousePosition);
            var sp = spline.GetNonUniformPoint(p);
            Handles.DrawLine(hit, sp);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift) {
                var i = (Mathf.FloorToInt(p * spline.points.Count) + 2) % spline.points.Count;
                points.InsertArrayElementAtIndex(i);
                points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
                serializedObject.ApplyModifiedProperties();
                hotIndex = i;
            }
        }
    }

    private void ShowClosestPointOnOpenSpline(SerializedProperty points) {
        var spline = target as SplineComponent;
        var plane = new Plane(spline.transform.up, spline.transform.position);
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float center;
        if (plane.Raycast(ray, out center)) {
            var hit = ray.origin + ray.direction * center;
            var discSize = HandleUtility.GetHandleSize(hit);
            Handles.DrawWireDisc(hit, spline.transform.up, discSize);
            var p = SearchForClosestPoint(Event.current.mousePosition);

            if ((hit - spline.GetNonUniformPoint(0)).sqrMagnitude < 25) p = 0;
            if ((hit - spline.GetNonUniformPoint(1)).sqrMagnitude < 25) p = 1;

            var sp = spline.GetNonUniformPoint(p);

            var extend = Mathf.Approximately(p, 0) || Mathf.Approximately(p, 1);

            Handles.color = extend ? Color.red : Color.white;
            Handles.DrawLine(hit, sp);
            Handles.color = Color.white;

            var i = 1 + Mathf.FloorToInt(p * (spline.points.Count - 3));

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift) {
                if (extend) {
                    if (i == spline.points.Count - 2) i++;
                    points.InsertArrayElementAtIndex(i);
                    points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(hit);
                    hotIndex = i;
                } else {
                    i++;
                    points.InsertArrayElementAtIndex(i);
                    points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
                    hotIndex = i;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private float SearchForClosestPoint(Vector2 screenPoint, float A = 0f, float B = 1f, float steps = 1000) {
        var spline = target as SplineComponent;
        var smallestDelta = float.MaxValue;
        var step = (B - A) / steps;
        var closestI = A;
        for (var i = 0; i <= steps; i++) {
            var p = spline.GetNonUniformPoint(i * step);
            var gp = HandleUtility.WorldToGUIPoint(p);
            var delta = (screenPoint - gp).sqrMagnitude;
            if (delta < smallestDelta) {
                closestI = i;
                smallestDelta = delta;
            }
        }

        return closestI * step;
    }

    private void Flatten(List<Vector3> points) {
        for (var i = 0; i < points.Count; i++) {
            points[i] = Vector3.Scale(points[i], new Vector3(1, 0, 1));
        }
    }

    private void CenterAroundOrigin(List<Vector3> points) {
        var center = Vector3.zero;
        foreach (var point in points) {
            center += point;
        }

        center /= points.Count;
        for (var i = 0; i < points.Count; i++) {
            points[i] -= center;
        }
    }

    [DrawGizmo(GizmoType.NonSelected)]
    private static void DrawGizmosLoRes(SplineComponent spline, GizmoType gizmoType) {
        Gizmos.color = Color.white;
        DrawGizmo(spline, 64);
    }

    [DrawGizmo(GizmoType.Selected)]
    private static void DrawGizmosHiRes(SplineComponent spline, GizmoType gizmoType) {
        Gizmos.color = Color.white;
        if (spline.Resolution == 0) spline.Resolution = 128;
        DrawGizmo(spline, spline.Resolution);
    }

    private static void DrawGizmo(SplineComponent spline, int stepCount) {
        if (spline.points.Count <= 0)
            return;
        var p = 0f;
        var start = spline.GetNonUniformPoint(0);
        var step = 1f / stepCount;
        do {
            p += step;
            var here = spline.GetNonUniformPoint(p);
            Gizmos.DrawLine(start, here);
            start = here;
        } while (p + step <= 1);
    }
}