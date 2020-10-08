using System;
using UnityEngine;

[ExecuteInEditMode]
public class GlobalSettings : MonoBehaviour {
    private static GlobalSettings instance;
    public static GlobalSettings Instance {
        get {
            if (instance != null)
                return instance;
            var resources = Resources.FindObjectsOfTypeAll<GlobalSettings>();
            Debug.Assert(resources.Length == 1);
            
            resources[0].OnEnable();
            return instance;
        }
    }

    public float GridSize = 1f;
    public int GridSubdivisionsMinor = 4;
    public float GridSizeMinor => GridSize / GridSubdivisionsMinor;

    private void OnEnable() {
        instance = this;
    }
}