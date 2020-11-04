using UnityEngine;

public class GridGizmos : MonoBehaviour {
    public PlotCreator PlotCreator;
    [Header("Grid Settings")]
    [Min(1)] public float MajorGridLineWidth = 2f;
    [Min(1)] public float MinorGridLineWidth = 1f;
    public Vector2Int GridBounds;
    [Space]
    public Color MajorGridLineColor;
    public Color MinorGridLineColor;
    public bool ShowGizmos = true;

    private void OnDrawGizmos() {
        if (!ShowGizmos) return;
        for (var x = (float) -GridBounds.x; x <= GridBounds.x; x += GlobalSettings.Instance.GridSize) {
            Gizmos.color = MajorGridLineColor;
            var start = new Vector3(x, .01f, -GridBounds.y);
            var end = new Vector3(x, .01f, GridBounds.y);
            GizmoUtils.DrawLine(start, end, MajorGridLineWidth);
            if (x >= GridBounds.x)
                continue;

            Gizmos.color = MinorGridLineColor;
            for (var x1 = x + GlobalSettings.Instance.GridSizeMinor; x1 < x + GlobalSettings.Instance.GridSize; x1 += GlobalSettings.Instance.GridSizeMinor) {
                var start1 = new Vector3(x1, .01f, -GridBounds.y);
                var end1 = new Vector3(x1, .01f, GridBounds.y);
                GizmoUtils.DrawLine(start1, end1, MinorGridLineWidth);
            }
        }

        for (var y = (float) -GridBounds.y; y <= GridBounds.y; y += GlobalSettings.Instance.GridSize) {
            Gizmos.color = MajorGridLineColor;
            var start = new Vector3(-GridBounds.x, .01f, y);
            var end = new Vector3(GridBounds.x, .01f, y);
            GizmoUtils.DrawLine(start, end, MajorGridLineWidth);
            if (y >= GridBounds.y)
                continue;

            Gizmos.color = MinorGridLineColor;
            for (var y1 = y + GlobalSettings.Instance.GridSizeMinor; y1 < y + GlobalSettings.Instance.GridSize; y1 += GlobalSettings.Instance.GridSizeMinor) {
                var start1 = new Vector3(-GridBounds.x, .01f, y1);
                var end1 = new Vector3(GridBounds.x, .01f, y1);
                GizmoUtils.DrawLine(start1, end1, MinorGridLineWidth);
            }
        }
    }
}