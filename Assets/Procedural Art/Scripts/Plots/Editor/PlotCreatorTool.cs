using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Plot Creator", typeof(PlotCreator))]
public class PlotCreatorTool : EditorTool {
    private Vector3 startPoint = Vector3.zero;
    private Vector3 currentPoint = Vector3.zero;

    private PlotCreator plotCreator;
    private bool isDragging;
    private bool isDraggingSelection;
    private bool isRemovingFromSelection;

    private HashSet<int> selectedPlots = new HashSet<int>();
    private HashSet<int> plotsUnderSelection = new HashSet<int>();
    private int hoveredPlot = -1;
    private GUIContent iconContent;
    private Tool currentTool;
    private int previousSelectedGridIndex;

    private void OnEnable() {
        var icon = EditorGUIUtility.isProSkin ? Resources.Load<Texture2D>("PlotCreator/plotCreator_pro") : Resources.Load<Texture2D>("PlotCreator/plotCreator");
        iconContent = new GUIContent {
            image = icon,
            text = "Plot Creator Tool",
            tooltip = "Drag with your mouse to create rectangular plots"
        };
        currentTool = Tool.Move;
    }

    public override GUIContent toolbarIcon => iconContent;

    public void Clear() {
        hoveredPlot = -1;
        selectedPlots.Clear();
        plotsUnderSelection.Clear();
    }

