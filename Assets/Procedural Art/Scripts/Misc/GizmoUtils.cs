using UnityEngine;

public static class GizmoUtils {
    public static void DrawLine(Vector3 p1, Vector3 p2, float width) {
        var count = Mathf.CeilToInt(width);
        if (count == 1) {
            Gizmos.DrawLine(p1, p2);
        } else {
            var c = Camera.current;
            if (c == null) {
                Debug.LogError("Camera.current is null");
                return;
            }

            var scp1 = c.WorldToScreenPoint(p1);
            var scp2 = c.WorldToScreenPoint(p2);

            var v1 = (scp2 - scp1).normalized; // line direction
            var n = Vector3.Cross(v1, Vector3.forward); // normal vector

            for (var i = 0; i < count; i++) {
                var o = 0.99f * n * width * ((float) i / (count - 1) - 0.5f);
                var origin = c.ScreenToWorldPoint(scp1 + o);
                var destiny = c.ScreenToWorldPoint(scp2 + o);
                Gizmos.DrawLine(origin, destiny);
            }
        }
    }
}