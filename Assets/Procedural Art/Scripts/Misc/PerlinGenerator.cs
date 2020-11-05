using UnityEngine;

public static class PerlinGenerator {
    public static Color[] Generate(int textureSize, float frequency, float seed, bool separateRGB = false, bool alpha = false) {
        var colors = new Color[textureSize * textureSize];
        for (int x = 0; x < textureSize; x++) {
            for (int y = 0; y < textureSize; y++) {
                float r, g, b;
                var a = 1f;
                
                r = Mathf.PerlinNoise(seed + (x + 0.01f) / frequency, seed + (y + 0.01f) / frequency);
                g = b = r;
                
                if (separateRGB) g = Mathf.PerlinNoise(seed + seed + (x + 0.01f) / frequency, seed + (y + 0.01f) / frequency);
                if (separateRGB) b = Mathf.PerlinNoise(seed + (x + 0.01f) / frequency, seed + seed + (y + 0.01f) / frequency);
                if (alpha) a = Mathf.PerlinNoise(seed + seed + (x + 0.01f) / frequency, seed + seed + (y + 0.01f) / frequency);
                colors[x * textureSize + y] = new Color(r, g, b, a);
            }
        }

        return colors;
    }
}