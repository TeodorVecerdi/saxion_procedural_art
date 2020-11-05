using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ExecuteInEditMode]
public class PerlinMapGenerator : MonoBehaviour {
    public int TextureSize = 1024;
    public float Frequency = 4f;
    public float Seed = 0;
    public bool SeparateRGB;
    public bool Alpha;
    [HideInInspector] public Texture2D texture;

    public void Generate() {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, true);
        texture.alphaIsTransparency = true;
        var colors = new Color[TextureSize * TextureSize];
        for (int x = 0; x < TextureSize; x++) {
            for (int y = 0; y < TextureSize; y++) {
                float r, g, b;
                var a = 1f;
                r = g = b = Mathf.PerlinNoise(Seed + (x + 0.01f) / Frequency, Seed + (y + 0.01f) / Frequency);
                
                if (SeparateRGB) g = Mathf.PerlinNoise(Seed + Seed + (x + 0.01f) / Frequency, Seed + (y + 0.01f) / Frequency);
                if (SeparateRGB) b = Mathf.PerlinNoise(Seed + (x + 0.01f) / Frequency, Seed + Seed + (y + 0.01f) / Frequency);
                if (Alpha) a = Mathf.PerlinNoise(Seed + Seed + (x + 0.01f) / Frequency, Seed + Seed + (y + 0.01f) / Frequency);

                // Debug.Log($"rgba = {r} {g} {b} {a}");
                var vec3 = new Vector4(r, g, b, a);

                // vec3.Normalize();
                colors[x * TextureSize + y] = new Color(r, g, b, a);
            }
        }

        texture.SetPixels(colors);
        texture.Apply(true, false);
        this.texture = texture;
    }
}

[CustomEditor(typeof(PerlinMapGenerator))]
public class PerlinMapGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        var generator = target as PerlinMapGenerator;
        if (GUILayout.Button("Generate Seed")) {
            generator.Seed = Random.Range(0f, (float) (1 << 10));
        }

        if (GUILayout.Button("Generate Image")) {
            generator.Generate();
        }

        if (generator.texture != null) {
            GUILayout.Space(6);
            var rect = GUILayoutUtility.GetAspectRect(1f, GUIStyle.none);

            // rect.height = rect.width;
            GUI.DrawTexture(rect, generator.texture);
            if (GUILayout.Button("Save As...")) {
                var savePath = EditorUtility.SaveFilePanelInProject("Save As...", "NoiseTexture", "png", "", Application.dataPath);
                savePath = savePath.Replace(Application.dataPath, "Assets");
                if (!string.IsNullOrEmpty(savePath)) {
                    var bytes = generator.texture.EncodeToPNG();
                    File.WriteAllBytes(savePath, bytes);
                    AssetDatabase.ImportAsset(savePath);
                }
            }
        }
    }
}