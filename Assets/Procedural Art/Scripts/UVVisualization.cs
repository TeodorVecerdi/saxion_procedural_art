using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class UVVisualization : MonoBehaviour {
    public Vector3 V0 = Vector3.zero;
    public Vector3 V1 = Vector3.up;
    public Vector3 V2 = Vector3.up + Vector3.right;
    public Vector3 V3 = Vector3.right;

    public bool ShowProjection;
    public bool ShowPerpendiculars;

    public Vector3 Middle(Vector3 v1, Vector3 v2, Vector3 v3) => (v1 + v2 + v3) / 3;

    private void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(V0, V1);
        Gizmos.DrawLine(V1, V2);
        Gizmos.DrawLine(V2, V0);
        Gizmos.DrawLine(V0, Middle(V0, V1, V2));
        Gizmos.DrawLine(V1, Middle(V0, V1, V2));
        Gizmos.DrawLine(V2, Middle(V0, V1, V2));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(V0, V2);
        Gizmos.DrawLine(V2, V3);
        Gizmos.DrawLine(V3, V0);
        Gizmos.DrawLine(V0, Middle(V0, V2, V3));
        Gizmos.DrawLine(V2, Middle(V0, V2, V3));
        Gizmos.DrawLine(V3, Middle(V0, V2, V3));

        // PROJECTION
        var v0_1 = V0 - V0;
        var v1_1 = V1 - V0;
        var v2_1 = V2 - V0;
        var v3_1 = V3 - V0;
        /*    STEP 1.2: Cross products */
        var triangleNormalZ = Vector3.Cross(v2_1, v1_1).normalized;
        var axisNormalZ = Vector3.Cross(triangleNormalZ, Vector3.forward);
        var angleZ = Vector3.SignedAngle(triangleNormalZ, Vector3.forward, Vector3.right);
        var rotationZ = Quaternion.AngleAxis(angleZ, axisNormalZ);

        var triangleNormalX = Vector3.Cross(v1_1, v2_1).normalized;
        var axisNormalX = Vector3.Cross(triangleNormalX, Vector3.right);
        var angleX = Vector3.SignedAngle(triangleNormalX, Vector3.right, Vector3.forward);
        var rotationX = Quaternion.AngleAxis(angleX, axisNormalX);

        // var angle = Vector3.Angle(triangleNormal, Vector3.forward);
        // var rotationZ = Quaternion.Euler(0, 0, angle);

        var v0_2 = /*rotationX * */ rotationZ * v0_1;
        var v1_2 = /*rotationX * */ rotationZ * v1_1;
        var v2_2 = /*rotationX * */ rotationZ * v2_1;
        var v3_2 = /*rotationX * */ rotationZ * v3_1;

        /*if (ShowPerpendiculars) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(v0_1, U);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(v0_1, W);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(v0_1, V);
        }*/
        var v0_3 = v0_1;
        var v1_3 = v1_1;
        var v2_3 = v2_1;
        var v3_3 = v3_1;
        var triangleNormal = Vector3.Cross(v1_3 - v0_3, v3_3 - v0_3);
        if (triangleNormal.x > triangleNormal.y && triangleNormal.x > triangleNormal.z) {
            v0_3.x = 0;
            v1_3.x = 0;
            v2_3.x = 0;
            v3_3.x = 0;
        } else if (triangleNormal.y > triangleNormal.x && triangleNormal.y > triangleNormal.z) {
            v0_3.y = 0;
            v1_3.y = 0;
            v2_3.y = 0;
            v3_3.y = 0;
        } else if (triangleNormal.z > triangleNormal.x && triangleNormal.z > triangleNormal.y) {
            v0_3.z = 0;
            v1_3.z = 0;
            v2_3.z = 0;
            v3_3.z = 0;
        }

        if (ShowProjection) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(v0_2, v1_2);
            Gizmos.DrawLine(v1_2, v2_2);
            Gizmos.DrawLine(v2_2, v0_2);
            Gizmos.DrawLine(v0_2, Middle(v0_2, v1_2, v2_2));
            Gizmos.DrawLine(v1_2, Middle(v0_2, v1_2, v2_2));
            Gizmos.DrawLine(v2_2, Middle(v0_2, v1_2, v2_2));
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(v0_2, v2_2);
            Gizmos.DrawLine(v2_2, v3_2);
            Gizmos.DrawLine(v3_2, v0_2);
            Gizmos.DrawLine(v0_2, Middle(v0_2, v2_2, v3_2));
            Gizmos.DrawLine(v2_2, Middle(v0_2, v2_2, v3_2));
            Gizmos.DrawLine(v3_2, Middle(v0_2, v2_2, v3_2));
        }

        if (ShowPerpendiculars) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(v0_3, v1_3);
            Gizmos.DrawLine(v1_3, v2_3);
            Gizmos.DrawLine(v2_3, v0_3);
            Gizmos.DrawLine(v0_3, Middle(v0_3, v1_3, v2_3));
            Gizmos.DrawLine(v1_3, Middle(v0_3, v1_3, v2_3));
            Gizmos.DrawLine(v2_3, Middle(v0_3, v1_3, v2_3));
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(v0_3, v2_3);
            Gizmos.DrawLine(v2_3, v3_3);
            Gizmos.DrawLine(v3_3, v0_3);
            Gizmos.DrawLine(v0_3, Middle(v0_3, v2_3, v3_3));
            Gizmos.DrawLine(v2_3, Middle(v0_3, v2_3, v3_3));
            Gizmos.DrawLine(v3_3, Middle(v0_3, v2_3, v3_3));
        }
    }
}