    public override void OnToolGUI(EditorWindow window) {
        plotCreator = target as PlotCreator;
        if (plotCreator.SelectedPlotGrid == null) return;
        if(previousSelectedGridIndex != plotCreator.SelectedPlotGridIndex) Clear();
        previousSelectedGridIndex = plotCreator.SelectedPlotGridIndex;
        if (plotCreator.ShowPlots) {
            for (int plotI = 0; plotI < plotCreator.PlotGrids.Count; plotI++) {
                for (var i = 0; i < plotCreator.PlotGrids[plotI].Plots.Count; i++) {
                    Color color;
                    if (plotI == plotCreator.SelectedPlotGridIndex) {
                        color = plotCreator.NormalColor;
                        if (selectedPlots.Contains(i) || (plotsUnderSelection.Contains(i) && !isRemovingFromSelection && isDraggingSelection) || (hoveredPlot == i && !isDragging)) color = plotCreator.SelectedColor;
                        if (plotsUnderSelection.Contains(i) && isRemovingFromSelection && selectedPlots.Contains(i)) color = plotCreator.UnselectColor;
                        if (!plotCreator.IsEnabled) color = Color.gray;
                    } else color = plotCreator.DisabledColor(plotI);
                    
                    var rotated = MathUtils.RotatedRectangle(plotCreator.PlotGrids[plotI].Plots[i]);
                    Handles.DrawSolidRectangleWithOutline(new[] {rotated.leftDown, rotated.leftUp, rotated.rightUp, rotated.rightDown}, color, Color.black);
                }
            }
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.W) {
            currentTool = Tool.Move;
            Event.current.Use();
        } else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.E) {
            currentTool = Tool.Rotate;
            Event.current.Use();
        } else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R) {
            currentTool = Tool.Scale;
            Event.current.Use();
        }

        if (selectedPlots.Count > 0) {
            switch (currentTool) {
                case Tool.Move: {
                    var selectionCenter = Vector3.zero;
                    foreach (var selected in selectedPlots) {
                        var bounds = plotCreator.SelectedPlotGrid.Plots[selected].Bounds;
                        selectionCenter += new Vector3(bounds.center.x, 0, bounds.center.y);

                        var labelStyle = EditorStyles.largeLabel;
                        labelStyle.fontSize = 24;
                        labelStyle.fontStyle = FontStyle.Bold;
                        Handles.Label(new Vector3(bounds.x, 0, bounds.y), $"{bounds.width} x {bounds.height}", labelStyle);
                    }

                    selectionCenter /= selectedPlots.Count;

                    EditorGUI.BeginChangeCheck();

                    var offsetToGrid = selectionCenter - selectionCenter.ClosestGridPoint(true);
                    var newPosition = Handles.PositionHandle(selectionCenter, Quaternion.identity).ClosestGridPoint(true);
                    var delta = newPosition - selectionCenter + offsetToGrid;
                    var deltaV2 = new Vector2(delta.x, delta.z);

                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(plotCreator, "Moved plot(s)");
                        foreach (var selected in selectedPlots) {
                            plotCreator.SelectedPlotGrid.Plots[selected].Bounds.center += deltaV2;
                        }

                        Event.current.Use();
                    }

                    break;
                }
                case Tool.Rotate: {
                    var labelStyle = EditorStyles.largeLabel;
                    labelStyle.fontSize = 24;
                    labelStyle.fontStyle = FontStyle.Bold;
                    foreach (var selected in selectedPlots) {
                        var currentPlot = plotCreator.SelectedPlotGrid.Plots[selected];
                        Handles.Label(new Vector3(currentPlot.Bounds.x, 0, currentPlot.Bounds.y), $"{currentPlot.Rotation}°", labelStyle);
                        EditorGUI.BeginChangeCheck();
                        var newRotation = Handles.RotationHandle(Quaternion.Euler(0, currentPlot.Rotation, 0), currentPlot.Bounds.center.ToVec3()).eulerAngles.y;
                        if (EditorGUI.EndChangeCheck()) {
                            Undo.RecordObject(plotCreator, "Rotated plot(s)");
                            currentPlot.Rotation = newRotation;
                            if (currentPlot.Rotation >= 360)
                                currentPlot.Rotation -= 360;

                            Event.current.Use();
                        }
                    }

                    break;
                }
                case Tool.Scale: {
                    foreach (var selected in selectedPlots) {
                        var currentPlot = plotCreator.SelectedPlotGrid.Plots[selected];
                        EditorGUI.BeginChangeCheck();
                        var scale = currentPlot.Bounds.size.ToVec3(1);
                        var newScale = Handles.ScaleHandle(scale, currentPlot.Bounds.center.ToVec3(), Quaternion.Euler(0, currentPlot.Rotation, 0), HandleUtility.GetHandleSize(currentPlot.Bounds.center.ToVec3())).ClosestGridPoint(false);

                        if (EditorGUI.EndChangeCheck()) {
                            newScale.y = 0;
                            if (newScale.x <= 0) newScale.x = GlobalSettings.Instance.GridSize;
                            if (newScale.y <= 0) newScale.y = GlobalSettings.Instance.GridSize;
                            Undo.RecordObject(plotCreator, "Scaled plot");
                            currentPlot.Bounds.size = newScale.ToVec2();
                        }
                    }

                    break;
                }
            }
        }

        if (selectedPlots.Count > 0 && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace) {
            Event.current.Use();
            Undo.RecordObject(plotCreator, "Deleted plots");
            var plotsToRemove = new List<int>(selectedPlots);
            plotsToRemove.Sort();
            selectedPlots.Clear();
            int offset = 0;
            foreach (var plot in plotsToRemove) {
                plotCreator.SelectedPlotGrid.Plots.RemoveAt(plot - offset);
                offset++;
            }
        }

        if (!plotCreator.IsEnabled) return;

        var rayMouse = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        // rayMouse.origin = rotationRev * rayMouse.origin;

        if (Physics.Raycast(rayMouse, out var mouseInfo)) {
            var mousePoint = mouseInfo.point.ClosestGridPoint(true);
            Handles.color = new Color(1, 1, 1, 0.1f);
            Handles.DrawSolidDisc(mousePoint, mouseInfo.normal, 0.15f);

            hoveredPlot = FindIntersectingPlot(new Vector2(mousePoint.x, mousePoint.z));
        }

        plotsUnderSelection.Clear();

        if (Event.current.type == EventType.MouseDown && !isDragging && (Event.current.modifiers == EventModifiers.None || Event.current.modifiers.OnlyTheseFlags(EventModifiers.Control) || Event.current.modifiers.OnlyTheseFlags(EventModifiers.Control | EventModifiers.Shift)) && Event.current.button != 2) {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out var info)) {
                if (Event.current.control) {
                    isDraggingSelection = true;
                }

                isDragging = true;
                startPoint = currentPoint = info.point.ClosestGridPoint(true);
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                Event.current.Use();
                /*if (Tools.current != Tool.None) {
                    currentTool = Tools.current;
                    Tools.current = Tool.None;
                }*/
            }
        }

        if (isDragging) {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var point = Vector3.zero;
            if (Physics.Raycast(ray, out var info)) {
                currentPoint = info.point.ClosestGridPoint(true);
                var size = currentPoint - startPoint;
                var alignedSize = size.ClosestGridPoint(false);
                currentPoint = startPoint + alignedSize;
                point = info.point;
            }

            var labelStyle = EditorStyles.largeLabel;
            labelStyle.fontSize = 24;
            labelStyle.fontStyle = FontStyle.Bold;
            var currentSize = (currentPoint - startPoint).Abs();
            Handles.Label((currentPoint - Vector3.right * currentSize.x), $"{currentSize.x} x {currentSize.z}", labelStyle);

            Handles.color = Color.white;
            Handles.DrawSolidDisc(startPoint, info.normal, 0.2f);
            Handles.DrawSolidDisc(currentPoint, info.normal, 0.2f);

            isDraggingSelection = Event.current.control;
            isRemovingFromSelection = isDraggingSelection && Event.current.shift;

            var minX = Mathf.Min(startPoint.x, currentPoint.x);
            var maxX = Mathf.Max(startPoint.x, currentPoint.x);
            var minZ = Mathf.Min(startPoint.z, currentPoint.z);
            var maxZ = Mathf.Max(startPoint.z, currentPoint.z);

            var leftBack = new Vector3(minX, 0, minZ);
            var rightBack = new Vector3(maxX, 0, minZ);
            var leftFront = new Vector3(minX, 0, maxZ);
            var rightFront = new Vector3(maxX, 0, maxZ);
            if (isDraggingSelection) {
                Handles.color = Color.blue;
                if (Event.current.shift)
                    Handles.color = Color.red;
            }

            Handles.DrawAAConvexPolygon(leftBack, rightBack);
            Handles.DrawAAConvexPolygon(rightBack, rightFront);
            Handles.DrawAAConvexPolygon(rightFront, leftFront);
            Handles.DrawAAConvexPolygon(leftFront, leftBack);

            var start = new Vector2(leftBack.x, leftBack.z);
            var end = new Vector2(rightFront.x, rightFront.z);
            var rect = new Rect {min = start, max = end};
            var selection = FindIntersectingPlots(rect);
            selection.ForEach(i => plotsUnderSelection.Add(i));

            if (Event.current.type == EventType.MouseUp) {
                if (Event.current.button == 0) {
                    var diff = startPoint - currentPoint;
                    if (Math.Abs(diff.x) > 0.0001f && Math.Abs(diff.z) > 0.0001f) {
                        if (isDraggingSelection) {
                            if (selection.Count > 0) {
                                if (Event.current.shift) {
                                    selectedPlots.RemoveWhere(i => selection.Contains(i));
                                } else {
                                    selection.ForEach(i => selectedPlots.Add(i));
                                }
                            }
                        } else {
                            Undo.RecordObject(plotCreator, "Created plot");
                            plotCreator.SelectedPlotGrid.Plots.Add(Plot.FromStartEnd(start, end));
                        }
                    } else {
                        var selectedPlot = FindIntersectingPlot(new Vector2(point.x, point.z));
                        if (selectedPlot != -1) {
                            if (Event.current.control) {
                                if (selectedPlots.Contains(selectedPlot)) selectedPlots.Remove(selectedPlot);
                                else selectedPlots.Add(selectedPlot);
                            } else {
                                selectedPlots.Clear();
                                selectedPlots.Add(selectedPlot);
                            }
                        } else {
                            if (!Event.current.control) selectedPlots.Clear();
                        }
                    }

                    // if (selectedPlots.Count == 0) Tools.current = currentTool;
                }

                Handles.color = plotCreator.NormalColor;
                isDragging = false;
                isDraggingSelection = false;
                GUIUtility.hotControl = 0;
                Event.current.Use();

                // Tools.current = currentTool;
            }
        }
    }

    private int FindIntersectingPlot(Vector2 position) {
        for (var i = 0; i < plotCreator.SelectedPlotGrid.Plots.Count; i++) {
            if (MathUtils.PointInRotatedRectangle(position, plotCreator.SelectedPlotGrid.Plots[i]))
                return i;
        }

        return -1;
    }

    private List<int> FindIntersectingPlots(Rect rect) {
        var plots = new List<int>();
        for (var i = 0; i < plotCreator.SelectedPlotGrid.Plots.Count; i++) {
            var current = plotCreator.SelectedPlotGrid.Plots[i];
            if (MathUtils.RectangleContains(rect, current))
                plots.Add(i);
        }

        return plots;
    }
}