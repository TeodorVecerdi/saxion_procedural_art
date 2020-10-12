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

    private void OnEnable() {
        var icon = EditorGUIUtility.isProSkin ? Resources.Load<Texture2D>("PlotCreator/plotCreator_pro") : Resources.Load<Texture2D>("PlotCreator/plotCreator");
        iconContent = new GUIContent {
            image = icon,
            text = "Plot Creator Tool",
            tooltip = "Drag with your mouse to create rectangular plots"
        };
    }

    public override GUIContent toolbarIcon => iconContent;

    public override void OnToolGUI(EditorWindow window) {
        plotCreator = target as PlotCreator;

        if (plotCreator.ShowPlots) {
            for (var i = 0; i < plotCreator.Plots.Count; i++) {
                var color = plotCreator.NormalColor;
                if (selectedPlots.Contains(i) || (plotsUnderSelection.Contains(i) && !isRemovingFromSelection && isDraggingSelection) || (hoveredPlot == i && !isDragging)) color = plotCreator.SelectedColor;
                if (plotsUnderSelection.Contains(i) && isRemovingFromSelection && selectedPlots.Contains(i)) color = plotCreator.UnselectColor;
                if (!plotCreator.IsEnabled) color = Color.gray;

                var leftDown = new Vector3(plotCreator.Plots[i].Bounds.min.x, 0, plotCreator.Plots[i].Bounds.min.y);
                var rightDown = new Vector3(plotCreator.Plots[i].Bounds.min.x + plotCreator.Plots[i].Bounds.width, 0, plotCreator.Plots[i].Bounds.min.y);
                var rightUp = new Vector3(plotCreator.Plots[i].Bounds.min.x + plotCreator.Plots[i].Bounds.width, 0, plotCreator.Plots[i].Bounds.min.y + plotCreator.Plots[i].Bounds.height);
                var leftUp = new Vector3(plotCreator.Plots[i].Bounds.min.x, 0, plotCreator.Plots[i].Bounds.min.y + plotCreator.Plots[i].Bounds.height);
                Handles.DrawSolidRectangleWithOutline(new[] {leftDown, leftUp, rightUp, rightDown}, color, Color.black);
            }
        }

        if (selectedPlots.Count > 0) {
            var selectionCenter = Vector3.zero;
            foreach (var selected in selectedPlots) {
                var bounds = plotCreator.Plots[selected].Bounds;
                selectionCenter += new Vector3(bounds.center.x, 0, bounds.center.y);
                
                var labelStyle = EditorStyles.largeLabel;
                labelStyle.fontSize = 24;
                labelStyle.fontStyle = FontStyle.Bold;
                Handles.Label((new Vector3(bounds.x, 0, bounds.y)), $"{bounds.width} x {bounds.height}", labelStyle);
            }

            selectionCenter /= selectedPlots.Count;

            EditorGUI.BeginChangeCheck();
            var offsetToGrid = selectionCenter - selectionCenter.ClosestGridPoint(plotCreator.UseSubgrid);
            var newPosition = Handles.PositionHandle(selectionCenter, Quaternion.identity).ClosestGridPoint(plotCreator.UseSubgrid);
            var delta = newPosition - selectionCenter + offsetToGrid;
            var deltaV2 = new Vector2(delta.x, delta.z);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(plotCreator, "Moved plot");
                foreach (var selected in selectedPlots) {
                    plotCreator.Plots[selected].Bounds.center += deltaV2;
                }

                Event.current.Use();
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
                plotCreator.Plots.RemoveAt(plot - offset);
                offset++;
            }
        }

        if (!plotCreator.IsEnabled) return;

        var rayMouse = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(rayMouse, out var mouseInfo)) {
            var mousePoint = mouseInfo.point.ClosestGridPoint(plotCreator.UseSubgrid);
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
                startPoint = currentPoint = info.point.ClosestGridPoint(plotCreator.UseSubgrid);
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
                currentPoint = info.point.ClosestGridPoint(plotCreator.UseSubgrid);
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
                            plotCreator.Plots.Add(Plot.FromStartEnd(start, end));
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
        for (var i = 0; i < plotCreator.Plots.Count; i++) {
            var plotBounds = plotCreator.Plots[i].Bounds;
            plotBounds.size += Vector2.one * 0.005f;
            if (plotBounds.Contains(position, true))
                return i;
        }

        return -1;
    }

    private List<int> FindIntersectingPlots(Rect rect) {
        var plots = new List<int>();
        for (var i = 0; i < plotCreator.Plots.Count; i++) {
            if (plotCreator.Plots[i].Bounds.Overlaps(rect, true) || rect.Overlaps(plotCreator.Plots[i].Bounds, true))
                plots.Add(i);
        }

        return plots;
    }
}